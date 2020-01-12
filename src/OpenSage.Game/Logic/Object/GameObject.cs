using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Content.Loaders;
using OpenSage.Data.Map;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.Logic.Object
{
    [DebuggerDisplay("[Object:{Definition.Name} ({Owner})]")]
    public sealed class GameObject : DisposableBase
    {
        internal static GameObject FromMapObject(
            MapObject mapObject,
            IReadOnlyList<Team> teams,
            AssetStore assetStore,
            GameObjectCollection parent,
            in Vector3 position)
        {
            var gameObject = parent.Add(mapObject.TypeName);

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

            if (mapObject.Properties.TryGetValue("objectSelectable", out var selectable))
            {
                gameObject.IsSelectable = (bool) selectable.Value;
            }

            gameObject.Transform.Translation = position;
            gameObject.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, mapObject.Angle);

            if (gameObject.Definition.IsBridge)
            {
                BridgeTowers.CreateForLandmarkBridge(assetStore, parent, gameObject, mapObject);
            }

            return gameObject;
        }

        internal void CopyModelConditionFlags(BitArray<ModelConditionFlag> newFlags)
        {
            ModelConditionFlags.CopyFrom(newFlags);
        }

        private readonly GameObject _rallyPointMarker;

        public ObjectDefinition Definition { get; }

        public Transform Transform { get; }

        public IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; }

        public BitArray<ModelConditionFlag> ModelConditionFlags { get; private set; }

        public IReadOnlyList<DrawModule> DrawModules { get; }

        public IReadOnlyList<BehaviorModule> BehaviorModules { get; }

        public BodyModule Body { get; }

        public Collider Collider { get; }

        public Player Owner { get; set; }

        public Team Team { get; set; }

        public bool IsSelectable { get; set; }

        public bool IsSelected { get; set; }
        public Vector3? RallyPoint { get; set; }

        private Locomotor CurrentLocomotor { get; set; }
        private Weapon CurrentWeapon { get; set; }

        public List<Vector3> TargetPoints { get; set; }

        private TimeSpan ConstructionStart { get; set; }

        public float BuildProgress { get; set; }

        private Navigation.Navigation Navigation { get; set; }

        public bool Destroyed { get; set; }

        public bool Damaged { get; set; }

        public float Speed { get; set; }
        public float Lift { get; set; }

        public bool IsPlacementPreview { get; set; }

        public bool IsPlacementInvalid { get; set; }

        public GameObjectCollection Parent { get; private set; }

        public ProductionUpdate ProductionUpdate { get; }
        public GameObject TargetEnemy { get; }
        public List<UpgradeTemplate> Upgrades { get; }
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal GameObject(ObjectDefinition objectDefinition, AssetLoadContext loadContext, Player owner,
                            GameObjectCollection parent, Navigation.Navigation navigation)
        {
            if (objectDefinition == null)
            {
                throw new ArgumentNullException(nameof(objectDefinition));
            }

            Definition = objectDefinition;
            Owner = owner;
            Parent = parent;
            Navigation = navigation;

            SetLocomotor();
            SetWeapon();
            Transform = Transform.CreateIdentity();

            var drawModules = new List<DrawModule>();
            foreach (var drawData in objectDefinition.Draws)
            {
                var drawModule = AddDisposable(drawData.CreateDrawModule(loadContext));
                if (drawModule != null)
                {
                    // TODO: This will never be null once we've implemented all the draw modules.
                    drawModules.Add(drawModule);
                }
            }
            DrawModules = drawModules;

            var behaviors = new List<BehaviorModule>();
            foreach (var behaviorData in objectDefinition.Behaviors)
            {
                var module = AddDisposable(behaviorData.CreateModule(this));
                if (module != null)
                {
                    // TODO: This will never be null once we've implemented all the draw modules.
                    behaviors.Add(module);
                }
            }
            BehaviorModules = behaviors;

            ProductionUpdate = FindBehavior<ProductionUpdate>();

            Body = AddDisposable(objectDefinition.Body?.CreateBodyModule(this));

            Collider = Collider.Create(objectDefinition, Transform);

            ModelConditionStates = drawModules
                .SelectMany(x => x.ModelConditionStates)
                .Distinct()
                .OrderBy(x => x.NumBitsSet)
                .ToList();

            ModelConditionFlags = new BitArray<ModelConditionFlag>();

            IsSelectable = Definition.KindOf?.Get(ObjectKinds.Selectable) ?? false;
            TargetPoints = new List<Vector3>();

            if (Definition.KindOf?.Get(ObjectKinds.AutoRallyPoint) ?? false)
            {
                var rpMarkerDef = loadContext.AssetStore.ObjectDefinitions.GetByName("RallyPointMarker");
                _rallyPointMarker = AddDisposable(new GameObject(rpMarkerDef, loadContext, owner, parent, navigation));
            }

            Upgrades = new List<UpgradeTemplate>();
        }

        internal IEnumerable<AttachedParticleSystem> GetAllAttachedParticleSystems()
        {
            foreach (var drawModule in DrawModules)
            {
                foreach (var attachedParticleSystem in drawModule.GetAllAttachedParticleSystems())
                {
                    yield return attachedParticleSystem;
                }
            }
        }

        internal void LogicTick(ulong frame, in TimeInterval time)
        {
            foreach (var behavior in BehaviorModules)
            {
                behavior.Update(time);
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
            if (naturalRallyPoint != null)
            {
                spawnedUnit.AddTargetPoint(ToWorldspace(naturalRallyPoint.Value));
            }

            // Then go to the rally point if it exists
            if (RallyPoint.HasValue)
            {
                spawnedUnit.AddTargetPoint(RallyPoint.Value);
            }
        }

        internal void AddTargetPoint(Vector3 targetPoint)
        {
            if (Definition.KindOf == null) return;

            if (Definition.KindOf.Get(ObjectKinds.Infantry)
                || Definition.KindOf.Get(ObjectKinds.Vehicle))
            {
                var start = TargetPoints.Count > 0 ? TargetPoints.Last() : Transform.Translation;
                var path = Navigation.CalculatePath(start, targetPoint);
                TargetPoints.AddRange(path);
                Logger.Debug("Set new target points: " + TargetPoints.Count);
            }

            ModelConditionFlags.SetAll(false);
            ModelConditionFlags.Set(ModelConditionFlag.Moving, true);
        }

        internal void SetTargetPoint(Vector3 targetPoint)
        {
            TargetPoints.Clear();

            AddTargetPoint(targetPoint);
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
        }

        public bool IsBeingConstructed()
        {
            return ModelConditionFlags.Get(ModelConditionFlag.ActivelyBeingConstructed) ||
                   ModelConditionFlags.Get(ModelConditionFlag.AwaitingConstruction) ||
                   ModelConditionFlags.Get(ModelConditionFlag.PartiallyConstructed);
        }

        internal void LocalLogicTick(in TimeInterval gameTime, float tickT, HeightMap heightMap)
        {
            var deltaTime = (float) gameTime.DeltaTime.TotalSeconds;

            // Check if the unit is currently moving
            if (ModelConditionFlags.Get(ModelConditionFlag.Moving) && TargetPoints.Count > 0)
            {
                CurrentLocomotor.LocalLogicTick(gameTime, TargetPoints, heightMap);
                var distance = Vector2.Distance(Transform.Translation.Vector2XY(), TargetPoints[0].Vector2XY());
                if (distance < 0.5f)
                {
                    Logger.Debug($"Reached point {TargetPoints[0]}");
                    TargetPoints.RemoveAt(0);
                    if (TargetPoints.Count == 0)
                    {
                        ClearModelConditionFlags();
                        Speed = 0;
                    }
                }
            }

            if (ModelConditionFlags.Get(ModelConditionFlag.Attacking))
            {
                CurrentWeapon.LocalLogicTick(gameTime, TargetEnemy);
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
                drawModule.Update(gameTime, this);
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

        public void OnLocalMove(AudioSystem gameAudio)
        {
            var audioEvent = Definition.VoiceMove?.Value;
            if (audioEvent != null)
            {
                gameAudio.PlayAudioEvent(audioEvent);
            }
        }

        private void SetLocomotor()
        {
            var locomotorSet = Definition.LocomotorSets.Find(x => x.Condition == LocomotorSetCondition.Normal);
            CurrentLocomotor = (locomotorSet != null)
                ? new Locomotor(this, locomotorSet)
                : null;
        }

        private void SetWeapon()
        {
            //TODO: we always pick the weapon without any conditions
            var weaponSet = Definition.WeaponSets.Find(x => x.Conditions.AnyBitSet == false);
            CurrentWeapon = (weaponSet != null)
                ? new Weapon(this, weaponSet)
                : null;
        }
        public void Upgrade(UpgradeTemplate upgrade)
        {
            if (upgrade.AcademyClassify == AcademyType.Superpower)
            {
                Upgrades.Add(upgrade);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
