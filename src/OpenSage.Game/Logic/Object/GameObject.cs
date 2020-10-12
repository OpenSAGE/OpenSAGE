using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Map;
using OpenSage.DataStructures;
using OpenSage.Diagnostics;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui.ControlBar;
using OpenSage.Logic.Object.Helpers;
using OpenSage.Mathematics;
using OpenSage.Mathematics.FixedMath;
using OpenSage.Terrain;

namespace OpenSage.Logic.Object
{
    [DebuggerDisplay("[Object:{Definition.Name} ({Owner})]")]
    public sealed class GameObject : DisposableBase, IInspectable, IHasCollider
    {
        internal static GameObject FromMapObject(
            MapObject mapObject,
            AssetStore assetStore,
            GameObjectCollection gameObjects,
            HeightMap heightMap,
            bool useRotationAnchorOffset = true,
            in float? overwriteAngle = 0.0f,
            IReadOnlyList<Team> teams = null)
        {
            var gameObject = gameObjects.Add(mapObject.TypeName);

            // TODO: Is there any valid case where we'd want to return null instead of throwing an exception?
            if (gameObject == null)
            {
                return null;
            }

            if (gameObject.Body != null)
            {
                var healthMultiplier = mapObject.Properties.TryGetValue("objectInitialHealth", out var health)
                    ? (uint) health.Value / 100.0f
                    : 1.0f;

                gameObject.Body.SetInitialHealth(healthMultiplier);
            }

            if (mapObject.Properties.TryGetValue("objectName", out var objectName))
            {
                gameObject.Name = (string) objectName.Value;
            }

            if (teams != null)
            {
                if (mapObject.Properties.TryGetValue("originalOwner", out var teamName))
                {
                    var name = (string) teamName.Value;
                    if (name.Contains('/'))
                    {
                        name = name.Split('/')[1];
                    }
                    var team = teams.FirstOrDefault(t => t.Name == name);
                    gameObject.Team = team;
                    gameObject.Owner = team?.Owner;
                }
            }

            if (mapObject.Properties.TryGetValue("objectSelectable", out var selectable))
            {
                gameObject.IsSelectable = (bool) selectable.Value;
            }

            // TODO: handle "align to terrain" property
            var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, overwriteAngle ?? mapObject.Angle);
            var rotationOffset = useRotationAnchorOffset
                ? Vector4.Transform(new Vector4(gameObject.Definition.RotationAnchorOffset.X, gameObject.Definition.RotationAnchorOffset.Y, 0.0f, 1.0f), rotation)
                : Vector4.UnitW;
            var position = mapObject.Position + rotationOffset.ToVector3();
            var height = heightMap.GetHeight(position.X, position.Y) + mapObject.Position.Z;
            gameObject.UpdateTransform(new Vector3(position.X, position.Y, height), rotation,
                gameObject.Definition.Scale);

            if (gameObject.Definition.IsBridge)
            {
                BridgeTowers.CreateForLandmarkBridge(assetStore, gameObjects, gameObject, mapObject);
            }

            if (gameObject.Definition.KindOf.Get(ObjectKinds.Structure))
            {
                gameObject.BuildProgress = 1.0f;
                gameObject._gameContext.Navigation.UpdateAreaPassability(gameObject, false);
            }

            if (gameObject.Definition.KindOf.Get(ObjectKinds.Horde))
            {
                gameObject.FindBehavior<HordeContainBehavior>().Unpack();
            }

            return gameObject;
        }

        internal void CopyModelConditionFlags(BitArray<ModelConditionFlag> newFlags)
        {
            ModelConditionFlags.CopyFrom(newFlags);
        }

        private readonly Dictionary<string, BehaviorModule> _tagToModuleLookup;

        private readonly GameContext _gameContext;

        internal GameContext GameContext => _gameContext;

        private readonly BehaviorUpdateContext _behaviorUpdateContext;

        private readonly GameObject _rallyPointMarker;

        private BodyDamageType _bodyDamageType = BodyDamageType.Pristine;

        public readonly ObjectDefinition Definition;

        private readonly Transform _transform;
        public readonly Transform ModelTransform;

        private void UpdateCollider()
        {
            Collider.Update(_transform);
            _gameContext.Quadtree.Update(this);
        }

        public Transform Transform => _transform;
        public float Yaw => _transform.Yaw;
        public Vector3 EulerAngles => _transform.EulerAngles;
        public Vector3 LookDirection => _transform.LookDirection;
        public Vector3 Translation => _transform.Translation;
        public Quaternion Rotation => _transform.Rotation;
        public Matrix4x4 TransformMatrix => _transform.Matrix;

        public void SetTransformMatrix(Matrix4x4 matrix)
        {
            _transform.Matrix = matrix;
            UpdateCollider();
        }

        public void SetTranslation(Vector3 translation)
        {
            _transform.Translation = translation;
            UpdateCollider();
        }

        public void SetRotation(Quaternion rotation)
        {
            _transform.Rotation = rotation;
            UpdateCollider();
        }

        public void SetScale(float scale)
        {
            _transform.Scale = scale;
            UpdateCollider();
        }

        public void UpdateTransform(in Vector3? translation = null, in Quaternion? rotation = null, float scale = 1.0f)
        {
            _transform.Translation = translation ?? _transform.Translation;
            _transform.Rotation = rotation ?? _transform.Rotation;
            _transform.Scale = scale;
            UpdateCollider();
        }

        public readonly IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates;

        public readonly BitArray<ModelConditionFlag> ModelConditionFlags;

        // Doing this with a field and a property instead of an auto-property allows us to have a read-only public interface,
        // while simultaneously supporting fast (non-allocating) iteration when accessing the list within the class.
        public IReadOnlyList<DrawModule> DrawModules => _drawModules;
        private readonly List<DrawModule> _drawModules;

        public IReadOnlyList<BehaviorModule> BehaviorModules => _behaviorModules;
        private readonly List<BehaviorModule> _behaviorModules;

        public readonly BodyModule Body;

        public Collider Collider { get; }

        public float VerticalOffset;

        public Player Owner { get; internal set; }

        public GameObject ParentHorde { get; set; }

        public int Supply { get; set; }

        public List<string> HiddenSubObjects;

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != null)
                {
                    throw new InvalidOperationException("An object's name cannot change once it's been set.");
                }

                _name = value ?? throw new ArgumentNullException(nameof(value));
                Parent.AddNameLookup(this);
            }
        }

        string IInspectable.Name => "GameObject";

        public Team Team { get; set; }

        public bool IsSelectable { get; private set; }
        public bool IsProjectile { get; private set; } = false;
        public bool CanAttack { get; private set; }

        public bool IsSelected { get; set; }

        public Vector3? RallyPoint { get; set; }

        internal Weapon CurrentWeapon { get; private set; }

        private TimeSpan ConstructionStart { get; set; }

        public float BuildProgress { get; set; }

        public bool Destroyed { get; set; }

        public bool IsDamaged
        {
            get
            {
                var healthPercentage = (float) Body.HealthPercentage;
                var damagedThreshold = _gameContext.AssetLoadContext.AssetStore.GameData.Current.UnitDamagedThreshold;
                return healthPercentage <= damagedThreshold;
            }
        }

        public float Speed { get; set; }
        public float SteeringWheelsYaw { get; set; }
        public float Lift { get; set; }

        public bool IsPlacementPreview { get; set; }

        public bool IsPlacementInvalid { get; set; }

        public GameObjectCollection Parent { get; private set; }

        public AIUpdate AIUpdate { get; }
        public ProductionUpdate ProductionUpdate { get; }

        public List<UpgradeTemplate> Upgrades { get; }
        public List<UpgradeTemplate> ConflictingUpgrades { get; }

        public int ExperienceValue { get; private set; }
        internal float ExperienceMultiplier { get; set; }

        public int EnergyProduction { get; internal set; }

        // TODO
        public ArmorTemplateSet CurrentArmorSet => Definition.ArmorSets.Values.First();

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal GameObject(
            ObjectDefinition objectDefinition,
            GameContext gameContext,
            Player owner,
            GameObjectCollection parent)
        {
            Definition = objectDefinition ?? throw new ArgumentNullException(nameof(objectDefinition));

            _tagToModuleLookup = new Dictionary<string, BehaviorModule>();
            _gameContext = gameContext;
            Owner = owner;
            Parent = parent;

            _behaviorUpdateContext = new BehaviorUpdateContext(gameContext, this, TimeInterval.Zero);

            SetDefaultWeapon();
            _transform = Transform.CreateIdentity();
            ModelTransform = Transform.CreateIdentity();
            _transform.Scale = Definition.Scale;

            var drawModules = new List<DrawModule>();
            foreach (var drawData in objectDefinition.Draws.Values)
            {
                var drawModule = AddDisposable(drawData.CreateDrawModule(this, gameContext));
                if (drawModule != null)
                {
                    // TODO: This will never be null once we've implemented all the draw modules.
                    drawModules.Add(drawModule);
                }
            }
            _drawModules = drawModules;

            var behaviors = new List<BehaviorModule>();

            void AddBehavior(string tag, BehaviorModule behavior)
            {
                behaviors.Add(behavior);
                _tagToModuleLookup.Add(tag, behavior);
            }

            AddBehavior("ModuleTag_SMCHelper", new ObjectSpecialModelConditionHelper());

            // TODO: This shouldn't be added to all objects. I don't know what the rule is.
            // Maybe KindOf = CAN_ATTACK ?
            AddBehavior("ModuleTag_DefectionHelper", new ObjectDefectionHelper());

            // TODO: This shouldn't be added to all objects. I don't know what the rule is.
            // Probably only those with weapons.
            AddBehavior("ModuleTag_WeaponStatusHelper", new ObjectWeaponStatusHelper());

            // TODO: This shouldn't be added to all objects. I don't know what the rule is.
            // Probably only those with weapons.
            AddBehavior("ModuleTag_FiringTrackerHelper", new ObjectFiringTrackerHelper());

            foreach (var behaviorData in objectDefinition.Behaviors.Values)
            {
                var module = AddDisposable(behaviorData.CreateModule(this, gameContext));
                if (module != null)
                {
                    // TODO: This will never be null once we've implemented all the behaviors.
                    AddBehavior(behaviorData.Tag, module);
                }
            }
            _behaviorModules = behaviors;

            ProductionUpdate = FindBehavior<ProductionUpdate>();

            ModelConditionFlags = new BitArray<ModelConditionFlag>();

            Body = AddDisposable(objectDefinition.Body.CreateBodyModule(this));
            _tagToModuleLookup.Add(objectDefinition.Body.Tag, Body);

            if (objectDefinition.AIUpdate != null)
            {
                AIUpdate = AddDisposable(objectDefinition.AIUpdate.CreateAIUpdate(this));
                _tagToModuleLookup.Add(objectDefinition.AIUpdate.Tag, AIUpdate);
            }

            Collider = Collider.Create(objectDefinition, _transform);

            ModelConditionStates = drawModules
                .SelectMany(x => x.ModelConditionStates)
                .Distinct()
                .OrderBy(x => x.NumBitsSet)
                .ToList();

            IsSelectable = Definition.KindOf.Get(ObjectKinds.Selectable);
            CanAttack = Definition.KindOf.Get(ObjectKinds.CanAttack);

            if (Definition.KindOf.Get(ObjectKinds.AutoRallyPoint))
            {
                var rpMarkerDef = gameContext.AssetLoadContext.AssetStore.ObjectDefinitions.GetByName("RallyPointMarker");
                _rallyPointMarker = AddDisposable(new GameObject(rpMarkerDef, gameContext, owner, parent));
            }

            if (Definition.KindOf.Get(ObjectKinds.Projectile))
            {
                IsProjectile = true;
            }

            if (Definition.KindOf.Get(ObjectKinds.Tree))
            {
                Supply = Definition.SupplyOverride > 0 ? Definition.SupplyOverride : gameContext.AssetLoadContext.AssetStore.GameData.Current.SupplyBoxesPerTree;
            }

            HiddenSubObjects = new List<string>();
            Upgrades = new List<UpgradeTemplate>();
            ConflictingUpgrades = new List<UpgradeTemplate>();

            ExperienceMultiplier = 1.0f;
            ExperienceValue = 0;
        }

        // TODO: This probably shouldn't be here.
        public Matrix4x4? GetWeaponFireFXBoneTransform(WeaponSlot slot, int index)
        {
            foreach (var drawModule in _drawModules)
            {
                var fireFXBone = drawModule.GetWeaponFireFXBone(slot);
                if (fireFXBone != null)
                {
                    var (modelInstance, bone) = drawModule.FindBone(fireFXBone + (index + 1).ToString("D2"));
                    if (bone != null)
                    {
                        return modelInstance.AbsoluteBoneTransforms[bone.Index];
                    }
                    break;
                }
            }

            return null;
        }

        // TODO: This probably shouldn't be here.
        public Matrix4x4? GetWeaponLaunchBoneTransform(WeaponSlot slot, int index)
        {
            foreach (var drawModule in _drawModules)
            {
                var fireFXBone = drawModule.GetWeaponLaunchBone(slot);
                if (fireFXBone != null)
                {
                    var (modelInstance, bone) = drawModule.FindBone(fireFXBone + (index + 1).ToString("D2"));
                    if (bone != null)
                    {
                        return modelInstance.AbsoluteBoneTransforms[bone.Index];
                    }
                    break;
                }
            }

            return null;
        }

        public (ModelInstance modelInstance, ModelBone bone) FindBone(string boneName)
        {
            foreach (var drawModule in _drawModules)
            {
                var (modelInstance, bone) = drawModule.FindBone(boneName);
                if (bone != null)
                {
                    return (modelInstance, bone);
                }
            }

            return (null, null);
        }

        internal void LogicTick(ulong frame, in TimeInterval time)
        {
            if (Destroyed)
            {
                return;
            }

            // TODO: Should there be a BeforeLogicTick where we update this?
            _behaviorUpdateContext.UpdateTime(time);

            if (ModelConditionFlags.Get(ModelConditionFlag.Attacking))
            {
                CurrentWeapon?.LogicTick(time);
            }

            AIUpdate?.Update(_behaviorUpdateContext);

            foreach (var behavior in _behaviorModules)
            {
                behavior.Update(_behaviorUpdateContext);
            }
        }

        public bool CanRecruitHero(ObjectDefinition definition, int maxCount)
        {
            var count = 0;
            foreach (var obj in Parent.Items)
            {
                if (obj.Definition == definition && obj.Owner == Owner)
                {
                    if (++count >= maxCount)
                    {
                        return false;
                    }
                }
            }
            return count < maxCount;
        }

        public bool CanProduceObject(ObjectDefinition definition)
        {
            return ProductionUpdate?.CanProduceObject(definition) ?? true;
        }

        internal bool Intersects(GameObject other)
        {
            if (Collider == null || other.Collider == null)
            {
                return false;
            }

            if (Definition.KindOf.Get(ObjectKinds.Immobile)
                && other.Definition.KindOf.Get(ObjectKinds.Immobile))
            {
                return false;
            }

            return Collider.Intersects(other.Collider);
        }

        internal void DoCollide(GameObject collidingObject, in TimeInterval time)
        {
            // TODO: Can we avoid updating this every time?
            _behaviorUpdateContext.UpdateTime(time);

            foreach (var behavior in _behaviorModules)
            {
                behavior.OnCollide(_behaviorUpdateContext, collidingObject);
            }
        }

        private void HandleConstruction(in TimeInterval gameTime)
        {
            // Check if the unit is being constructed
            if (IsBeingConstructed())
            {
                var passed = gameTime.TotalTime - ConstructionStart;
                BuildProgress = Math.Clamp((float) passed.TotalSeconds / Definition.BuildTime, 0.0f, 1.0f);

                if (BuildProgress >= 1.0f)
                {
                    FinishConstruction();
                }
            }
        }

        internal Vector3 ToWorldspace(in Vector3 localPos)
        {
            var worldPos = Vector4.Transform(new Vector4(localPos, 1.0f), _transform.Matrix);
            return new Vector3(worldPos.X, worldPos.Y, worldPos.Z);
        }

        internal Transform ToWorldspace(in Transform localPos)
        {
            var worldPos = localPos.Matrix * _transform.Matrix;
            return new Transform(worldPos);
        }

        internal T FindBehavior<T>()
        {
            // TODO: Cache this?
            return _behaviorModules.OfType<T>().FirstOrDefault();
        }

        internal IEnumerable<T> FindBehaviors<T>()
        {
            // TODO: Cache this?
            return _behaviorModules.OfType<T>();
        }

        public bool UpgradeAvailable(UpgradeTemplate upgrade)
        {
            if (upgrade == null)
            {
                return false;
            }

            return upgrade.Type == UpgradeType.Player ? Owner.Upgrades.Contains(upgrade) : Upgrades.Contains(upgrade);
        }

        public bool ConflictingUpgradeAvailable(UpgradeTemplate upgrade)
        {
            if (upgrade == null || upgrade.Type == UpgradeType.Player)
            {
                return false; // TODO: player invalid upgrades?
            }
            return ConflictingUpgrades.Contains(upgrade);
        }

        internal void StartConstruction(in TimeInterval gameTime)
        {
            if (Definition.KindOf.Get(ObjectKinds.Structure))
            {
                ModelConditionFlags.SetAll(false);
                ModelConditionFlags.Set(ModelConditionFlag.ActivelyBeingConstructed, true);
                ModelConditionFlags.Set(ModelConditionFlag.AwaitingConstruction, true);
                ModelConditionFlags.Set(ModelConditionFlag.PartiallyConstructed, true);
                ConstructionStart = gameTime.TotalTime;
            }

            _gameContext.Navigation.UpdateAreaPassability(this, false);
        }

        internal void FinishConstruction()
        {
            ClearModelConditionFlags();

            var spawnBehavior = FindBehavior<SpawnBehavior>();
            if (spawnBehavior != null)
            {
                spawnBehavior.SpawnInitial();
            }

            EnergyProduction += Definition.EnergyProduction;
        }

        public bool IsBeingConstructed()
        {
            return ModelConditionFlags.Get(ModelConditionFlag.ActivelyBeingConstructed) ||
                   ModelConditionFlags.Get(ModelConditionFlag.AwaitingConstruction) ||
                   ModelConditionFlags.Get(ModelConditionFlag.PartiallyConstructed);
        }

        internal void UpdateDamageFlags(Fix64 healthPercentage)
        {
            // TODO: SoundOnDamaged
            // TODO: SoundOnReallyDamaged
            // TODO: SoundDie
            // TODO: TransitionDamageFX

            var oldDamageType = _bodyDamageType;

            if (healthPercentage < (Fix64) GameContext.AssetLoadContext.AssetStore.GameData.Current.UnitReallyDamagedThreshold)
            {
                ModelConditionFlags.Set(ModelConditionFlag.ReallyDamaged, true);
                ModelConditionFlags.Set(ModelConditionFlag.Damaged, false);
                _bodyDamageType = BodyDamageType.ReallyDamaged;
            }
            else if (healthPercentage < (Fix64) GameContext.AssetLoadContext.AssetStore.GameData.Current.UnitDamagedThreshold)
            {
                ModelConditionFlags.Set(ModelConditionFlag.ReallyDamaged, false);
                ModelConditionFlags.Set(ModelConditionFlag.Damaged, true);
                _bodyDamageType = BodyDamageType.Damaged;
            }
            else
            {
                ModelConditionFlags.Set(ModelConditionFlag.ReallyDamaged, false);
                ModelConditionFlags.Set(ModelConditionFlag.Damaged, false);
                _bodyDamageType = BodyDamageType.Pristine;
            }

            if (oldDamageType != _bodyDamageType)
            {
                foreach (var behavior in _behaviorModules)
                {
                    behavior.OnDamageStateChanged(
                        _behaviorUpdateContext,
                        oldDamageType,
                        _bodyDamageType);
                }
            }
        }

        internal void LocalLogicTick(in TimeInterval gameTime, float tickT, HeightMap heightMap)
        {
            if (Destroyed)
            {
                return;
            }

            HandleConstruction(gameTime);

            _rallyPointMarker?.LocalLogicTick(gameTime, tickT, heightMap);
        }

        internal void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime)
        {
            if (Destroyed || ModelConditionFlags.Get(ModelConditionFlag.Sold))
            {
                return;
            }

            UpdateDrawModuleConditionStates();

            // Update all draw modules
            foreach (var drawModule in _drawModules)
            {
                drawModule.Update(gameTime);
            }

            // This must be done after processing anything that might update this object's transform.
            var worldMatrix = ModelTransform.Matrix * _transform.Matrix;
            foreach (var drawModule in _drawModules)
            {
                drawModule.SetWorldMatrix(worldMatrix);
            }

            var castsShadow = false;
            switch (Definition.Shadow)
            {
                case ObjectShadowType.ShadowVolume:
                case ObjectShadowType.ShadowVolumeNew:
                    castsShadow = true;
                    break;
            }

            var renderItemConstantsPS = new MeshShaderResources.RenderItemConstantsPS
            {
                HouseColor = Owner.Color.ToVector3(),
                Opacity = IsPlacementPreview ? 0.7f : 1.0f,
                TintColor = IsPlacementInvalid ? new Vector3(1, 0.3f, 0.3f) : Vector3.One,
            };

            foreach (var drawModule in _drawModules)
            {
                drawModule.BuildRenderList(
                    renderList,
                    camera,
                    castsShadow,
                    renderItemConstantsPS,
                    HiddenSubObjects);
            }

            if ((IsSelected || IsPlacementPreview) && _rallyPointMarker != null && RallyPoint != null)
            {
                _rallyPointMarker._transform.Translation = RallyPoint.Value;

                // TODO: check if this should be drawn with transparency?
                _rallyPointMarker.BuildRenderList(renderList, camera, gameTime);
            }
        }

        public void ClearModelConditionFlags()
        {
            ModelConditionFlags.SetAll(false);
        }

        private void UpdateDrawModuleConditionStates()
        {
            // TODO: Let each drawable use the appropriate TransitionState between ConditionStates.
            foreach (var drawModule in _drawModules)
            {
                drawModule.UpdateConditionState(ModelConditionFlags, _gameContext.Random);
            }
        }

        public void OnLocalSelect(AudioSystem gameAudio)
        {
            var audioEvent = Definition.VoiceSelect?.Value;
            if (audioEvent != null && ParentHorde == null)
            {
                gameAudio.PlayAudioEvent(this, audioEvent);
            }
        }

        //TODO: make sure to play the correct voice event (e.g. VoiceMoveGroup etc.)
        public void OnLocalMove(AudioSystem gameAudio)
        {
            var audioEvent = Definition.VoiceMove?.Value;
            if (audioEvent != null && ParentHorde == null)
            {
                gameAudio.PlayAudioEvent(this, audioEvent);
            }
        }

        //TODO: use the target to figure out which sound triggers
        public void OnLocalAttack(AudioSystem gameAudio)
        {
            var audioEvent = Definition.VoiceAttack?.Value;
            if (audioEvent != null && ParentHorde == null)
            {
                gameAudio.PlayAudioEvent(this, audioEvent);
            }
        }

        internal void SetDefaultWeapon()
        {
            // TODO: we currently always pick the weapon without any conditions.
            var weaponSet = Definition.WeaponSets.Values.FirstOrDefault(x => x.Conditions?.AnyBitSet == false);
            SetWeaponSet(weaponSet);
        }

        internal void SetWeaponSet(WeaponTemplateSet weaponSet)
        {
            if (weaponSet != null)
            {
                var aiUpdate = Definition.Behaviors.Values.OfType<AIUpdateModuleData>().FirstOrDefault();
                var weaponSetUpdateData = weaponSet.ToWeaponSetUpdate(aiUpdate);

                // Happens for BFME structures
                if (weaponSetUpdateData.WeaponSlotHardpoints.Count == 0 &&
                   weaponSetUpdateData.WeaponSlotTurrets.Count == 0)
                {
                    return;
                }

                // TODO: This weapon selection is all wrong, and should be done in WeaponSetUpdate.

                var weaponSlotHardpoint = weaponSetUpdateData.WeaponSlotHardpoints.Count > 0
                    ? weaponSetUpdateData.WeaponSlotHardpoints[0]
                    : weaponSetUpdateData.WeaponSlotTurrets[0];

                var weaponTemplate = weaponSlotHardpoint.Weapons[0].Template.Value;
                SetWeapon(weaponTemplate);
            }
            else
            {
                CurrentWeapon = null;
            }
        }

        internal void SetWeapon(WeaponTemplate weaponTemplate)
        {
            if (weaponTemplate != null)
            {
                CurrentWeapon = new Weapon(
                this,
                weaponTemplate,
                0,
                WeaponSlot.Primary,
                _gameContext);
            }
            else
            {
                CurrentWeapon = null;
            }
        }

        public bool CanEnqueue(CommandButton button)
        {
            if (button.Object != null && button.Object.Value != null)
            {
                return CanConstructUnit(button.Object.Value);
            }
            if (button.Upgrade != null && button.Upgrade.Value != null)
            {
                return CanEnqueueUpgrade(button.Upgrade.Value);
            }
            return true;
        }

        public bool CanConstructUnit(ObjectDefinition objectDefinition)
        {
            return objectDefinition != null
                && Owner.CanProduceObject(Parent, objectDefinition)
                && CanProduceObject(objectDefinition);
        }

        public bool CanEnqueueUpgrade(UpgradeTemplate upgrade)
        {
            if (upgrade == null || ProductionUpdate == null)
            {
                return false;
            }

            var userHasEnoughMoney = Owner.Money >= upgrade.BuildCost;
            var hasQueuedUpgrade = ProductionUpdate.ProductionQueue.Any(x => x.UpgradeDefinition == upgrade);
            var canEnqueue = ProductionUpdate.CanEnque();
            var hasUpgrade = UpgradeAvailable(upgrade);
            var upgradeIsInvalid = ConflictingUpgradeAvailable(upgrade);

            return userHasEnoughMoney && canEnqueue && !hasQueuedUpgrade && !hasUpgrade && !upgradeIsInvalid;
        }

        public void Upgrade(UpgradeTemplate upgrade)
        {
            // TODO: do something
            if (upgrade.Type == UpgradeType.Object)
            {
                Upgrades.Add(upgrade);
            }
            else if(upgrade.Type == UpgradeType.Player)
            {
               Owner.Upgrades.Add(upgrade);
            }
            else
            {
                throw new InvalidOperationException("This should not happen");
            }
        }

        internal void Kill(DeathType deathType, TimeInterval time)
        {
            Body.DoDamage(DamageType.Unresistable, Body.Health, deathType, time);
        }

        internal void Die(DeathType deathType, TimeInterval time)
        {
            Logger.Info("Object dying " + deathType);

            ModelConditionFlags.Set(ModelConditionFlag.ReallyDamaged, false);
            ModelConditionFlags.Set(ModelConditionFlag.Damaged, false);

            // TODO: Can we avoid updating this every time?
            _behaviorUpdateContext.UpdateTime(time);

            // If there are multiple SlowDeathBehavior modules,
            // we need to use ProbabilityModifier to choose between them.
            var slowDeathBehaviors = FindBehaviors<SlowDeathBehavior>()
                .Where(x => x.IsApplicable(deathType))
                .ToList();
            if (slowDeathBehaviors.Count > 1)
            {
                var sumProbabilityModifiers = slowDeathBehaviors.Sum(x => x.ProbabilityModifier);
                var random = _gameContext.Random.Next(sumProbabilityModifiers);
                var cumulative = 0;
                foreach (var deathBehavior in slowDeathBehaviors)
                {
                    cumulative += deathBehavior.ProbabilityModifier;
                    if (random < cumulative)
                    {
                        deathBehavior.OnDie(_behaviorUpdateContext, deathType);
                        return;
                    }
                }
                throw new InvalidOperationException();
            }

            foreach (var dieModule in _behaviorModules)
            {
                dieModule.OnDie(_behaviorUpdateContext, deathType);
            }
        }

        internal void Destroy()
        {
            Destroyed = true;
        }

        internal void GainExperience(int experience)
        {
            ExperienceValue = (int) (ExperienceMultiplier * experience);
        }

        internal BehaviorModule GetModuleByTag(string tag)
        {
            return _tagToModuleLookup[tag];
        }

        void IInspectable.DrawInspector()
        {
            if (ImGui.Button("Bring into view"))
            {
                _gameContext.Scene3D.CameraController.GoToObject(this);
            }

            ImGui.SameLine();

            if (ImGui.Button("Select"))
            {
                _gameContext.Scene3D.LocalPlayer.SelectUnits(new[] { this });
            }

            ImGui.SameLine();

            if (ImGui.Button("Kill"))
            {
                // TODO: Time isn't right.
                Kill(DeathType.Exploded, _gameContext.Scene3D.Game.MapTime);
            }

            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.LabelText("DisplayName", Definition.DisplayName);

                var translation = _transform.Translation;
                if (ImGui.DragFloat3("Position", ref translation))
                {
                    _transform.Translation = translation;
                }

                // TODO: Make this editable
                ImGui.LabelText("ModelConditionFlags", ModelConditionFlags.DisplayName);

                var speed = Speed;
                if (ImGui.InputFloat("Speed", ref speed))
                {
                    Speed = speed;
                }

                var lift = Lift;
                if (ImGui.InputFloat("Lift", ref lift))
                {
                    Lift = lift;
                }
            }

            foreach (var drawModule in DrawModules)
            {
                if (ImGui.CollapsingHeader(drawModule.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    drawModule.DrawInspector();
                }
            }

            if (ImGui.CollapsingHeader(Body.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen))
            {
                Body.DrawInspector();
            }

            if (CurrentWeapon != null)
            {
                var weapon = CurrentWeapon;
                if (ImGui.CollapsingHeader("Weapon", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    weapon.DrawInspector();
                }
            }

            foreach (var behaviorModule in BehaviorModules)
            {
                if (ImGui.CollapsingHeader(behaviorModule.GetType().Name, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    behaviorModule.DrawInspector();
                }
            }
        }
    }
}
