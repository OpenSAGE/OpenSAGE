using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic.Object.Production;
using OpenSage.Terrain;

namespace OpenSage.Logic.Object
{
    [DebuggerDisplay("[Object:{Definition.Name} ({Owner})]")]
    public sealed class GameObject : DisposableBase
    {
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

        private Locomotor CurrentLocomotor { get; set; }

        private IniDataContext Context { get; set; }

        private Vector3? TargetPoint { get; set; }
        private float TargetAngle { get; set; }

        private TimeSpan ConstructionStart { get; set; }

        public bool Destroyed { get; set; }

        public GameObjectCollection Parent { get; private set; }

        public GameObject(ObjectDefinition objectDefinition, ContentManager contentManager, Player owner, GameObjectCollection parent)
        {
            Definition = objectDefinition;
            Context = contentManager.IniDataContext;
            Owner = owner;
            Parent = parent;

            SetLocomotor();
            Transform = Transform.CreateIdentity();

            var drawModules = new List<DrawModule>();
            foreach (var drawData in objectDefinition.Draws)
            {
                var drawModule = AddDisposable(drawData.CreateDrawModule(contentManager));
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
                .Distinct(new BitArrayEqualityComparer<ModelConditionFlag>())
                .OrderBy(x => x.NumBitsSet)
                .ToList();

            SetModelConditionFlags(new BitArray<ModelConditionFlag>());

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

            var flags = new BitArray<ModelConditionFlag>();
            flags.Set(ModelConditionFlag.Moving, true);
            SetModelConditionFlags(flags);
        }

        internal void StartConstruction(in TimeInterval gameTime)
        {
            if (Definition.KindOf == null) return;

            if (Definition.KindOf.Get(ObjectKinds.Structure))
            {
                var flags = new BitArray<ModelConditionFlag>();
                flags.Set(ModelConditionFlag.ActivelyBeingConstructed, true);
                flags.Set(ModelConditionFlag.AwaitingConstruction, true);
                flags.Set(ModelConditionFlag.PartiallyConstructed, true);
                SetModelConditionFlags(flags);
                ConstructionStart = gameTime.TotalTime;

                // this is strang in startcontruction, however, we have no better place to put it yet. belongs in finishconstruction
                foreach (var behavior in Definition.Behaviors)
                {
                    if (behavior is SpawnBehaviorModuleData)
                    {
                        var spawnTemplate = ((SpawnBehaviorModuleData) behavior).SpawnTemplateName;
                        var unitDefinition = Context.Objects.Find(x => x.Name == spawnTemplate);
                        Spawn(unitDefinition);
                    }
                }
            }
        }

        internal void LocalLogicTick(in TimeInterval gameTime, float tickT, HeightMap heightMap)
        {
            var flags = new BitArray<ModelConditionFlag>();
            var deltaTime = gameTime.DeltaTime.Milliseconds / 1000.0f;

            // Check if the unit is currently moving
            flags.Set(ModelConditionFlag.Moving, true);
            if (ModelConditionFlags.And(flags).AnyBitSet && TargetPoint.HasValue)
            {
                var x = Transform.Translation.X;
                var y = Transform.Translation.Y;
                var trans = Transform.Translation;

                // This locomotor speed is distance/second
                var delta = TargetPoint.Value - Transform.Translation;
                var distance = CurrentLocomotor.Speed * deltaTime;
                if (delta.Length() < distance) distance = delta.Length();

                var currentAngle = -Transform.EulerAngles.Z;
                var angleDelta = TargetAngle - currentAngle;

                var d = CurrentLocomotor.TurnRate * deltaTime * 0.1f;
                var newAngle = currentAngle + (angleDelta * d);
                //var newAngle = currentAngle + d;

                if (Math.Abs(angleDelta) > 0.1f)
                {
                    var pitch = 0.0f;
                    if (Definition.KindOf.Get(ObjectKinds.Vehicle))
                    {
                        var normal = heightMap.GetNormal(x, y);
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
                    SetModelConditionFlags(new BitArray<ModelConditionFlag>());
                }
            }

            // Check if the unit is being constructed
            flags.SetAll(false);
            flags.Set(ModelConditionFlag.ActivelyBeingConstructed, true);
            flags.Set(ModelConditionFlag.AwaitingConstruction, true);
            flags.Set(ModelConditionFlag.PartiallyConstructed, true);
            if (ModelConditionFlags.And(flags).AnyBitSet)
            {
                if (gameTime.TotalTime > (ConstructionStart + TimeSpan.FromSeconds(Definition.BuildTime)))
                {
                    SetModelConditionFlags(new BitArray<ModelConditionFlag>());
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

        public void SetModelConditionFlag(ModelConditionFlag flag, bool value)
        {
            ModelConditionFlags.Set(flag, value);
        }

        public void SetModelConditionFlags(BitArray<ModelConditionFlag> flags)
        {
            ModelConditionFlags = flags;

            // TODO: Let each drawable use the appropriate TransitionState between ConditionStates.

            foreach (var drawModule in DrawModules)
            {
                drawModule.UpdateConditionState(flags);
            }
        }

        public void OnLocalSelect(AudioSystem gameAudio)
        {
            if (Definition.VoiceSelect != null)
            {
                gameAudio.PlayAudioEvent(Definition.VoiceSelect);
            }
        }

        public void OnLocalMove(AudioSystem gameAudio)
        {
            if (Definition.VoiceMove != null)
            {
                gameAudio.PlayAudioEvent(Definition.VoiceMove);
            }
        }

        private void SetLocomotor()
        {
            var locoDefs = Definition.Locomotors;
            if (locoDefs.Count > 0)
            {
                var name = locoDefs.First().Value[0];
                if (Context.Locomotors.ContainsKey(name))
                {
                    CurrentLocomotor = Context.Locomotors[name];
                }
            }
        }

    }
}
