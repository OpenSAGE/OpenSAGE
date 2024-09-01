using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Audio;
using OpenSage.Client;
using OpenSage.Data.Map;
using OpenSage.DataStructures;
using OpenSage.Diagnostics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.InGame;
using OpenSage.Logic.Object.Helpers;
using OpenSage.Mathematics;
using FixedMath.NET;
using OpenSage.Diagnostics.Util;
using OpenSage.Terrain;
using OpenSage.FileFormats;
using OpenSage.Logic.Object.Damage;

namespace OpenSage.Logic.Object
{
    [DebuggerDisplay("[Object:{Definition.Name} ({Owner})]")]
    public sealed class GameObject : Entity, IInspectable, ICollidable, IPersistableObject
    {
        internal static GameObject FromMapObject(
            MapObject mapObject,
            GameContext gameContext,
            bool useRotationAnchorOffset = true,
            in float? overwriteAngle = 0.0f)
        {
            TeamTemplate teamTemplate = null;
            if (mapObject.Properties.TryGetValue("originalOwner", out var teamName))
            {
                var name = (string)teamName.Value;
                if (name.Contains('/'))
                {
                    name = name.Split('/')[1];
                }
                teamTemplate = gameContext.Game.TeamFactory.FindTeamTemplateByName(name);
            }

            var gameObjectDefinition = gameContext.AssetLoadContext.AssetStore.ObjectDefinitions.GetByName(mapObject.TypeName);

            // TODO: Is there any valid case where we'd want to allow a game object to be null?
            if (gameObjectDefinition is null)
            {
                return null;
            }

            var gameObject = gameContext.GameLogic.CreateObject(gameObjectDefinition, teamTemplate?.Owner);
            gameObject.TeamTemplate = teamTemplate;
            gameObject.SetMapObjectProperties(mapObject, useRotationAnchorOffset, overwriteAngle);

            return gameObject;
        }

        internal void SetMapObjectProperties(
            MapObject mapObject,
            bool useRotationAnchorOffset = true,
            in float? overwriteAngle = 0.0f)
        {
            if (_body != null)
            {
                var healthMultiplier = mapObject.Properties.TryGetValue("objectInitialHealth", out var health)
                    ? (uint) health.Value / 100.0f
                    : 1.0f;

                _body.SetInitialHealth(healthMultiplier);
            }

            if (mapObject.Properties.TryGetValue("objectName", out var objectName))
            {
                Name = (string) objectName.Value;
            }

            if (mapObject.Properties.TryGetValue("objectSelectable", out var selectable))
            {
                IsSelectable = (bool) selectable.Value;
            }

            // TODO: handle "align to terrain" property
            var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, overwriteAngle ?? mapObject.Angle);
            var rotationOffset = useRotationAnchorOffset
                ? Vector4.Transform(new Vector4(Definition.RotationAnchorOffset.X, Definition.RotationAnchorOffset.Y, 0.0f, 1.0f), rotation)
                : Vector4.UnitW;
            var position = mapObject.Position + rotationOffset.ToVector3();
            var height = _gameContext.Scene3D.Terrain.HeightMap.GetHeight(position.X, position.Y) + mapObject.Position.Z;
            UpdateTransform(new Vector3(position.X, position.Y, height), rotation,
                Definition.Scale);

            if (Definition.IsBridge)
            {
                BridgeTowers.CreateForLandmarkBridge(_gameContext, this);
            }

            if (Definition.KindOf.Get(ObjectKinds.Horde))
            {
                FindBehavior<HordeContainBehavior>().Unpack();
            }
        }

        private readonly Dictionary<string, ModuleBase> _tagToModuleLookup = new();
        private readonly List<ModuleBase> _modules = new();

        private void AddModule(string tag, ModuleBase module)
        {
            module.Tag = tag;
            _modules.Add(module);
            _tagToModuleLookup.Add(tag, module);
        }

        private ModuleBase GetModuleByTag(string tag)
        {
            return _tagToModuleLookup[tag];
        }

        private readonly Dictionary<string, AttributeModifier> _attributeModifiers;

        private uint _id;

        public uint ID
        {
            get => _id;
            internal set
            {
                var oldObjectId = _id;

                _id = value;

                _gameContext.GameLogic.OnObjectIdChanged(this, oldObjectId);
            }
        }

        public Percentage ProductionModifier { get; set; } = new Percentage(1);
        public Fix64 HealthModifier { get; set; }


        private readonly GameContext _gameContext;

        internal GameContext GameContext => _gameContext;

        private readonly BehaviorUpdateContext _behaviorUpdateContext;

        private readonly GameObject _rallyPointMarker;

        private BodyDamageType _bodyDamageType = BodyDamageType.Pristine;

        internal BitArray<WeaponSetConditions> WeaponSetConditions;
        private readonly WeaponSet _weaponSet;
        public WeaponSet ActiveWeaponSet => _weaponSet;

        public readonly ObjectDefinition Definition;

        public PartitionObject PartitionObject { get; internal set; }

        public readonly Geometry Geometry;

        private readonly Transform _transform;
        public readonly Transform ModelTransform;

        private bool _objectMoved;

        public uint CreatedByObjectID;
        public uint BuiltByObjectID;

        private BitArray<ObjectStatus> _status = new();
        private byte _unknown2;
        private GameObjectUnknownFlags _unknownFlags;
        private readonly ShroudReveal _shroudRevealSomething1 = new();
        private readonly ShroudReveal _shroudRevealSomething2 = new();
        private readonly ShroudReveal _shroudRevealSomething3 = new();
        private float _visionRange;
        private float _shroudClearingRange;

        public void ApplyVisionRangeScalar(float scalar)
        {
            _shroudRevealSomething1.VisionRange *= scalar;
            _visionRange *= scalar;
            _shroudClearingRange *= scalar;
        }

        public void RemoveVisionRangeScalar(float scalar)
        {
            _shroudRevealSomething1.VisionRange /= scalar;
            _visionRange /= scalar;
            _shroudClearingRange /= scalar;
        }

        private BitArray<DisabledType> _disabledTypes = new();
        private readonly LogicFrame[] _disabledTypesFrames = new LogicFrame[9];
        public readonly ObjectVeterancyHelper VeterancyHelper;
        private uint _containerId;
        public uint ContainerId => _containerId;
        private uint _containedFrame;
        private string _teamName;
        private uint _enteredOrExitedPolygonTriggerFrame;
        private Point3D _integerPosition;
        private PolygonTriggerState[] _polygonTriggersState;
        private int _unknown5;
        private uint _unknownFrame;
        public uint HealedByObjectId;
        public uint HealedEndFrame;
        private uint _weaponBonusTypes;
        private byte _weaponSomethingPrimary;
        private byte _weaponSomethingSecondary;
        private byte _weaponSomethingTertiary;
        private BitArray<SpecialPowerType> _specialPowers = new();

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
            _objectMoved = true;
        }

        public void SetTranslation(Vector3 translation)
        {
            if (_transform.Translation == translation)
            {
                return;
            }
            _transform.Translation = translation;
            OnObjectMoved();
        }

        public void SetRotation(Quaternion rotation)
        {
            if (_transform.Rotation == rotation)
            {
                return;
            }
            _transform.Rotation = rotation;
            OnObjectMoved();
        }

        public void SetScale(float scale)
        {
            _transform.Scale = scale;
            OnObjectMoved();
        }

        public void UpdateTransform(in Vector3? translation = null, in Quaternion? rotation = null, float scale = 1.0f)
        {
            _transform.Translation = translation ?? _transform.Translation;
            _transform.Rotation = rotation ?? _transform.Rotation;
            _transform.Scale = scale;
            OnObjectMoved();
        }

        private void OnObjectMoved()
        {
            _objectMoved = true;
            PartitionObject?.SetDirty();
        }

        // Doing this with a field and a property instead of an auto-property allows us to have a read-only public interface,
        // while simultaneously supporting fast (non-allocating) iteration when accessing the list within the class.
        public IReadOnlyList<BehaviorModule> BehaviorModules => _behaviorModules;
        private readonly IReadOnlyList<BehaviorModule> _behaviorModules;

        // this allows us to avoid allocating and casting a new list when we just want a single object
        private FrozenDictionary<Type, object> _firstBehaviorCache;
        private FrozenDictionary<Type, List<object>> _behaviorCache;

        private readonly BodyModule _body;
        public bool HasActiveBody() => _body is ActiveBody;

        public bool TryGetLastDamage(out DamageData damageData)
        {
            damageData = default;
            if (_body is ActiveBody b)
            {
                damageData = b.LastDamage;
                return true;
            }

            return false;
        }

        public Fix64 Health
        {
            get => _body?.Health ?? Fix64.Zero;
            set => _body.Health = value < Fix64.Zero ? Fix64.Zero : value;
        }

        public Fix64 MaxHealth
        {
            get => _body?.MaxHealth + HealthModifier ?? Fix64.Zero;
            set => _body.MaxHealth = value;
        }
        public Fix64 HealthPercentage => MaxHealth != Fix64.Zero ? Health / MaxHealth : Fix64.Zero;

        public bool IsFullHealth => Health >= MaxHealth;

        public bool IsDead => Health <= Fix64.Zero;

        public void DoDamage(DamageType damageType, Percentage percentage, DeathType deathType, GameObject damageDealer) =>
            DoDamage(damageType, MaxHealth * (Fix64)(float)percentage, deathType, damageDealer);

        public void DoDamage(DamageType damageType, Fix64 amount, DeathType deathType, GameObject damageDealer)
        {
            _body.DoDamage(damageType, amount, deathType, damageDealer);
            // units can have multiple delayed heal behaviors, as the default object has an inheritable autohealbehavior provided by veterancy
            foreach (var autoHealBehavior in FindBehaviors<ISelfHealable>())
            {
                autoHealBehavior.RegisterDamage();
            }
        }

        public void Heal(Percentage percentage, GameObject healer) => Heal(MaxHealth * (Fix64)(float)percentage, healer);

        public void Heal(Fix64 amount, GameObject healer)
        {
            _body.Heal(amount, healer);
            HealedByObjectId = healer?.ID ?? 0;
        }

        /// <summary>
        /// Heals without any sort of healer or registering any sort of DamageData.
        /// </summary>
        public void HealDirectly(Percentage percentage) => HealDirectly(MaxHealth * (Fix64)(float)percentage);

        /// <summary>
        /// Heals without any sort of healer or registering any sort of DamageData.
        /// </summary>
        /// <param name="amount"></param>
        public void HealDirectly(Fix64 amount)
        {
            _body.Heal(amount);
        }

        public Collider RoughCollider { get; set; }
        public Collider ShapedCollider => Collider.Create(Definition.CurrentGeometryShape, Transform);
        public List<Collider> Colliders { get; }

        public float VerticalOffset;

        public Player Owner { get; internal set; }

        public GameObject ParentHorde { get; set; }

        public int Supply { get; set; }

        public bool IsStructure { get; private set; }

        public BitArray<ModelConditionFlag> ModelConditionFlags => Drawable.ModelConditionFlags;

        public RadiusDecalTemplate SelectionDecal;

        private string _name;

        public string Name
        {
            get => _name;
            private set
            {
                if (_name != null)
                {
                    throw new InvalidOperationException("An object's name cannot change once it's been set.");
                }

                _name = value ?? throw new ArgumentNullException(nameof(value));
                _gameContext.GameLogic.AddNameLookup(this);
            }
        }

        string IInspectable.Name => "GameObject";

        public TeamTemplate TeamTemplate { get; set; }

        public Team Team { get; internal set; }

        public bool IsSelectable;
        public bool IsProjectile { get; private set; } = false;
        public bool CanAttack { get; private set; }

        public bool IsSelected { get; set; }

        public Vector3? RallyPoint
        {
            get => FindBehavior<IHasRallyPoint>()?.RallyPointManager.RallyPoint;
            set => FindBehavior<IHasRallyPoint>()?.RallyPointManager.SetRallyPoint(value);
        }

        internal Weapon CurrentWeapon => _weaponSet.CurrentWeapon;

        private LogicFrameSpan ConstructionProgress { get; set; }

        public LogicFrame? LifeTime { get; set; }

        private float _buildProgress100;

        // todo: this is a bit convoluted, but the save file uses 0-100 whereas we use 0-1 for everything
        public float BuildProgress
        {
            get => _buildProgress100 / 100;
            set => _buildProgress100 = value * 100;
        }

        public bool Hidden { get; set; }

        public bool IsDamaged
        {
            get
            {
                var healthPercentage = (float) _body.HealthPercentage;
                var damagedThreshold = _gameContext.AssetLoadContext.AssetStore.GameData.Current.UnitDamagedThreshold;
                return healthPercentage <= damagedThreshold;
            }
        }

        public float Speed { get; set; }
        public float SteeringWheelsYaw { get; set; }
        public float Lift { get; set; }

        public float TurretYaw { get; set; }
        public float TurretPitch { get; set; }

        public bool IsPlacementPreview { get; set; }

        public bool IsPlacementInvalid { get; set; }

        public AIUpdate AIUpdate { get; }
        public ProductionUpdate ProductionUpdate { get; }

        private readonly UpgradeSet _upgrades = new();

        // We compute this every time it's requested, but we don't want
        // to allocate a new object every time.
        private readonly UpgradeSet _upgradesAll = new();

        private const string VeterancyUpgradeNameVeteran = "Upgrade_Veterancy_VETERAN"; // doesn't seem to be used in any of the inis, but is present in the sav file
        private const string VeterancyUpgradeNameElite = "Upgrade_Veterancy_ELITE";
        private const string VeterancyUpgradeNameHeroic = "Upgrade_Veterancy_HEROIC";

        public int Rank
        {
            get => (int) VeterancyHelper.VeterancyLevel;
            set
            {
                var rank = (VeterancyLevel)value;
                VeterancyHelper.VeterancyLevel = rank;
                var upgradeToApply = rank switch
                {
                    VeterancyLevel.Veteran => VeterancyUpgradeNameVeteran,
                    VeterancyLevel.Elite => VeterancyUpgradeNameElite,
                    VeterancyLevel.Heroic => VeterancyUpgradeNameHeroic,
                    _ => null,
                };
                // veterancy is only ever additive, and the inis allude to the fact that it is fine to skip upgrades and go straight from e.g. Veteran to Heroic
                if (upgradeToApply != null)
                {
                    Upgrade(GameContext.Game.AssetStore.Upgrades.GetByName(upgradeToApply));
                }
            }
        }

        public int ExperienceValue
        {
            get => VeterancyHelper.ExperiencePoints;
            set => VeterancyHelper.ExperiencePoints = value;
        }
        public int ExperienceRequiredForNextLevel { get; set; }

        public int EnergyProduction { get; internal set; }

        // TODO
        public ArmorTemplateSet CurrentArmorSet => Definition.ArmorSets.Values.First();

        public readonly Drawable Drawable;

        /// <summary>
        /// A non-persisted queue of rising-cash-text items that should originate from this GameObject.
        /// </summary>
        public CashEvent? ActiveCashEvent { get; set; }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal GameObject(
            ObjectDefinition objectDefinition,
            GameContext gameContext,
            Player owner)
        {
            if (objectDefinition.BuildVariations != null && objectDefinition.BuildVariations.Count() > 0)
            {
                objectDefinition = objectDefinition.BuildVariations[gameContext.Random.Next(0, objectDefinition.BuildVariations.Count())].Value;
            }

            _objectMoved = true;
            Hidden = false;

            Definition = objectDefinition ?? throw new ArgumentNullException(nameof(objectDefinition));
            VeterancyHelper = new ObjectVeterancyHelper(this);

            _attributeModifiers = new Dictionary<string, AttributeModifier>();
            _gameContext = gameContext;
            Owner = owner ?? gameContext.Game.PlayerManager.GetCivilianPlayer();

            _behaviorUpdateContext = new BehaviorUpdateContext(gameContext, this);

            _weaponSet = new WeaponSet(this);
            WeaponSetConditions = new BitArray<WeaponSetConditions>();
            UpdateWeaponSet();

            _transform = Transform.CreateIdentity();
            ModelTransform = Transform.CreateIdentity();
            _transform.Scale = Definition.Scale;

            // TODO: Instead of GameObject owning the drawable, which makes logic tests a little awkward,
            // perhaps create Drawable somewhere else and attach it to this GameObject?
            Drawable = gameContext.GameClient?.CreateDrawable(objectDefinition, this);

            var behaviors = new List<BehaviorModule>();

            _behaviorModules = behaviors;

            void AddBehavior(string tag, BehaviorModule behavior)
            {
                behaviors.Add(behavior);
                AddModule(tag, behavior);
            }

            AddBehavior("ModuleTag_SMCHelper", new ObjectSpecialModelConditionHelper());

            if (objectDefinition.KindOf.Get(ObjectKinds.CanBeRepulsed))
            {
                AddBehavior("ModuleTag_RepulsorHelper", new ObjectRepulsorHelper());
            }

            // TODO: This shouldn't be added to all objects. I don't know what the rule is.
            if (_gameContext.Game.SageGame >= SageGame.CncGeneralsZeroHour)
            {
                AddBehavior("ModuleTag_StatusDamageHelper", new StatusDamageHelper());
                AddBehavior("ModuleTag_SubdualDamageHelper", new SubdualDamageHelper());
            }

            // TODO: This shouldn't be added to all objects. I don't know what the rule is.
            // Maybe KindOf = CAN_ATTACK ?
            AddBehavior("ModuleTag_DefectionHelper", new ObjectDefectionHelper());

            // TODO: This shouldn't be added to all objects. I don't know what the rule is.
            // Probably only those with weapons.
            AddBehavior("ModuleTag_WeaponStatusHelper", new ObjectWeaponStatusHelper());

            // TODO: This shouldn't be added to all objects. I don't know what the rule is.
            // Probably only those with weapons.
            AddBehavior("ModuleTag_FiringTrackerHelper", new ObjectFiringTrackerHelper());

            // TODO: This shouldn't be added to all objects. I don't know what the rule is.
            if (_gameContext.Game.SageGame is not SageGame.CncGenerals and not SageGame.CncGeneralsZeroHour)
            {
                // this was added in bfme and is not present in generals or zero hour
                AddBehavior("ModuleTag_ExperienceHelper", new ExperienceUpdate(this));
            }

            // TODO: This shouldn't be added to all objects. I don't know what the rule is.
            if (_gameContext.Game.SageGame >= SageGame.CncGeneralsZeroHour)
            {
                AddBehavior("ModuleTag_TempWeaponBonusHelper", new TempWeaponBonusHelper());
            }

            foreach (var behaviorDataContainer in objectDefinition.Behaviors.Values)
            {
                var behaviorModuleData = (BehaviorModuleData) behaviorDataContainer.Data;
                var module = AddDisposable(behaviorModuleData.CreateModule(this, gameContext));

                // TODO: This will never be null once we've implemented all the behaviors.
                if (module != null)
                {
                    AddBehavior(behaviorDataContainer.Tag, module);

                    if (module is BodyModule body)
                    {
                        _body = body;
                    }
                    else if (module is AIUpdate aiUpdate)
                    {
                        AIUpdate = aiUpdate;
                    }
                    else if (module is ProductionUpdate productionUpdate)
                    {
                        ProductionUpdate = productionUpdate;
                    }
                }
            }

            Geometry = Definition.Geometry.Clone();

            Colliders = new List<Collider>();
            foreach (var geometry in Geometry.Shapes)
            {
                Colliders.Add(Collider.Create(geometry, _transform));
            }

            RoughCollider = Collider.Create(Colliders);

            _visionRange = Definition.VisionRange;
            _shroudRevealSomething1.VisionRange = Definition.VisionRange;
            _shroudClearingRange = Definition.ShroudClearingRange;

            IsSelectable = Definition.KindOf.Get(ObjectKinds.Selectable);
            CanAttack = Definition.KindOf.Get(ObjectKinds.CanAttack);

            if (Definition.KindOf.Get(ObjectKinds.AutoRallyPoint))
            {
                var rpMarkerDef = gameContext.AssetLoadContext.AssetStore.ObjectDefinitions.GetByName("RallyPointMarker");
                // TODO: Only create a Drawable, as suggested by DRAWABLE_ONLY.
                _rallyPointMarker = AddDisposable(new GameObject(rpMarkerDef, gameContext, owner));
            }

            if (Definition.KindOf.Get(ObjectKinds.Projectile))
            {
                IsProjectile = true;
            }

            if (Definition.KindOf.Get(ObjectKinds.Tree))
            {
                Supply = Definition.SupplyOverride > 0 ? Definition.SupplyOverride : gameContext.AssetLoadContext.AssetStore.GameData.Current.SupplyBoxesPerTree;
            }

            if (Definition.KindOf.Get(ObjectKinds.Structure))
            {
                IsStructure = true;
            }
        }

        public bool AffectsAreaPassability
        {
            get
            {
                if (!IsStructure)
                {
                    return false;
                }
                if (_gameContext.Scene3D.Game.SageGame == SageGame.CncGenerals ||
                    _gameContext.Scene3D.Game.SageGame == SageGame.CncGenerals)
                {
                    // SupplyWarehouse in CncGenerals has NoCollide
                    return true;
                }
                return !Definition.KindOf.Get(ObjectKinds.NoCollide);

            }
        }

        public bool IsKindOf(ObjectKinds kind) => Definition.KindOf.Get(kind);

        public void AddAttributeModifier(string name, AttributeModifier modifier)
        {
            if (_attributeModifiers.ContainsKey(name))
            {
                return;
            }
            _attributeModifiers.Add(name, modifier);
        }

        public void RemoveAttributeModifier(string name)
        {
            _attributeModifiers[name].Invalid = true;
        }

        public void ShowCollider(string name)
        {
            if (Colliders.Any(x => x.Name.Equals(name)))
            {
                return;
            }

            var newColliders = new List<Collider>();
            foreach (var geometry in Definition.Geometry.Shapes)
            {
                if (geometry.Name.Equals(name))
                {
                    newColliders.Add(Collider.Create(geometry, Transform));
                }
            }

            if (AffectsAreaPassability)
            {
                foreach (var collider in newColliders)
                {
                    _gameContext.Navigation.UpdateAreaPassability(collider, false);
                }
            }
            Colliders.AddRange(newColliders);
            RoughCollider = Collider.Create(Colliders);
            _gameContext.Quadtree.Update(this);
        }

        public void HideCollider(string name)
        {
            if (!Colliders.Any(x => x.Name.Equals(name)))
            {
                return;
            }

            for (var i = Colliders.Count - 1; i >= 0; i--)
            {
                if (Colliders[i].Name.Equals(name))
                {
                    if (AffectsAreaPassability)
                    {
                        _gameContext.Navigation.UpdateAreaPassability(Colliders[i], true);
                    }
                    Colliders.RemoveAt(i);
                }
            }
            RoughCollider = Collider.Create(Colliders);
            _gameContext.Quadtree.Update(this);

            if (AffectsAreaPassability)
            {
                _gameContext.Navigation.UpdateAreaPassability(this, false);
            }
        }

        public void UpdateColliders()
        {
            RoughCollider.Update(_transform);
            foreach (var collider in Colliders)
            {
                collider.Update(_transform);
            }
            _gameContext.Quadtree.Update(this);
        }

        internal void LogicTick(in TimeInterval time)
        {
            if (_objectMoved)
            {
                UpdateColliders();

                if (AffectsAreaPassability)
                {
                    _gameContext.Navigation.UpdateAreaPassability(this, false);
                }

                _objectMoved = false;
            }

            if (ModelConditionFlags.Get(ModelConditionFlag.Attacking))
            {
                CurrentWeapon?.LogicTick();
            }
            if (ModelConditionFlags.Get(ModelConditionFlag.Sold))
            {
                Die(DeathType.Normal);
                ModelConditionFlags.Set(ModelConditionFlag.Sold, false);
            }

            foreach (var (key, modifier) in _attributeModifiers)
            {
                if (!modifier.Applied)
                {
                    modifier.Apply(this, time);
                }
                else if (modifier.Invalid || modifier.Expired(time))
                {
                    modifier.Remove(this);
                    _attributeModifiers.Remove(key);
                }
                else
                {
                    modifier.Update(this, time);
                }
            }
        }

        internal void Update()
        {
            VerifyHealer();
            CheckDisabledStates();
            foreach (var behavior in _behaviorModules)
            {
                if (IsDead && behavior is not SlowDeathBehavior and not LifetimeUpdate and not DeletionUpdate)
                {
                    continue; // if we're dead, we should only update SlowDeathBehavior, LifetimeUpdate, or DeletionUpdate
                }
                behavior.Update(_behaviorUpdateContext);
            }
        }

        public bool CanRecruitHero(ObjectDefinition definition)
        {
            foreach (var obj in _gameContext.GameLogic.Objects)
            {
                if (obj.Definition.Name == definition.Name
                    && obj.Owner == Owner)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanProduceObject(ObjectDefinition definition)
        {
            return ProductionUpdate?.CanProduceObject(definition) ?? true;
        }

        public bool CollidesWith(ICollidable other, bool twoDimensional = false)
        {
            if (RoughCollider == null || other.RoughCollider == null)
            {
                return false;
            }

            if (!RoughCollider.Intersects(other.RoughCollider, twoDimensional))
            {
                return false;
            }

            foreach (var collider in Colliders)
            {
                foreach (var otherCollider in other.Colliders)
                {
                    if (collider.Intersects(otherCollider, twoDimensional))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal void OnCollide(GameObject collidingObject)
        {
            Logger.Info($"GameObject {Definition.Name} colliding with {collidingObject.Definition.Name}");

            foreach (var behavior in _behaviorModules)
            {
                if (behavior is ICollideModule collideModule)
                {
                    collideModule.OnCollide(collidingObject);
                }
            }
        }

        internal void OnDestroy()
        {
            foreach (var behavior in _behaviorModules)
            {
                if (behavior is IDestroyModule destroyModule)
                {
                    destroyModule.OnDestroy();
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

        public T FindBehavior<T>()
        {
            if (_firstBehaviorCache == null)
            {
                InstantiateBehaviorCache();
            }

            return _firstBehaviorCache!.TryGetValue(typeof(T), out var behavior) ? (T)behavior : default;
        }

        public IEnumerable<T> FindBehaviors<T>()
        {
            if (_behaviorCache == null)
            {
                InstantiateBehaviorCache();
            }

            return _behaviorCache!.TryGetValue(typeof(T), out var behaviors) ? behaviors.Cast<T>() : [];
        }

        public T FindSpecialPowerBehavior<T>(SpecialPowerType specialPowerType) where T : SpecialPowerModule
        {
            return FindBehaviors<T>().FirstOrDefault(m => m.SpecialPowerType == specialPowerType);
        }

        public bool HasBehavior<T>()
        {
            return FindBehavior<T>() != null;
        }

        private void InstantiateBehaviorCache()
        {
            var cache = new Dictionary<Type, List<object>>();
            var firstCache = new Dictionary<Type, object>();
            foreach (var module in _behaviorModules)
            {
                var type = module.GetType();
                foreach (var parent in GetParentTypes(type))
                {
                    if (!cache.TryGetValue(parent, out var value))
                    {
                        value = [];
                        cache[parent] = value;
                        firstCache[parent] = module;
                    }

                    value.Add(module);
                }
            }

            _firstBehaviorCache = firstCache.ToFrozenDictionary();
            _behaviorCache = cache.ToFrozenDictionary();
        }

        // modified https://stackoverflow.com/a/18375526
        private static IEnumerable<Type> GetParentTypes(Type type)
        {
            yield return type;

            // return all implemented or inherited interfaces
            foreach (var i in type.GetInterfaces())
            {
                yield return i;
            }

            // return all inherited types
            var currentBaseType = type.BaseType;
            while (currentBaseType != null)
            {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType;
            }
        }

        public bool HasUpgrade(UpgradeTemplate upgrade)
        {
            if (upgrade == null)
            {
                return true;
            }

            return upgrade.Type == UpgradeType.Player
                ? Owner.HasUpgrade(upgrade)
                : _upgrades.Contains(upgrade);
        }

        public bool HasEnqueuedUpgrade(UpgradeTemplate upgrade)
        {
            return upgrade.Type == UpgradeType.Player
                ? Owner.HasEnqueuedUpgrade(upgrade)
                : ProductionUpdate.ProductionQueue.Any(x => x.UpgradeDefinition == upgrade);
        }

        /// <summary>
        /// Used to set an object status when the integer value of the bit is known but the actual enum value is not.
        /// </summary>
        /// <remarks>
        /// When <see cref="ObjectStatus"/> is fully defined, this method should be removed.
        /// </remarks>
        public void SetUnknownStatus(int status, bool value)
        {
            _status.Set(status, value);
        }

        public void SetObjectStatus(ObjectStatus status, bool value)
        {
            _status.Set(status, value);
        }

        /// <summary>
        /// Called when a foundation has been placed, but construction has not yet begun
        /// </summary>
        internal void PrepareConstruction()
        {
            if (!IsStructure)
            {
                return;
            }

            Health = Fix64.Zero;
            ClearModelConditionFlags();

            ModelConditionFlags.Set(ModelConditionFlag.ActivelyBeingConstructed, false);
            ModelConditionFlags.Set(ModelConditionFlag.AwaitingConstruction, true);
            ModelConditionFlags.Set(ModelConditionFlag.PartiallyConstructed, false);

            _status.Set(ObjectStatus.UnderConstruction, true);

            // flatten terrain around object
            var centerPosition = _gameContext.Terrain.HeightMap.GetTilePosition(_transform.Translation);

            if (centerPosition == null)
            {
                throw new InvalidStateException("object built outside of map bounds");
            }

            var (centerX, centerY) = centerPosition.Value;
            var maxHeight = _gameContext.Terrain.HeightMap.GetHeight(centerX, centerY);
            // on slopes, the height map isn't always exactly where our cursor is (our cursor is a float, and the heightmap is always a ushort), so snap the building to the nearest heightmap value
            UpdateTransform(_transform.Translation with { Z = maxHeight }, _transform.Rotation);

            // clear anything applicable in the build area (trees, rubble, etc)
            var toDelete = new BitArray<ObjectKinds>(ObjectKinds.Shrubbery, ObjectKinds.ClearedByBuild); // this may not be completely accurate
            foreach (var intersecting in _gameContext.Quadtree.FindIntersecting(ShapedCollider))
            {
                if (intersecting.Definition.KindOf.Intersects(toDelete))
                {
                    GameContext.GameLogic.DestroyObject(intersecting);
                }
            }

            _gameContext.Terrain.SetMaxHeight(ShapedCollider, maxHeight);
        }

        /// <summary>
        /// Called when the dozer gets within the bounding sphere of the object in generals
        /// </summary>
        internal void SetIsBeingConstructed()
        {
            ModelConditionFlags.Set(ModelConditionFlag.ActivelyBeingConstructed, true);
            ModelConditionFlags.Set(ModelConditionFlag.PartiallyConstructed, true);
            ModelConditionFlags.Set(ModelConditionFlag.AwaitingConstruction, false);
        }

        internal void AdvanceConstruction()
        {
            var lastBuildProgress = BuildProgress;
            BuildProgress = Math.Clamp(++ConstructionProgress / Definition.BuildTime, 0.0f, 1.0f);
            // structures can be attacked while under construction, and their health is a factor of their build progress;
            var newHealth = (Fix64)(BuildProgress - lastBuildProgress) * MaxHealth;
            HealDirectly(newHealth);

            if (BuildProgress >= 1.0f)
            {
                FinishConstruction();
            }
        }

        internal void FinishConstruction()
        {
            ClearModelConditionFlags();

            _status.Set(ObjectStatus.UnderConstruction, false);

            EnergyProduction += Definition.EnergyProduction;

            foreach (var module in FindBehaviors<ICreateModule>())
            {
                module.OnBuildComplete();
            }
        }

        public bool IsBeingConstructed()
        {
            return ModelConditionFlags.Get(ModelConditionFlag.ActivelyBeingConstructed) ||
                   ModelConditionFlags.Get(ModelConditionFlag.AwaitingConstruction) ||
                   ModelConditionFlags.Get(ModelConditionFlag.PartiallyConstructed);
        }

        public bool IsUnderActiveConstruction()
        {
            return ModelConditionFlags.Get(ModelConditionFlag.ActivelyBeingConstructed);
        }

        internal void UpdateDamageFlags(Fix64 healthPercentage, bool takingDamage)
        {
            // TODO: SoundDie
            // TODO: TransitionDamageFX

            var oldDamageType = _bodyDamageType;

            if (healthPercentage < (Fix64) GameContext.AssetLoadContext.AssetStore.GameData.Current.UnitReallyDamagedThreshold)
            {
                if (takingDamage && !ModelConditionFlags.Get(ModelConditionFlag.ReallyDamaged))
                {
                    _gameContext.AudioSystem.PlayAudioEvent(Definition.SoundOnReallyDamaged?.Value);
                }

                if (!IsBeingConstructed())
                {
                    // prevents damaged flags from being set while the building is being constructed - generals has models for this it seems, but they're not implemented as far as I can tell?
                    ModelConditionFlags.Set(ModelConditionFlag.ReallyDamaged, true);
                    _bodyDamageType = BodyDamageType.ReallyDamaged;
                }

                ModelConditionFlags.Set(ModelConditionFlag.Damaged, false);
            }
            else if (healthPercentage < (Fix64) GameContext.AssetLoadContext.AssetStore.GameData.Current.UnitDamagedThreshold)
            {
                if (takingDamage && !ModelConditionFlags.Get(ModelConditionFlag.Damaged))
                {
                    _gameContext.AudioSystem.PlayAudioEvent(Definition.SoundOnDamaged?.Value);
                }

                if (!IsBeingConstructed())
                {
                    ModelConditionFlags.Set(ModelConditionFlag.Damaged, true);
                    _bodyDamageType = BodyDamageType.Damaged;
                }
                ModelConditionFlags.Set(ModelConditionFlag.ReallyDamaged, false);
            }
            else
            {
                ModelConditionFlags.Set(ModelConditionFlag.ReallyDamaged, false);
                ModelConditionFlags.Set(ModelConditionFlag.Damaged, false);
                _bodyDamageType = BodyDamageType.Pristine;
            }

            if (oldDamageType == _bodyDamageType)
            {
                return;
            }

            foreach (var behavior in _behaviorModules)
            {
                behavior.OnDamageStateChanged(
                    _behaviorUpdateContext,
                    oldDamageType,
                    _bodyDamageType);
            }
        }

        internal void LocalLogicTick(in TimeInterval gameTime, float tickT, HeightMap heightMap)
        {
            _rallyPointMarker?.LocalLogicTick(gameTime, tickT, heightMap);
            Drawable.LogicTick();
        }

        internal void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime)
        {
            if (ModelConditionFlags.Get(ModelConditionFlag.Sold) || Hidden)
            {
                return;
            }

            var renderItemConstantsPS = new MeshShaderResources.RenderItemConstantsPS
            {
                HouseColor = Owner.Color.ToVector3(),
                Opacity = IsPlacementPreview ? 0.7f : 1.0f,
                TintColor = IsPlacementInvalid ? new Vector3(1, 0.3f, 0.3f) : Vector3.One,
            };

            // This must be done after processing anything that might update this object's transform.
            var worldMatrix = ModelTransform.Matrix * _transform.Matrix;
            Drawable.BuildRenderList(renderList, camera, gameTime, worldMatrix, renderItemConstantsPS);

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
            // todo: maintain map-based flags, such as NIGHT, SNOW, etc
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

        internal void SetWeaponSetCondition(WeaponSetConditions condition, bool value)
        {
            WeaponSetConditions.Set(condition, value);
            UpdateWeaponSet();
        }

        private void UpdateWeaponSet()
        {
            _weaponSet.Update();
        }

        internal void SetWarhead(WeaponTemplate weaponTemplate)
        {
            // TODO
        }

        public bool CanPurchase(CommandButton button)
        {
            if (button.Object != null && button.Object.Value != null)
            {
                return HasUpgrade(button.NeededUpgrade?.Value)
                    && CanConstructUnit(button.Object.Value);
            }
            if (button.Upgrade != null && button.Upgrade.Value != null)
            {
                return HasUpgrade(button.NeededUpgrade?.Value)
                    && CanEnqueueUpgrade(button.Upgrade.Value);
            }
            if (button.Command == CommandType.CastleUnpack)
            {
                var castleBehavior = FindBehavior<CastleBehavior>();
                return HasEnoughMoney(castleBehavior.GetUnpackCost(Owner));
            }
            return true;
        }

        public bool HasEnoughMoney(float cost) => Owner.BankAccount.Money >= cost;

        public bool CanConstructUnit(ObjectDefinition objectDefinition)
        {
            return objectDefinition != null
                && HasEnoughMoney(objectDefinition.BuildCost)
                && Owner.CanProduceObject(objectDefinition)
                && CanProduceObject(objectDefinition);
        }

        public bool CanEnqueueUpgrade(UpgradeTemplate upgrade)
        {
            if (upgrade == null || ProductionUpdate == null)
            {
                return false;
            }

            var userHasEnoughMoney = HasEnoughMoney(upgrade.BuildCost);
            var hasQueuedUpgrade = HasEnqueuedUpgrade(upgrade);
            var canEnqueue = ProductionUpdate.CanEnqueue();
            var hasUpgrade = HasUpgrade(upgrade);

            var existingUpgrades = GetUpgradesCompleted();
            existingUpgrades.Add(upgrade);

            var upgradeModuleCanUpgrade = false;
            var hasUpgradeBehaviors = false; // some objects don't have upgrade modules
            foreach (var upgradeModule in FindBehaviors<IUpgradeableModule>())
            {
                hasUpgradeBehaviors = true;
                if (upgradeModule.CanUpgrade(existingUpgrades))
                {
                    upgradeModuleCanUpgrade = true;
                    break;
                }
            }

            return userHasEnoughMoney && canEnqueue && !hasQueuedUpgrade && !hasUpgrade && (!hasUpgradeBehaviors || upgradeModuleCanUpgrade);
        }

        private UpgradeSet GetUpgradesCompleted()
        {
            _upgradesAll.Clear();
            _upgradesAll.UnionWith(_upgrades);
            _upgradesAll.UnionWith(Owner.UpgradesCompleted);
            return _upgradesAll;
        }

        public void Upgrade(UpgradeTemplate upgrade)
        {
            _upgrades.Add(upgrade);

            UpdateUpgradeableModules();
        }

        internal void UpdateUpgradeableModules()
        {
            var completedUpgrades = GetUpgradesCompleted();

            foreach (var upgradeableModule in FindBehaviors<IUpgradeableModule>())
            {
                upgradeableModule.TryUpgrade(completedUpgrades);
            }
        }

        public void RemoveUpgrade(UpgradeTemplate upgrade)
        {
            _upgrades.Remove(upgrade);

            // TODO: Set _triggered to false for all affected upgrade modules
        }

        public bool CanAttackObject(GameObject target)
        {
            return CanAttack && Definition.WeaponSets.Values.Any(t =>
                t.Slots.Any(s => s?.Weapon.Value?.AntiMask.CanAttackObject(target) == true));
        }

        // todo: this probably is not correct
        public bool IsAirborne(float groundDelta = 0.1f)
        {
            return Translation.Z - GameContext.Terrain.HeightMap.GetHeight(Translation.X, Translation.Y) > groundDelta;
        }

        internal void Kill(DeathType deathType)
        {
            _body.DoDamage(DamageType.Unresistable, _body.Health, deathType, null);
        }

        internal void Die(DeathType deathType)
        {
            Logger.Info("Object dying " + deathType);
            bool construction = IsBeingConstructed();

            if (construction)
            {
                ModelConditionFlags.Set(ModelConditionFlag.DestroyedWhilstBeingConstructed, true);

                var mostRecentConstructor = GameContext.GameLogic.GetObjectById(BuiltByObjectID);
                // mostRecentConstructor is set to the unit currently or most recently building us
                if (mostRecentConstructor.AIUpdate is IBuilderAIUpdate builderAiUpdate && builderAiUpdate.BuildTarget == this)
                {
                    // mostRecentConstructor is still trying to build us
                    mostRecentConstructor.AIUpdate.Stop();
                }
            }
            else
            {
                ModelConditionFlags.Set(ModelConditionFlag.ReallyDamaged, false);
                ModelConditionFlags.Set(ModelConditionFlag.Damaged, false);
            }

            IsSelectable = false;
            Owner.DeselectUnit(this);

            if (!construction)
            {
                ExecuteRandomSlowDeathBehavior(deathType);
            }

            foreach (var module in _behaviorModules)
            {
                if (module is SlowDeathBehavior)
                {
                    // this is handled above
                    continue;
                }

                module.OnDie(_behaviorUpdateContext, deathType, _status);
            }

            PlayDieSound(deathType);
        }

        /// <summary>
        /// Removes this object from the scene without playing any death effects or affecting stats.
        /// </summary>
        public void Destroy()
        {
            GameContext.GameLogic.DestroyObject(this);
        }

        private void ExecuteRandomSlowDeathBehavior(DeathType deathType)
        {
            // If there are multiple SlowDeathBehavior modules,
            // we need to use ProbabilityModifier to choose between them.
            var slowDeathBehaviors = FindBehaviors<SlowDeathBehavior>()
                .Where(x => x.IsApplicable(deathType, _status))
                .ToList();

            var sumProbabilityModifiers = slowDeathBehaviors.Sum(x => x.ProbabilityModifier);
            var random = _gameContext.Random.Next(sumProbabilityModifiers);
            var cumulative = 0;
            foreach (var deathBehavior in slowDeathBehaviors)
            {
                cumulative += deathBehavior.ProbabilityModifier;
                if (random < cumulative)
                {
                    deathBehavior.OnDie(_behaviorUpdateContext, deathType, _status);
                    return;
                }
            }
        }

        private void PlayDieSound(DeathType deathType)
        {
            var voiceDie = deathType switch
            {
                DeathType.Burned => Definition.SoundDieFire?.Value,
                DeathType.Poisoned or DeathType.PoisonedGamma or DeathType.PoisonedBeta => Definition.SoundDieToxin?.Value,
                _ => null,
            } ?? Definition.SoundDie?.Value;

            if (voiceDie != null)
            {
                GameContext.AudioSystem.PlayAudioEvent(this, voiceDie);
            }
        }

        public void SetSelectable(bool selectable)
        {
            IsSelectable = selectable;
            _status.Set(ObjectStatus.Unselectable, !selectable);
        }

        internal void AddToContainer(uint containerId)
        {
            _containerId = containerId;
            _containedFrame = _gameContext.GameLogic.CurrentFrame.Value;
            var disabledUntilFrame = new LogicFrame(0x3FFFFFFFu); // not sure why this is this way;
            Disable(DisabledType.Held, disabledUntilFrame);
            Hidden = true;
            SetSelectable(false);
            _status.Set(ObjectStatus.Masked, true);
            _status.Set(ObjectStatus.InsideGarrison, true); // even if it's a vehicle, tunnel, etc
            Owner.DeselectUnit(this);
        }

        internal void RemoveFromContainer()
        {
            _containerId = 0;
            _containedFrame = 0;
            UnDisable(DisabledType.Held);
            Hidden = false;
            SetSelectable(true);
            _status.Set(ObjectStatus.Masked, false);
            _status.Set(ObjectStatus.InsideGarrison, false);
        }

        public void Disable(DisabledType type, LogicFrame frame)
        {
            _disabledTypes.Set(type, true);
            _disabledTypesFrames[(int)type] = frame;
        }

        public void UnDisable(DisabledType type)
        {
            _disabledTypes.Set(type, false);
            _disabledTypesFrames[(int)type] = LogicFrame.Zero;
        }

        private void CheckDisabledStates()
        {
            for (var i = 0; i < _disabledTypesFrames.Length; i++)
            {
                var disabledTypeFrame = _disabledTypesFrames[i];
                if (disabledTypeFrame > LogicFrame.Zero && disabledTypeFrame < _gameContext.GameLogic.CurrentFrame)
                {
                    UnDisable((DisabledType)i);
                }
            }
        }

        internal void Sell()
        {
            ModelConditionFlags.Set(ModelConditionFlag.Sold, true);
            _status.Set(ObjectStatus.Unselectable, true);
            _status.Set(ObjectStatus.Sold, true);
            Owner.DeselectUnit(this);
        }

        internal void GainExperience(int experience)
        {
            if (!Definition.IsTrainable)
            {
                return;
            }

            VeterancyHelper.GainExperience(experience);
        }

        internal void SetBeingHealed(GameObject healer, uint endFrame)
        {
            HealedByObjectId = healer.ID;
            HealedEndFrame = endFrame;
        }

        private void VerifyHealer()
        {
            if (HealedByObjectId != 0 && (IsFullHealth || _gameContext.GameLogic.CurrentFrame.Value >= HealedEndFrame))
            {
                HealedByObjectId = 0;
                HealedEndFrame = 0; // todo: is this reset?
            }
        }

        public void AddWeaponBonusType(WeaponBonusType bonusType)
        {
            _weaponBonusTypes |= (uint)bonusType;
        }

        public void RemoveWeaponBonusType(WeaponBonusType bonusType)
        {
            _weaponBonusTypes &= ~(uint)bonusType;
        }

        public void Persist(StatePersister reader)
        {
            var version = reader.PersistVersion(9);

            var id = ID;
            reader.PersistObjectID(ref id, "ObjectId");
            if (reader.Mode == StatePersistMode.Read)
            {
                ID = id;
            }

            var transform = reader.Mode == StatePersistMode.Write
                ? Matrix4x3.FromMatrix4x4(_transform.Matrix)
                : Matrix4x3.Identity;
            reader.PersistMatrix4x3(ref transform);
            if (reader.Mode == StatePersistMode.Read)
            {
                SetTransformMatrix(transform.ToMatrix4x4());
            }

            var teamId = Team?.Id ?? 0u;
            reader.PersistUInt32(ref teamId);
            Team = GameContext.Game.TeamFactory.FindTeamById(teamId);

            Owner = Team.Template.Owner;

            reader.PersistObjectID(ref CreatedByObjectID);
            reader.PersistUInt32(ref BuiltByObjectID);

            var drawableId = Drawable.ID;
            reader.PersistUInt32(ref drawableId);
            if (reader.Mode == StatePersistMode.Read)
            {
                Drawable.ID = drawableId;
            }

            reader.PersistAsciiString(ref _name);

            if (version >= 9)
            {
                reader.PersistBitArray(ref _status);
            }
            else
            {
                reader.PersistBitArrayAsUInt32(ref _status); // this is stored as a uint in the sav file
            }

            reader.PersistByte(ref _unknown2);
            reader.PersistEnumByteFlags(ref _unknownFlags);

            reader.PersistObject(Geometry);

            reader.PersistObject(_shroudRevealSomething1);
            reader.PersistObject(_shroudRevealSomething2);

            if (version >= 9)
            {
                reader.PersistObject(_shroudRevealSomething3);

                reader.SkipUnknownBytes(66);
            }

            reader.PersistSingle(ref _visionRange);
            reader.PersistSingle(ref _shroudClearingRange);

            reader.SkipUnknownBytes(4);

            reader.PersistBitArray(ref _disabledTypes);

            reader.SkipUnknownBytes(1);

            reader.PersistArray(
                _disabledTypesFrames,
                static (StatePersister persister, ref LogicFrame item) =>
                {
                    persister.PersistLogicFrameValue(ref item);
                });

            reader.SkipUnknownBytes(8);

            if (version >= 9)
            {
                reader.SkipUnknownBytes(12);
            }

            reader.PersistObject(VeterancyHelper);
            reader.PersistObjectID(ref _containerId);
            reader.PersistFrame(ref _containedFrame);

            // TODO: This goes up to 100, not 1, as other code in GameObject expects
            reader.PersistSingle(ref _buildProgress100);

            reader.PersistObject(_upgrades);

            // Not always (but usually is) the same as the teamId above implies.
            reader.PersistAsciiString(ref _teamName);

            reader.SkipUnknownBytes(16);

            byte polygonTriggerStateCount = (byte)(_polygonTriggersState?.Length ?? 0);
            reader.PersistByte(ref polygonTriggerStateCount);
            if (reader.Mode == StatePersistMode.Read)
            {
                _polygonTriggersState = new PolygonTriggerState[polygonTriggerStateCount];
            }

            reader.PersistFrame(ref _enteredOrExitedPolygonTriggerFrame);
            reader.PersistPoint3D(ref _integerPosition);

            reader.BeginArray("PolygonTriggerStates");
            for (var i = 0; i < polygonTriggerStateCount; i++)
            {
                reader.PersistObjectValue(ref _polygonTriggersState[i]);
            }
            reader.EndArray();

            var unknown4 = 1;
            reader.PersistInt32(ref unknown4);
            if (unknown4 != 1)
            {
                throw new InvalidStateException();
            }

            reader.PersistInt32(ref _unknown5); // 0, 1
            reader.PersistBoolean(ref IsSelectable);
            reader.PersistFrame(ref _unknownFrame);

            reader.SkipUnknownBytes(4);

            // Modules
            var numModules = (ushort)_modules.Count;
            reader.PersistUInt16(ref numModules);

            reader.BeginArray("Modules");
            if (reader.Mode == StatePersistMode.Read)
            {
                for (var i = 0; i < numModules; i++)
                {
                    reader.BeginObject();

                    var moduleTag = "";
                    reader.PersistAsciiString(ref moduleTag);

                    var module = GetModuleByTag(moduleTag);

                    reader.BeginSegment($"{module.GetType().Name} module in game object {Definition.Name}");

                    reader.PersistObject(module);

                    reader.EndSegment();

                    reader.EndObject();
                }
            }
            else
            {
                foreach (var module in _modules)
                {
                    reader.BeginObject();

                    var moduleTag = module.Tag;
                    reader.PersistAsciiString(ref moduleTag);

                    reader.BeginSegment($"{module.GetType().Name} module in game object {Definition.Name}");

                    reader.PersistObject(module);

                    reader.EndSegment();

                    reader.EndObject();
                }
            }
            reader.EndArray();

            reader.PersistObjectID(ref HealedByObjectId);
            reader.PersistFrame(ref HealedEndFrame);
            reader.PersistBitArray(ref WeaponSetConditions);
            reader.PersistUInt32(ref _weaponBonusTypes);

            var weaponBonusTypesBitArray = new BitArray<WeaponBonusType>();
            var weaponBonusTypeCount = EnumUtility.GetEnumCount<WeaponBonusType>();
            for (var i = 0; i < weaponBonusTypeCount; i++)
            {
                var weaponBonusBit = (_weaponBonusTypes >> i) & 1;
                weaponBonusTypesBitArray.Set(i, weaponBonusBit == 1);
            }

            reader.PersistByte(ref _weaponSomethingPrimary);
            reader.PersistByte(ref _weaponSomethingSecondary);
            reader.PersistByte(ref _weaponSomethingTertiary);
            reader.PersistObject(_weaponSet);
            reader.PersistBitArray(ref _specialPowers);

            reader.SkipUnknownBytes(1);

            var unknown6 = true;
            reader.PersistBoolean(ref unknown6);
            if (!unknown6)
            {
                throw new InvalidStateException();
            }

            var unknown7 = true;
            reader.PersistBoolean(ref unknown7);
            if (!unknown7)
            {
                throw new InvalidStateException();
            }
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
                Kill(DeathType.Exploded);
            }

            if ((Definition.IsTrainable || Definition.BuildVariations?.Any(v => v.Value.IsTrainable) == true) &&
                ImGui.CollapsingHeader("Veterancy"))
            {
                ImGui.LabelText("Experience", VeterancyHelper.ExperiencePoints.ToString());
                var rank = VeterancyHelper.VeterancyLevel;
                ImGuiUtility.ComboEnum("Current Rank", ref rank);
                Rank = (int)rank;
            }

            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.LabelText("DisplayName", Definition.DisplayName ?? string.Empty);

                var translation = _transform.Translation;
                if (ImGui.DragFloat3("Position", ref translation))
                {
                    _transform.Translation = translation;
                }

                ImGui.LabelText("ObjectStatus", _status.DisplayName);
                if (ImGui.TreeNodeEx("Select ObjectStatus"))
                {
                    foreach (var flag in Enum.GetValues<ObjectStatus>())
                    {
                        if (ImGui.Selectable(flag.ToString(), _status.Get(flag)))
                        {
                            _status.Set(flag, !_status.Get(flag));
                        }
                    }
                    ImGui.TreePop();
                }

                ImGui.LabelText("ModelConditionFlags", ModelConditionFlags.DisplayName);
                if (ImGui.TreeNodeEx("Select ModelConditionFlags"))
                {
                    foreach (var flag in Enum.GetValues<ModelConditionFlag>())
                    {
                        if (ImGui.Selectable(flag.ToString(), ModelConditionFlags.Get(flag)))
                        {
                            ModelConditionFlags.Set(flag, !ModelConditionFlags.Get(flag));
                        }
                    }
                    ImGui.TreePop();
                }

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

            static void DrawInspector(ModuleBase module)
            {
                if (ImGui.CollapsingHeader($"{module.GetType().Name} {module.Tag}", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    module.DrawInspector();
                }
            }

            foreach (var drawModule in Drawable.DrawModules)
            {
                DrawInspector(drawModule);
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
                DrawInspector(behaviorModule);
            }
        }

        [Flags]
        private enum GameObjectUnknownFlags
        {
            None = 0,
            Unknown1 = 1,
            Unknown2 = 2,
            Unknown4 = 4,
            Unknown8 = 8,
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            GameContext.GameClient.DestroyDrawable(Drawable);

            base.Dispose(disposeManagedResources);
        }
    }

    public record struct CashEvent(int Amount, ColorRgb Color, Vector3 Offset = default);

    public sealed class ObjectVeterancyHelper : IPersistableObject
    {
        public VeterancyLevel VeterancyLevel;
        public int ExperiencePoints;
        private uint _experienceSinkObjectId;
        public float ExperienceScalar = 1;

        public bool ShowRankUpAnimation;

        private readonly GameObject _gameObject;

        internal ObjectVeterancyHelper(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public void GainExperience(int experience)
        {
            if (_experienceSinkObjectId > 0)
            {
                var experienceSink = _gameObject.GameContext.GameLogic.GetObjectById(_experienceSinkObjectId);
                experienceSink?.GainExperience(experience);
                return;
            }

            ExperiencePoints += (int) (ExperienceScalar * experience);
            var nextRank = (int)VeterancyLevel + 1;

            if (_gameObject.Definition.ExperienceRequired == null || nextRank >= _gameObject.Definition.ExperienceRequired.Values.Length)
            {
                return; // nothing left for us to gain
            }

            var xpForNextRank =_gameObject.Definition.ExperienceRequired.Values[nextRank];
            if (ExperiencePoints >= xpForNextRank)
            {
                VeterancyLevel = (VeterancyLevel) nextRank;
                ShowRankUpAnimation = true;
                _gameObject.GameContext.AudioSystem.PlayAudioEvent(_gameObject,
                    _gameObject.GameContext.AssetLoadContext.AssetStore.MiscAudio.Current.UnitPromoted.Value);
            }
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistEnum(ref VeterancyLevel);
            reader.PersistInt32(ref ExperiencePoints);
            reader.PersistObjectID(ref _experienceSinkObjectId);
            reader.PersistSingle(ref ExperienceScalar);
        }
    }

    internal struct PolygonTriggerState : IPersistableObject
    {
        public PolygonTrigger PolygonTrigger;
        public bool EnteredThisFrame;
        private byte _unknown1;
        public bool IsInside;

        public void Persist(StatePersister reader)
        {
            var polygonTriggerName = PolygonTrigger?.Name;
            reader.PersistAsciiString(ref polygonTriggerName);

            if (reader.Mode == StatePersistMode.Read)
            {
                PolygonTrigger = reader.Game.Scene3D.MapFile.PolygonTriggers.GetPolygonTriggerByName(polygonTriggerName);
            }

            reader.PersistBoolean(ref EnteredThisFrame);

            reader.PersistByte(ref _unknown1); // encountered 1 for TrainCoal on Alpine Assault, not every time
            if (_unknown1 != 0 && _unknown1 != 1)
            {
                throw new InvalidStateException();
            }

            reader.PersistBoolean(ref IsInside);
        }
    }
}
