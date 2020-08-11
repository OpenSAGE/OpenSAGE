using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Map;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Logic.Object.Helpers;
using OpenSage.Mathematics;
using OpenSage.Mathematics.FixedMath;
using OpenSage.Terrain;

namespace OpenSage.Logic.Object
{
    [DebuggerDisplay("[Object:{Definition.Name} ({Owner})]")]
    public sealed class GameObject : DisposableBase
    {

        internal static GameObject FromMapObject(
            MapObject mapObject,
            AssetStore assetStore,
            GameObjectCollection gameObjects,
            HeightMap heightMap,
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
            var rotationOffset = Vector4.Transform(new Vector4(gameObject.Definition.RotationAnchorOffset.X, gameObject.Definition.RotationAnchorOffset.Y, 0.0f, 1.0f), rotation);
            var position = mapObject.Position + rotationOffset.ToVector3();
            var height = heightMap.GetHeight(position.X, position.Y) + mapObject.Position.Z;
            gameObject.Transform.Translation = new Vector3(position.X, position.Y, height);
            gameObject.Transform.Rotation = rotation;
            gameObject.Transform.Scale = gameObject.Definition.Scale;

            if (gameObject.Definition.IsBridge)
            {
                BridgeTowers.CreateForLandmarkBridge(assetStore, gameObjects, gameObject, mapObject);
            }

            if (gameObject.Definition.KindOf.Get(ObjectKinds.Structure))
            {
                gameObject._gameContext.Navigation.UpdateAreaPassability(gameObject, false);
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

        private readonly GameObject _rallyPointMarker;

        private BodyDamageType _bodyDamageType = BodyDamageType.Pristine;

        public readonly ObjectDefinition Definition;

        public readonly Transform Transform;

        public readonly IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates;

        public readonly BitArray<ModelConditionFlag> ModelConditionFlags;

        public readonly IReadOnlyList<DrawModule> DrawModules;

        public readonly IReadOnlyList<BehaviorModule> BehaviorModules;

        public readonly BodyModule Body;

        public readonly Collider Collider;

        public float VerticalOffset;

        public Player Owner { get; internal set; }

        private string _name;

        public string Name
        {
            get
            {
                return _name;
            }

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
        public float Yaw { get; set; }
        public float Lift { get; set; }

        public bool IsPlacementPreview { get; set; }

        public bool IsPlacementInvalid { get; set; }

        public GameObjectCollection Parent { get; private set; }

        public AIUpdate AIUpdate { get; }
        public ProductionUpdate ProductionUpdate { get; }

        public List<UpgradeTemplate> Upgrades { get; }

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
            if (objectDefinition == null)
            {
                throw new ArgumentNullException(nameof(objectDefinition));
            }

            _tagToModuleLookup = new Dictionary<string, BehaviorModule>();

            _gameContext = gameContext;

            Definition = objectDefinition;
            Owner = owner;
            Parent = parent;

            SetDefaultWeapon();
            Transform = Transform.CreateIdentity();
            Transform.Scale = objectDefinition.Scale;

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
            DrawModules = drawModules;

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

            foreach (var behaviorData in objectDefinition.Behaviors)
            {
                var module = AddDisposable(behaviorData.CreateModule(this, gameContext));
                if (module != null)
                {
                    // TODO: This will never be null once we've implemented all the behaviors.
                    AddBehavior(behaviorData.Tag, module);
                }
            }
            BehaviorModules = behaviors;

            ProductionUpdate = FindBehavior<ProductionUpdate>();

            ModelConditionFlags = new BitArray<ModelConditionFlag>();

            Body = AddDisposable(objectDefinition.Body.CreateBodyModule(this));
            _tagToModuleLookup.Add(objectDefinition.Body.Tag, Body);

            if (objectDefinition.AIUpdate != null)
            {
                AIUpdate = AddDisposable(objectDefinition.AIUpdate.CreateAIUpdate(this));
                _tagToModuleLookup.Add(objectDefinition.AIUpdate.Tag, AIUpdate);
            }

            Collider = Collider.Create(objectDefinition, Transform);

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

            Upgrades = new List<UpgradeTemplate>();

            ExperienceMultiplier = 1.0f;
            ExperienceValue = 0;
        }

        // TODO: This probably shouldn't be here.
        public Matrix4x4? GetWeaponFireFXBoneTransform(WeaponSlot slot, int index)
        {
            foreach (var drawModule in DrawModules)
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
            foreach (var drawModule in DrawModules)
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
            foreach (var drawModule in DrawModules)
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

            //if (ModelConditionFlags.Get(ModelConditionFlag.Attacking))
            {
                CurrentWeapon?.LogicTick(time);
            }

            // TODO: Don't create this every time.
            var behaviorUpdateContext = new BehaviorUpdateContext(
                _gameContext,
                this,
                time);

            AIUpdate?.Update(behaviorUpdateContext);

            foreach (var behavior in BehaviorModules)
            {
                behavior.Update(behaviorUpdateContext);
            }
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

            // TODO: Use more accurate collider/collider intersection.

            var thisBoundingArea = Collider.GetBoundingArea();
            var otherBoundingArea = other.Collider.GetBoundingArea();

            return thisBoundingArea.Intersects(otherBoundingArea);
        }

        internal void DoCollide(GameObject collidingObject, in TimeInterval time)
        {
            // TODO: Don't create this every time.
            var context = new BehaviorUpdateContext(
                _gameContext,
                this,
                time);

            foreach (var behavior in BehaviorModules)
            {
                behavior.OnCollide(context, collidingObject);
            }
        }

        private void HandleConstruction(in TimeInterval gameTime)
        {
            // Check if the unit is being constructed
            if (IsBeingConstructed())
            {
                var passed = gameTime.TotalTime - ConstructionStart;
                BuildProgress = Math.Clamp((float) passed.TotalSeconds / Definition.BuildTime, 0.0f, 1.0f);

                if (BuildProgress == 1.0f)
                {
                    FinishConstruction();
                }
            }
        }

        internal Vector3 ToWorldspace(in Vector3 localPos)
        {
            var worldPos = Vector4.Transform(new Vector4(localPos, 1.0f), Transform.Matrix);
            return new Vector3(worldPos.X, worldPos.Y, worldPos.Z);
        }

        internal T FindBehavior<T>()
        {
            // TODO: Cache this?
            return BehaviorModules.OfType<T>().FirstOrDefault();
        }

        internal IEnumerable<T> FindBehaviors<T>()
        {
            // TODO: Cache this?
            return BehaviorModules.OfType<T>();
        }

        internal void Spawn(ObjectDefinition objectDefinition)
        {
            var productionExit = FindBehavior<IProductionExit>();

            if (productionExit == null)
            {
                // If there's no IProductionExit behavior on this object, don't spawn anything.
                return;
            }

            var spawnedUnit = Parent.Add(objectDefinition, Owner);
            spawnedUnit.Transform.Rotation = Transform.Rotation;
            spawnedUnit.Transform.Translation = ToWorldspace(productionExit.GetUnitCreatePoint());

            // First go to the natural rally point
            var naturalRallyPoint = productionExit.GetNaturalRallyPoint();
            if (naturalRallyPoint.HasValue)
            {
                spawnedUnit.AIUpdate.AddTargetPoint(ToWorldspace(naturalRallyPoint.Value));
            }

            // Then go to the rally point if it exists
            if (RallyPoint.HasValue)
            {
                spawnedUnit.AIUpdate.AddTargetPoint(RallyPoint.Value);
            }
        }

        public bool UpgradeAvailable(UpgradeTemplate upgrade)
        {
            if (upgrade == null) return false;

            if(upgrade.Type == UpgradeType.Player)
            {
                return Owner.Upgrades.Contains(upgrade);
            }
            else
            {
                return Upgrades.Contains(upgrade);
            }
        }

        internal void StartConstruction(in TimeInterval gameTime)
        {
            if (Definition.KindOf == null) return;

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

            foreach (var behavior in Definition.Behaviors)
            {
                if (behavior is SpawnBehaviorModuleData spawnBehaviorModuleData)
                {
                    Spawn(spawnBehaviorModuleData.SpawnTemplate.Value);
                }
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
                var behaviorUpdateContext = new BehaviorUpdateContext(
                    _gameContext,
                    this,
                    new TimeInterval()); // TODO

                foreach (var behavior in BehaviorModules)
                {
                    behavior.OnDamageStateChanged(
                        behaviorUpdateContext,
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
            if (Destroyed)
            {
                return;
            }

            if (ModelConditionFlags.Get(ModelConditionFlag.Sold))
            {
                return;
            }

            // Update all draw modules
            UpdateDrawModuleConditionStates();
            foreach (var drawModule in DrawModules)
            {
                drawModule.Update(gameTime);
            }

            // This must be done after processing anything that might update this object's transform.
            var worldMatrix = Transform.Matrix;
            foreach (var drawModule in DrawModules)
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

            foreach (var drawModule in DrawModules)
            {
                drawModule.BuildRenderList(
                    renderList,
                    camera,
                    castsShadow,
                    renderItemConstantsPS);
            }

            if ((IsSelected || IsPlacementPreview) && _rallyPointMarker != null && RallyPoint != null)
            {
                _rallyPointMarker.Transform.Translation = RallyPoint.Value;

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
            foreach (var drawModule in DrawModules)
            {
                drawModule.UpdateConditionState(ModelConditionFlags);
            }
        }

        public void OnLocalSelect(AudioSystem gameAudio)
        {
            var audioEvent = Definition.VoiceSelect?.Value;
            if (audioEvent != null)
            {
                gameAudio.PlayAudioEvent(audioEvent);
            }
        }

        //TODO: make sure to play the correct voice event (e.g. VoiceMoveGroup etc.)
        public void OnLocalMove(AudioSystem gameAudio)
        {
            var audioEvent = Definition.VoiceMove?.Value;
            if (audioEvent != null)
            {
                gameAudio.PlayAudioEvent(audioEvent);
            }
        }

        //TODO: use the target to figure out which sound triggers
        public void OnLocalAttack(AudioSystem gameAudio)
        {
            var audioEvent = Definition.VoiceAttack?.Value;
            if (audioEvent != null)
            {
                gameAudio.PlayAudioEvent(audioEvent);
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
                var aiUpdate = Definition.Behaviors.OfType<AIUpdateModuleData>().FirstOrDefault();
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

            var behaviorUpdateContext = new BehaviorUpdateContext(
                _gameContext,
                this,
                time);

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
                        deathBehavior.OnDie(behaviorUpdateContext, deathType);
                        return;
                    }
                }
                throw new InvalidOperationException();
            }

            foreach (var dieModule in BehaviorModules)
            {
                dieModule.OnDie(behaviorUpdateContext, deathType);
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
    }
}
