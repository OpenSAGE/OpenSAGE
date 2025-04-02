using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using FixedMath.NET;
using ImGuiNET;
using OpenSage.Audio;
using OpenSage.Client;
using OpenSage.Data.Map;
using OpenSage.DataStructures;
using OpenSage.Diagnostics;
using OpenSage.Diagnostics.Util;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.InGame;
using OpenSage.Logic.Map;
using OpenSage.Logic.Object.Helpers;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.Logic.Object;

[DebuggerDisplay("[Object:{Definition.Name} ({Owner})]")]
public sealed class GameObject : Entity, IInspectable, ICollidable, IPersistableObject
{
    internal static GameObject FromMapObject(
        MapObject mapObject,
        GameEngine gameContext,
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
            if (mapObject.Properties.TryGetValue("objectInitialHealth", out var health))
            {
                _body.SetInitialHealth((int)health.Value);
            }
        }

        if (mapObject.Properties.TryGetValue("objectName", out var objectName))
        {
            Name = (string)objectName.Value;
        }

        if (mapObject.Properties.TryGetValue("objectSelectable", out var selectable))
        {
            IsSelectable = (bool)selectable.Value;
        }

        // TODO: handle "align to terrain" property
        var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, overwriteAngle ?? mapObject.Angle);
        var rotationOffset = useRotationAnchorOffset
            ? Vector4.Transform(new Vector4(Definition.RotationAnchorOffset.X, Definition.RotationAnchorOffset.Y, 0.0f, 1.0f), rotation)
            : Vector4.UnitW;
        var position = mapObject.Position + rotationOffset.ToVector3();
        var height = _gameEngine.Scene3D.Terrain.HeightMap.GetHeight(position.X, position.Y) + mapObject.Position.Z;
        UpdateTransform(new Vector3(position.X, position.Y, height), rotation,
            Definition.Scale);

        if (Definition.IsBridge)
        {
            BridgeTowers.CreateForLandmarkBridge(_gameEngine, this);
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

    private ObjectId _id;

    public ObjectId Id
    {
        get => _id;
        internal set
        {
            var oldObjectId = _id;

            _id = value;

            _gameEngine.GameLogic.OnObjectIdChanged(this, oldObjectId);
        }
    }

    public Percentage ProductionModifier { get; set; } = new Percentage(1);
    public Fix64 HealthModifier { get; set; }

    private readonly GameEngine _gameEngine;

    private readonly BehaviorUpdateContext _behaviorUpdateContext;

    internal BitArray<WeaponSetConditions> WeaponSetConditions;
    private readonly WeaponSet _weaponSet;
    public WeaponSet ActiveWeaponSet => _weaponSet;

    public readonly ObjectDefinition Definition;

    public PartitionObject PartitionObject { get; internal set; }

    public readonly Geometry Geometry;

    public void SetGeometryInfoZ(float newZ)
    {
        // A z-only change does not need to un/register with PartitionManager.
        Geometry.SetMaxHeightAbovePosition(newZ);

        // TODO(Port): Implement this.
        //Drawable?.ReactToGeometryChange();
    }

    public readonly Transform ModelTransform;

    private bool _objectMoved;

    public ObjectId CreatedByObjectID;
    public ObjectId BuiltByObjectID;

    private BitArray<ObjectStatus> _status = new();
    private byte _scriptStatus;
    private ObjectPrivateStatusFlags _privateStatus;
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
    public readonly ExperienceTracker ExperienceTracker;
    private ObjectId _containerId;
    public ObjectId ContainerId => _containerId;
    private uint _containedFrame;

    public GameObject ContainedBy => _containerId.IsValid
        ? _gameEngine.GameLogic.GetObjectById(_containerId)
        : null;

    private string _teamName;
    private uint _enteredOrExitedPolygonTriggerFrame;
    private Point3D _integerPosition;
    private PolygonTriggerState[] _polygonTriggersState;
    private int _unknown5;
    private uint _unknownFrame;
    public ObjectId HealedByObjectId;
    public uint HealedEndFrame;
    private BitArray<WeaponBonusType> _weaponBonusTypes = new();
    private byte _weaponSomethingPrimary;
    private byte _weaponSomethingSecondary;
    private byte _weaponSomethingTertiary;
    private BitArray<SpecialPowerType> _specialPowers = new();

    protected override void OnEntityMoved()
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

    private readonly StatusDamageHelper _statusDamageHelper;
    private readonly SubdualDamageHelper _subdualDamageHelper;

    public IContainModule Contain { get; }

    public PhysicsBehavior Physics { get; }

    private readonly BodyModule _body;
    public BodyModule BodyModule => _body;
    public bool HasActiveBody() => _body is ActiveBody;

    // TODO(Port): Implement this.
    public bool IsFactionStructure => false;

    public bool IsEffectivelyDead
    {
        get => GetPrivateStatus(ObjectPrivateStatusFlags.EffectivelyDead);
        set
        {
            SetPrivateStatus(ObjectPrivateStatusFlags.EffectivelyDead, value);
            if (value)
            {
                // TODO(Port): Implement this.
                //if (_radarData != null)
                //{
                //    _gameEngine.Radar.RemoveGameObject(this);
                //}
            }
        }
    }

    public DamageInfoOutput AttemptDamage(in DamageInfoInput damageInput)
    {
        var damageOutput = _body?.AttemptDamage(damageInput) ?? default;

        // TODO(Port): shockwave and radar stuff.

        return damageOutput;
    }

    public DamageInfoOutput AttemptHealing(float amount, GameObject source)
    {
        return _body?.AttemptDamage(new DamageInfoInput(source)
        {
            DamageType = DamageType.Healing,
            DeathType = DeathType.None,
            Amount = amount,
        }) ?? default;
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
            _gameEngine.GameLogic.AddNameLookup(this);
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

    public VeterancyLevel Rank
    {
        get => ExperienceTracker.VeterancyLevel;
        set
        {
            ExperienceTracker.SetVeterancyLevel(value);
        }
    }

    public int ExperienceRequiredForNextLevel { get; set; }

    public int EnergyProduction { get; internal set; }

    // TODO(Port): Actually implement and use this.
    private PathfindLayerType _layer = PathfindLayerType.Ground;
    public PathfindLayerType Layer
    {
        get => _layer;
        set => _layer = value;
    }

    // TODO(Port): Cache this, just like Thing::getHeightAboveTerrain().
    public float HeightAboveTerrain
    {
        get
        {
            var pos = Transform.Translation;
            var terrainZ = _gameEngine.Game.TerrainLogic.GetLayerHeight(pos.X, pos.Y, Layer);
            return pos.Z - terrainZ;
        }
    }

    // TODO(Port): Cache this as well
    public float HeightAboveTerrainOrWater
    {
        get
        {
            var pos = Transform.Translation;
            if (_gameEngine.Game.TerrainLogic.IsUnderwater(pos.X, pos.Y, out var waterZ))
            {
                return pos.Z - waterZ;
            }
            else
            {
                return HeightAboveTerrain;
            }
        }
    }

    public bool IsAboveTerrain => HeightAboveTerrain > 0.0f;
    public bool IsAboveTerrainOrWater => HeightAboveTerrainOrWater > 0.0f;

    /// <summary>
    /// Original comment (which I don't totally understand):
    ///
    /// > If we treat this as airborne, then they slide down slopes.  This checks whether
    /// > they are high enough that we should let them act like they're flying.
    ///
    /// If it's high enough that it will take more than 3 frames to return to the ground,
    /// then it's significantly airborne. We calculate the distance we can fall in 3 frames
    /// with d = g * t^2. Gravity is negative, so we must negate it here.
    /// </summary>
    public bool IsSignificantlyAboveTerrain => HeightAboveTerrain > -_gameEngine.AssetStore.GameData.Current.Gravity * MathUtility.Square(3);

    // TODO(Port): Implement this.
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public float CarrierDeckHeight => 0;

    public readonly Drawable Drawable;

    /// <summary>
    /// A non-persisted queue of rising-cash-text items that should originate from this GameObject.
    /// </summary>
    public CashEvent? ActiveCashEvent { get; set; }

    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    internal GameObject(
        ObjectDefinition objectDefinition,
        GameEngine gameContext,
        Player owner)
    {
        if (objectDefinition.BuildVariations != null && objectDefinition.BuildVariations.Count() > 0)
        {
            objectDefinition = objectDefinition.BuildVariations[gameContext.GameLogic.Random.Next(0, objectDefinition.BuildVariations.Length - 1)].Value;
        }

        _objectMoved = true;
        Hidden = false;

        Definition = objectDefinition ?? throw new ArgumentNullException(nameof(objectDefinition));
        ExperienceTracker = new ExperienceTracker(this, gameContext);

        _attributeModifiers = new Dictionary<string, AttributeModifier>();
        _gameEngine = gameContext;
        Owner = owner ?? gameContext.Game.PlayerManager.GetCivilianPlayer();

        _behaviorUpdateContext = new BehaviorUpdateContext(gameContext, this);

        _weaponSet = new WeaponSet(this, _gameEngine);
        WeaponSetConditions = new BitArray<WeaponSetConditions>();
        UpdateWeaponSet();

        ModelTransform = Transform.CreateIdentity();
        Transform.Scale = Definition.Scale;

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

        AddBehavior("ModuleTag_SMCHelper", new ObjectSpecialModelConditionHelper(this, gameContext));

        if (objectDefinition.KindOf.Get(ObjectKinds.CanBeRepulsed))
        {
            AddBehavior("ModuleTag_RepulsorHelper", new ObjectRepulsorHelper(this, gameContext));
        }

        // TODO: This shouldn't be added to all objects. I don't know what the rule is.
        if (_gameEngine.Game.SageGame >= SageGame.CncGeneralsZeroHour)
        {
            AddBehavior("ModuleTag_StatusDamageHelper", _statusDamageHelper = new StatusDamageHelper(this, gameContext));
            AddBehavior("ModuleTag_SubdualDamageHelper", _subdualDamageHelper = new SubdualDamageHelper(this, gameContext));
        }

        // TODO: This shouldn't be added to all objects. I don't know what the rule is.
        // Maybe KindOf = CAN_ATTACK ?
        AddBehavior("ModuleTag_DefectionHelper", new ObjectDefectionHelper(this, gameContext));

        // TODO: This shouldn't be added to all objects. I don't know what the rule is.
        // Probably only those with weapons.
        AddBehavior("ModuleTag_WeaponStatusHelper", new ObjectWeaponStatusHelper(this, gameContext));

        // TODO: This shouldn't be added to all objects. I don't know what the rule is.
        // Probably only those with weapons.
        AddBehavior("ModuleTag_FiringTrackerHelper", new ObjectFiringTrackerHelper(this, gameContext));

        // TODO: This shouldn't be added to all objects. I don't know what the rule is.
        if (_gameEngine.Game.SageGame is not SageGame.CncGenerals and not SageGame.CncGeneralsZeroHour)
        {
            // this was added in bfme and is not present in generals or zero hour
            AddBehavior("ModuleTag_ExperienceHelper", new ExperienceUpdate(this, gameContext));
        }

        // TODO: This shouldn't be added to all objects. I don't know what the rule is.
        if (_gameEngine.Game.SageGame >= SageGame.CncGeneralsZeroHour)
        {
            AddBehavior("ModuleTag_TempWeaponBonusHelper", new TempWeaponBonusHelper(this, gameContext));
        }

        foreach (var behaviorDataContainer in objectDefinition.Behaviors.Values)
        {
            var behaviorModuleData = (BehaviorModuleData)behaviorDataContainer.Data;
            var module = AddDisposable(behaviorModuleData.CreateModule(this, gameContext));

            // TODO: This will never be null once we've implemented all the behaviors.
            if (module != null)
            {
                AddBehavior(behaviorDataContainer.Tag, module);

                switch (module)
                {
                    case BodyModule body:
                        _body = body;
                        break;

                    case IContainModule contain:
                        Contain = contain;
                        break;

                    case AIUpdate aiUpdate:
                        AIUpdate = aiUpdate;
                        break;

                    case ProductionUpdate productionUpdate:
                        ProductionUpdate = productionUpdate;
                        break;

                    case PhysicsBehavior physics:
                        Physics = physics;
                        break;
                }
            }
        }

        // Allow for inter-module resolution.
        foreach (var behavior in _behaviorModules)
        {
            behavior.OnObjectCreated();
        }

        Geometry = Definition.Geometry.Clone();

        Colliders = new List<Collider>();
        foreach (var geometry in Geometry.Shapes)
        {
            Colliders.Add(Collider.Create(geometry, Transform));
        }

        RoughCollider = Collider.Create(Colliders);

        _visionRange = Definition.VisionRange;
        _shroudRevealSomething1.VisionRange = Definition.VisionRange;
        _shroudClearingRange = Definition.ShroudClearingRange;

        IsSelectable = Definition.KindOf.Get(ObjectKinds.Selectable);
        CanAttack = Definition.KindOf.Get(ObjectKinds.CanAttack);

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
            if (_gameEngine.Scene3D.Game.SageGame == SageGame.CncGenerals ||
                _gameEngine.Scene3D.Game.SageGame == SageGame.CncGenerals)
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
                _gameEngine.Navigation.UpdateAreaPassability(collider, false);
            }
        }
        Colliders.AddRange(newColliders);
        RoughCollider = Collider.Create(Colliders);
        _gameEngine.Quadtree.Update(this);
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
                    _gameEngine.Navigation.UpdateAreaPassability(Colliders[i], true);
                }
                Colliders.RemoveAt(i);
            }
        }
        RoughCollider = Collider.Create(Colliders);
        _gameEngine.Quadtree.Update(this);

        if (AffectsAreaPassability)
        {
            _gameEngine.Navigation.UpdateAreaPassability(this, false);
        }
    }

    public void UpdateColliders()
    {
        RoughCollider.Update(Transform);
        foreach (var collider in Colliders)
        {
            collider.Update(Transform);
        }
        _gameEngine.Quadtree.Update(this);
    }

    internal void LogicTick(in TimeInterval time)
    {
        if (_objectMoved)
        {
            UpdateColliders();

            if (AffectsAreaPassability)
            {
                _gameEngine.Navigation.UpdateAreaPassability(this, false);
            }

            _objectMoved = false;
        }

        if (ModelConditionFlags.Get(ModelConditionFlag.Attacking))
        {
            CurrentWeapon?.LogicTick();
        }
        if (ModelConditionFlags.Get(ModelConditionFlag.Sold))
        {
            Kill();
            ModelConditionFlags.Set(ModelConditionFlag.Sold, false);
        }

        foreach (var (key, modifier) in _attributeModifiers)
        {
            if (!modifier.Applied)
            {
                modifier.Apply(this, _gameEngine, time);
            }
            else if (modifier.Invalid || modifier.Expired(time))
            {
                modifier.Remove(this, _gameEngine);
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

        // TODO(Port): Implement this properly.

        void RunUpdate(UpdateOrder updatePhase)
        {
            foreach (var behavior in _behaviorModules)
            {
                if (IsEffectivelyDead && behavior is not SlowDeathBehavior and not LifetimeUpdate and not DeletionUpdate)
                {
                    continue; // if we're dead, we should only update SlowDeathBehavior, LifetimeUpdate, or DeletionUpdate
                }
                if (behavior is IUpdateModule updateModule && updateModule.UpdatePhase == updatePhase)
                {
                    updateModule.Update(_behaviorUpdateContext);
                }
            }
        }

        RunUpdate(UpdateOrder.Order0);
        RunUpdate(UpdateOrder.Order1);
        RunUpdate(UpdateOrder.Order2);
        RunUpdate(UpdateOrder.Order3);
    }

    public bool CanRecruitHero(ObjectDefinition definition)
    {
        foreach (var obj in _gameEngine.GameLogic.Objects)
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

    // TODO(Port): Remove this overload.
    internal void OnCollide(GameObject other)
    {
        OnCollide(other, Vector3.Zero, Vector3.Zero);
    }

    internal void OnCollide(GameObject other, in Vector3 location, in Vector3 normal)
    {
        Logger.Info($"GameObject {Definition.Name} colliding with {other?.Definition.Name ?? "Ground"}");

        foreach (var behavior in _behaviorModules)
        {
            if (behavior is ICollideModule collideModule)
            {
                collideModule.OnCollide(other, location, normal);
            }
        }
    }

    internal void OnDestroy()
    {
        foreach (var behavior in _behaviorModules)
        {
            behavior.OnDestroy();
        }
    }

    internal Vector3 ToWorldspace(in Vector3 localPos)
    {
        var worldPos = Vector4.Transform(new Vector4(localPos, 1.0f), Transform.Matrix);
        return new Vector3(worldPos.X, worldPos.Y, worldPos.Z);
    }

    internal Transform ToWorldspace(in Transform localPos)
    {
        var worldPos = localPos.Matrix * Transform.Matrix;
        return new Transform(worldPos);
    }

    public Vector3 UnitDirectionVector2D
    {
        get
        {
            // TODO: Cache this.
            var angle = Transform.Yaw;
            return new Vector3(
                MathF.Cos(angle),
                MathF.Sin(angle),
                0.0f);
        }
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

    public bool TestStatus(ObjectStatus status) => _status.Get(status);

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

        // TODO: This code should end up in DozerAIUpdate.Construct.
        // Newly constructed objects start at one hit point.
        BodyModule.InternalChangeHealth(-BodyModule.Health + 1.0f);

        ClearModelConditionFlags();

        ModelConditionFlags.Set(ModelConditionFlag.ActivelyBeingConstructed, false);
        ModelConditionFlags.Set(ModelConditionFlag.AwaitingConstruction, true);
        ModelConditionFlags.Set(ModelConditionFlag.PartiallyConstructed, false);

        _status.Set(ObjectStatus.UnderConstruction, true);

        // flatten terrain around object
        var centerPosition = _gameEngine.Terrain.HeightMap.GetTilePosition(Transform.Translation);

        if (centerPosition == null)
        {
            throw new InvalidStateException("object built outside of map bounds");
        }

        var (centerX, centerY) = centerPosition.Value;
        var maxHeight = _gameEngine.Terrain.HeightMap.GetHeight(centerX, centerY);
        // on slopes, the height map isn't always exactly where our cursor is (our cursor is a float, and the heightmap is always a ushort), so snap the building to the nearest heightmap value
        UpdateTransform(Transform.Translation with { Z = maxHeight }, Transform.Rotation);

        // clear anything applicable in the build area (trees, rubble, etc)
        var toDelete = new BitArray<ObjectKinds>(ObjectKinds.Shrubbery, ObjectKinds.ClearedByBuild); // this may not be completely accurate
        foreach (var intersecting in _gameEngine.Quadtree.FindIntersecting(ShapedCollider))
        {
            if (intersecting.Definition.KindOf.Intersects(toDelete))
            {
                _gameEngine.GameLogic.DestroyObject(intersecting);
            }
        }

        _gameEngine.Terrain.SetMaxHeight(ShapedCollider, maxHeight);
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
        var newHealth = (BuildProgress - lastBuildProgress) * _body.MaxHealth;
        AttemptHealing(newHealth, null);

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

    internal void LocalLogicTick(in TimeInterval gameTime, float tickT, HeightMap heightMap)
    {
        Drawable.LogicTick(gameTime);
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
        var worldMatrix = ModelTransform.Matrix * Transform.Matrix;
        Drawable.BuildRenderList(renderList, camera, gameTime, worldMatrix, renderItemConstantsPS);
    }

    public void ClearModelConditionState(ModelConditionFlag state)
    {
        ModelConditionFlags.Set(state, false);
    }

    public void ClearModelConditionFlags()
    {
        ModelConditionFlags.SetAll(false);
        // todo: maintain map-based flags, such as NIGHT, SNOW, etc
    }

    public void SetModelConditionState(ModelConditionFlag state)
    {
        ModelConditionFlags.Set(state, true);
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
        return Translation.Z - _gameEngine.Terrain.HeightMap.GetHeight(Translation.X, Translation.Y) > groundDelta;
    }

    public bool IsUsingAirborneLocomotor()
    {
        if (AIUpdate.CurrentLocomotor == null)
        {
            return false;
        }

        return AIUpdate.CurrentLocomotor.LocomotorTemplate.Surfaces.HasFlag(Surfaces.Air);
    }

    // TODO(Port): Actually set _privateStatus.
    public bool IsOffMap => (_privateStatus & ObjectPrivateStatusFlags.OffMap) != 0;

    /// <summary>
    /// Kills the object with an optional type of damage and death.
    /// </summary>
    public void Kill(DamageType damageType = DamageType.Unresistable, DeathType deathType = DeathType.Normal)
    {
        // Do unmodifiable damage equal to their max health to kill.
        var damageOutput = AttemptDamage(new DamageInfoInput
        {
            DamageType = damageType,
            DeathType = deathType,
            Amount = _body.MaxHealth,
            Kill = true, // Triggers object to die no matter what
        });

        Debug.Assert(!damageOutput.NoEffect, "Attempting to kill an unKillable object (InactiveBody?)");
    }

    internal void OnDie(in DamageInfoInput damageInput)
    {
        // TODO(Port): Port this from Object::onDie().

        Logger.Info("Object dying " + damageInput.DeathType);
        bool construction = IsBeingConstructed();

        if (construction)
        {
            ModelConditionFlags.Set(ModelConditionFlag.DestroyedWhilstBeingConstructed, true);

            var mostRecentConstructor = _gameEngine.GameLogic.GetObjectById(BuiltByObjectID);
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
            ExecuteRandomSlowDeathBehavior(damageInput);
        }

        foreach (var module in FindBehaviors<IDieModule>())
        {
            module.OnDie(damageInput);
        }

        PlayDieSound(damageInput.DeathType);
    }

    public void DoStatusDamage(ObjectStatus status, LogicFrameSpan duration)
    {
        _statusDamageHelper?.DoStatusDamage(status, duration);
    }

    public void NotifySubdualDamage(float amount)
    {
        _subdualDamageHelper?.NotifySubdualDamage(amount);

        // If we are gaining subdual damage, we are slowly tinting.
        if (Drawable != null)
        {
            // TODO(Port): Implement this:

            //if (amount > 0)
            //    getDrawable()->setTintStatus(TINT_STATUS_GAINING_SUBDUAL_DAMAGE);
            //else
            //    getDrawable()->clearTintStatus(TINT_STATUS_GAINING_SUBDUAL_DAMAGE);
        }
    }

    /// <summary>
    /// Removes this object from the scene without playing any death effects or affecting stats.
    /// </summary>
    public void Destroy()
    {
        _gameEngine.GameLogic.DestroyObject(this);
    }

    private void ExecuteRandomSlowDeathBehavior(in DamageInfoInput damageInput)
    {
        var damageInputCopy = damageInput;

        // If there are multiple SlowDeathBehavior modules,
        // we need to use ProbabilityModifier to choose between them.
        var slowDeathBehaviors = FindBehaviors<SlowDeathBehavior>()
            .Where(x => x.IsDieApplicable(damageInputCopy))
            .ToList();

        var sumProbabilityModifiers = slowDeathBehaviors.Sum(x => x.ProbabilityModifier);
        var random = _gameEngine.GameLogic.Random.Next(0, sumProbabilityModifiers - 1);
        var cumulative = 0;
        foreach (var deathBehavior in slowDeathBehaviors)
        {
            cumulative += deathBehavior.ProbabilityModifier;
            if (random < cumulative)
            {
                ((IDieModule)deathBehavior).OnDie(damageInput);
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
            _gameEngine.AudioSystem.PlayAudioEvent(this, voiceDie);
        }
    }

    public void SetSelectable(bool selectable)
    {
        IsSelectable = selectable;
        _status.Set(ObjectStatus.Unselectable, !selectable);
    }

    internal void AddToContainer(ObjectId containerId)
    {
        _containerId = containerId;
        _containedFrame = _gameEngine.GameLogic.CurrentFrame.Value;
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
        _containerId = ObjectId.Invalid;
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
            if (disabledTypeFrame > LogicFrame.Zero && disabledTypeFrame < _gameEngine.GameLogic.CurrentFrame)
            {
                UnDisable((DisabledType)i);
            }
        }
    }

    public bool IsDisabledByType(DisabledType type) => _disabledTypes.Get(type);

    public void SetDisabled(DisabledType type)
    {
        // TODO(Port): Should be FOREVER. See Object::setDisabled()
        Disable(type, new LogicFrame(0x3fffffff));
    }

    public void ClearDisabled(DisabledType type)
    {
        // TODO(Port): Implement this properly. See Object::clearDisabled()
        UnDisable(type);
    }

    public void Topple(in Vector3 toppleDirection, float toppleSpeed, ToppleOptions options)
    {
        var toppleUpdate = FindBehavior<ToppleUpdate>();

        if (toppleUpdate != null && toppleUpdate.IsAbleToBeToppled)
        {
            toppleUpdate.ApplyTopplingForce(toppleDirection, toppleSpeed, options);
        }
    }

    public void SetCaptured(bool isCaptured)
    {
        if (!isCaptured)
        {
            throw new ArgumentException("Clearing captured status. This should never happen.");
        }

        SetPrivateStatus(ObjectPrivateStatusFlags.Captured, isCaptured);

        // No need to see if we should skip updates, this flag has no effect on skipping updates.
    }

    private bool GetPrivateStatus(ObjectPrivateStatusFlags flag) => (_privateStatus & flag) != 0;

    private void SetPrivateStatus(ObjectPrivateStatusFlags flag, bool value)
    {
        if (value)
        {
            _privateStatus |= flag;
        }
        else
        {
            _privateStatus &= ~flag;
        }
    }

    public byte CrusherLevel => (byte)Definition.CrusherLevel;

    public byte CrushableLevel => (byte)Definition.CrushableLevel;

    public bool CanCrushOrSquish(GameObject otherObj, CrushSquishTestType testType)
    {
        // TODO(Port): Implement this.
        return false;
    }

    public void Defect(Team newTeam, uint detectionTime)
    {
        // TODO(Port): Implement this.
    }

    internal void Sell()
    {
        ModelConditionFlags.Set(ModelConditionFlag.Sold, true);
        _status.Set(ObjectStatus.Unselectable, true);
        _status.Set(ObjectStatus.Sold, true);
        Owner.DeselectUnit(this);
    }

    internal void SetBeingHealed(GameObject healer, uint endFrame)
    {
        HealedByObjectId = healer.Id;
        HealedEndFrame = endFrame;
    }

    private void VerifyHealer()
    {
        if (HealedByObjectId.IsValid && _gameEngine.GameLogic.CurrentFrame.Value >= HealedEndFrame)
        {
            HealedByObjectId = ObjectId.Invalid;
            HealedEndFrame = 0; // todo: is this reset?
        }
    }

    public void AddWeaponBonusType(WeaponBonusType bonusType)
    {
        _weaponBonusTypes.Set(bonusType, true);
    }

    public void RemoveWeaponBonusType(WeaponBonusType bonusType)
    {
        _weaponBonusTypes.Set(bonusType, false);
    }

    public bool IsUndetectedDefector => _privateStatus.HasFlag(ObjectPrivateStatusFlags.UndetectedDefector);

    public RelationshipType GetRelationship(GameObject that)
    {
        if (Team == null || that == null)
        {
            return RelationshipType.Neutral;
        }

        if (IsUndetectedDefector)
        {
            // so my AI does not give away my position by auto acquire
            return RelationshipType.Neutral;
        }
        else if (that.IsUndetectedDefector)
        {
            // so I treat undetecteddefectors like they were my very own
            return RelationshipType.Allies;
        }
        else
        {
            return Team.GetRelationship(that.Team);
        }
    }

    public void OnVeterancyLevelChanged(VeterancyLevel oldLevel, VeterancyLevel newLevel, bool provideFeedback = true)
    {
        // TODO(Port): Implement this properly.

        var upgradeToApply = newLevel switch
        {
            VeterancyLevel.Veteran => VeterancyUpgradeNameVeteran,
            VeterancyLevel.Elite => VeterancyUpgradeNameElite,
            VeterancyLevel.Heroic => VeterancyUpgradeNameHeroic,
            _ => null,
        };
        // veterancy is only ever additive, and the inis allude to the fact that it is fine to skip upgrades and go straight from e.g. Veteran to Heroic
        if (upgradeToApply != null)
        {
            Upgrade(_gameEngine.Game.AssetStore.Upgrades.GetByName(upgradeToApply));
        }

        _body?.OnVeterancyLevelChanged(oldLevel, newLevel, provideFeedback);

        _gameEngine.AudioSystem.PlayAudioEvent(
            this,
            _gameEngine.AssetLoadContext.AssetStore.MiscAudio.Current.UnitPromoted.Value
        );
    }

    public void ScoreTheKill(GameObject victim)
    {
        // TODO(Port): Implement this.
    }

    public void Persist(StatePersister reader)
    {
        var version = reader.PersistVersion(9);

        var id = Id;
        reader.PersistObjectId(ref id, "ObjectId");
        if (reader.Mode == StatePersistMode.Read)
        {
            Id = id;
        }

        var transform = reader.Mode == StatePersistMode.Write
            ? Matrix4x3.FromMatrix4x4(Transform.Matrix)
            : Matrix4x3.Identity;
        reader.PersistMatrix4x3(ref transform);
        if (reader.Mode == StatePersistMode.Read)
        {
            SetTransformMatrix(transform.ToMatrix4x4());
        }

        var teamId = Team?.Id ?? 0u;
        reader.PersistUInt32(ref teamId);
        Team = _gameEngine.Game.TeamFactory.FindTeamById(teamId);

        Owner = Team.Template.Owner;

        reader.PersistObjectId(ref CreatedByObjectID);
        reader.PersistObjectId(ref BuiltByObjectID);

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

        reader.PersistByte(ref _scriptStatus);
        reader.PersistEnumByteFlags(ref _privateStatus);

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

        reader.PersistObject(ExperienceTracker);
        reader.PersistObjectId(ref _containerId);
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

        reader.PersistEnum(ref _layer);
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

        reader.PersistObjectId(ref HealedByObjectId);
        reader.PersistFrame(ref HealedEndFrame);
        reader.PersistBitArray(ref WeaponSetConditions);
        reader.PersistBitArrayAsUInt32(ref _weaponBonusTypes);

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
            _gameEngine.Scene3D.TacticalView.LookAt(Translation);
        }

        ImGui.SameLine();

        if (ImGui.Button("Select"))
        {
            _gameEngine.Scene3D.LocalPlayer.SelectUnits(new[] { this });
        }

        ImGui.SameLine();

        if (ImGui.Button("Kill"))
        {
            Kill(deathType: DeathType.Exploded);
        }

        if ((Definition.IsTrainable || Definition.BuildVariations?.Any(v => v.Value.IsTrainable) == true) &&
            ImGui.CollapsingHeader("Veterancy"))
        {
            ImGui.LabelText("Experience", ExperienceTracker.CurrentExperience.ToString());
            var rank = ExperienceTracker.VeterancyLevel;
            ImGuiUtility.ComboEnum("Current Rank", ref rank);
            Rank = rank;
        }

        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.LabelText("DisplayName", Definition.DisplayName ?? string.Empty);

            var translation = Transform.Translation;
            if (ImGui.DragFloat3("Position", ref translation))
            {
                Transform.Translation = translation;
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
    private enum ObjectPrivateStatusFlags : byte
    {
        None = 0,

        /// <summary>
        /// Object is effectively dead.
        /// </summary>
        EffectivelyDead = 1,

        /// <summary>
        /// Set to true when object defects from its team.
        /// Set to false when object attacks anything or when time runs out.
        /// </summary>
        UndetectedDefector = 2,

        /// <summary>
        /// Set to true if object has been captured. Otherwise, it's false.
        /// Note: it never becomes false once it's true.
        /// </summary>
        Captured = 4,

        /// <summary>
        /// Set to true if object is known to be off the current map.
        /// </summary>
        OffMap = 8,
    }

    protected override void Dispose(bool disposeManagedResources)
    {
        _gameEngine.GameClient.DestroyDrawable(Drawable);

        base.Dispose(disposeManagedResources);
    }
}

public record struct CashEvent(int Amount, ColorRgb Color, Vector3 Offset = default);

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

// Pathfind layers - ground is the first layer, each bridge is another. jba.
// Layer 1 is the ground.
// Layer 2 is the top layer - bridge if one is present, ground otherwise.
// Layer 2 - LAYER_LAST -1 are bridges.
// Layer_WALL is a special "wall" layer for letting units run aroound on top of a wall
// made of structures.
// Note that the bridges just index in the pathfinder, so you don't actually
// have a LAYER_BRIDGE_1 enum value.
public enum PathfindLayerType
{
    Invalid = 0,
    Ground = 1,
    Wall = 15,
}

public enum CrushSquishTestType
{
    TestCrushOnly,
    TestSquishOnly,
    TestCrushOrSquish,
}

public readonly record struct PlayerMaskType(ushort Value)
{
    public static readonly PlayerMaskType None = new(0xFFFF);
    public static readonly PlayerMaskType All = new(0xFFFF);
}
