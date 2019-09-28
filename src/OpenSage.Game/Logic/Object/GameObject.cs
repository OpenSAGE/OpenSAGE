using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Content.Loaders;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic.Object.Production;
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

            // TODO: If the object doesn't have a health value, how do we initialise it?
            if (gameObject.Definition.Body is ActiveBodyModuleData body)
            {
                var healthMultiplier = mapObject.Properties.TryGetValue("objectInitialHealth", out var health)
                    ? (uint) health.Value / 100.0f
                    : 1.0f;

                // TODO: Should we use InitialHealth or MaximumHealth here?
                var initialHealth = body.InitialHealth * healthMultiplier;
                gameObject.Health = (decimal) initialHealth;
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
            UpdateDrawModuleConditionStates();
        }

        public ObjectDefinition Definition { get; }

        public Transform Transform { get; }

        public IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; }

        public BitArray<ModelConditionFlag> ModelConditionFlags { get; private set; }

        public IReadOnlyList<DrawModule> DrawModules { get; }

        public Collider Collider { get; }

        // TODO: This could use a smaller fixed point type.
        public decimal Health { get; set; }

        public Player Owner { get; set; }

        public Team Team { get; set; }

        public bool IsSelectable { get; set; }

        public bool IsSelected { get; set; }

        public Vector3 RallyPoint { get; set; }

        private LocomotorTemplate CurrentLocomotorTemplate { get; set; }

        private Vector3? TargetPoint { get; set; }
        private float TargetAngle { get; set; }

        private TimeSpan ConstructionStart { get; set; }

        public bool Destroyed { get; set; }

        public GameObjectCollection Parent { get; private set; }

        internal GameObject(ObjectDefinition objectDefinition, AssetLoadContext loadContext, Player owner, GameObjectCollection parent)
        {
            if (objectDefinition == null)
            {
                throw new ArgumentNullException(nameof(objectDefinition));
            }

            Definition = objectDefinition;
            Owner = owner;
            Parent = parent;

            SetLocomotor();
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

            Collider = Collider.Create(objectDefinition, Transform);

            ModelConditionStates = drawModules
                .SelectMany(x => x.ModelConditionStates)
                .Distinct()
                .OrderBy(x => x.NumBitsSet)
                .ToList();

            ModelConditionFlags = new BitArray<ModelConditionFlag>();
            UpdateDrawModuleConditionStates();

            IsSelectable = Definition.KindOf?.Get(ObjectKinds.Selectable) ?? false;
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

        internal void LogicTick(ulong frame)
        {
            // TODO: Update modules.
            HandleProduction();
        }

        public bool IsProducing => _productionQueue.Count > 0;

        private List<ProductionJob> _productionQueue = new List<ProductionJob>();
        public IReadOnlyList<ProductionJob> ProductionQueue => _productionQueue;

        private void HandleProduction()
        {
            if (!IsProducing)
            {
                return;
            }
            var current = _productionQueue.First();
            //todo: determine correct value for the production
            if (current.Produce(20) == ProductionJobResult.Finished)
            {
                _productionQueue.RemoveAt(0);
                switch (current.Type)
                {
                    case ProductionJobType.Unit:
                        this.Spawn(current.ObjectDefinition);
                        break;
                }
            }
        }

        internal void QueueProduction(ObjectDefinition objectDefinition)
        {
            var job = new ProductionJob(objectDefinition);
            _productionQueue.Add(job);
        }

        public void CancelProduction(int pos)
        {
            if (pos < _productionQueue.Count)
            {
                _productionQueue.RemoveAt(pos);
            }
        }



        internal void Spawn(ObjectDefinition objectDefinition)
        {
            var spawnedUnit = Parent.Add(objectDefinition, Owner);
            var translation = Transform.Translation;

            foreach (var behavior in Definition.Behaviors)
            {
                switch (behavior)
                {
                    case SupplyCenterProductionExitUpdateModuleData supplyCenterModuleData:
                        translation -= supplyCenterModuleData.UnitCreatePoint;
                        break;
                    case DefaultProductionExitUpdateModuleData defaultModuleData:
                        translation -= defaultModuleData.UnitCreatePoint;
                        break;
                }
            }

            spawnedUnit.Transform.Translation = translation;
            spawnedUnit.MoveTo(RallyPoint);
        }

        internal void MoveTo(Vector3 targetPos)
        {
            if (Definition.KindOf == null) return;

            if (Definition.KindOf.Get(ObjectKinds.Infantry)
                || Definition.KindOf.Get(ObjectKinds.Vehicle))
            {
                TargetPoint = targetPos;
                var delta = TargetPoint.Value - Transform.Translation;
                TargetAngle = (float) Math.Atan2(delta.Y - Vector3.UnitX.Y, delta.X - Vector3.UnitX.X);
            }

            ModelConditionFlags.SetAll(false);
            ModelConditionFlags.Set(ModelConditionFlag.Moving, true);
            UpdateDrawModuleConditionStates();
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
                UpdateDrawModuleConditionStates();
                ConstructionStart = gameTime.TotalTime;

                // TODO: This shouldn't be here. However, we have no better place to put it yet. Belongs in FinishConstruction.
                foreach (var behavior in Definition.Behaviors)
                {
                    if (behavior is SpawnBehaviorModuleData spawnBehaviorModuleData)
                    {
                        Spawn(spawnBehaviorModuleData.SpawnTemplate.Value);
                    }
                }
            }
        }

        internal void LocalLogicTick(in TimeInterval gameTime, float tickT, HeightMap heightMap)
        {
            var deltaTime = (float) gameTime.DeltaTime.TotalSeconds;

            // Check if the unit is currently moving
            if (ModelConditionFlags.Get(ModelConditionFlag.Moving) && TargetPoint.HasValue)
            {
                var x = Transform.Translation.X;
                var y = Transform.Translation.Y;
                var trans = Transform.Translation;

                // This locomotor speed is distance/second
                var delta = TargetPoint.Value - Transform.Translation;
                var distance = CurrentLocomotorTemplate.Speed * deltaTime;
                if (delta.Length() < distance) distance = delta.Length();

                var currentAngle = -Transform.EulerAngles.Z;
                var angleDelta = TargetAngle - currentAngle;

                var d = CurrentLocomotorTemplate.TurnRate * deltaTime * 0.1f;
                var newAngle = currentAngle + (angleDelta * d);
                //var newAngle = currentAngle + d;

                if (Math.Abs(angleDelta) > 0.1f)
                {
                    var pitch = 0.0f;
                    if (Definition.KindOf.Get(ObjectKinds.Vehicle))
                    {
                        var normal = heightMap?.GetNormal(x, y) ?? new Vector3();
                        pitch = (float) Math.Atan2(Vector3.UnitZ.Y - normal.Y, Vector3.UnitZ.X - normal.X);
                    }
                    Transform.Rotation = Quaternion.CreateFromYawPitchRoll(pitch, 0.0f, newAngle);
                }

                var direction = Vector3.Normalize(delta);
                trans += direction * distance;
                trans.Z = heightMap.GetHeight(x, y);
                Transform.Translation = trans;

                if (Vector3.Distance(Transform.Translation, TargetPoint.Value) < 0.5f)
                {
                    TargetPoint = null;
                    ClearModelConditionFlags();
                }
            }

            // Check if the unit is being constructed
            if (
                ModelConditionFlags.Get(ModelConditionFlag.ActivelyBeingConstructed) ||
                ModelConditionFlags.Get(ModelConditionFlag.AwaitingConstruction) ||
                ModelConditionFlags.Get(ModelConditionFlag.PartiallyConstructed)
            )
            {
                if (gameTime.TotalTime > (ConstructionStart + TimeSpan.FromSeconds(Definition.BuildTime)))
                {
                    ClearModelConditionFlags();
                }
            }
            // Update all draw modules
            foreach (var drawModule in DrawModules)
            {
                drawModule.Update(gameTime);
            }

            // TODO: Make sure we've processed everything that might update
            // this object's transform before updating draw modules' position.
            var worldMatrix = Transform.Matrix;
            foreach (var drawModule in DrawModules)
            {
                drawModule.SetWorldMatrix(worldMatrix);
            }
        }

        internal void BuildRenderList(RenderList renderList, Camera camera)
        {
            if (Destroyed)
            {
                return;
            }

            if (ModelConditionFlags.Get(ModelConditionFlag.Sold))
            {
                return;
            }

            var castsShadow = false;
            switch (Definition.Shadow)
            {
                case ObjectShadowType.ShadowVolume:
                case ObjectShadowType.ShadowVolumeNew:
                    castsShadow = true;
                    break;
            }

            foreach (var drawModule in DrawModules)
            {
                drawModule.BuildRenderList(
                    renderList,
                    camera,
                    castsShadow,
                    Owner);
            }
        }

        public void ClearModelConditionFlags()
        {
            ModelConditionFlags.SetAll(false);
            UpdateDrawModuleConditionStates();
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
            var locoDefs = Definition.Locomotors;
            if (locoDefs.Count > 0)
            {
                var locomotor = Definition.Locomotors.First().Value[0].Value;
                if (locomotor != null)
                {
                    CurrentLocomotorTemplate = locomotor;
                }
            }
        }
    }
}
