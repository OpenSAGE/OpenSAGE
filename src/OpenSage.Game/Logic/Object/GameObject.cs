using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Terrain;

namespace OpenSage.Logic.Object
{
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


        public GameObject(ObjectDefinition objectDefinition, ContentManager contentManager, Player owner)
        {
            Definition = objectDefinition;
            Context = contentManager.IniDataContext;
            Owner = owner;

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

        internal void LogicTick(ulong frame, HeightMap heightMap)
        {
            // TODO: Update modules.
            if (Definition.KindOf == null) return;

            if (Definition.KindOf.Get(ObjectKinds.Infantry)
                || Definition.KindOf.Get(ObjectKinds.Vehicle))
            {
                if (IsSelected)
                {
                    //todo get movement speed from current locomotor
                    var speed = CurrentLocomotor.Speed / 10.0f;

                    var direction = new Vector3(1.0f, 0.0f, 0);
                    Transform.Translation += direction * speed;

                    var x = Transform.Translation.X;
                    var y = Transform.Translation.Y;
                    var z = heightMap.GetHeight(x, y);

                    
                    Transform.Translation = new Vector3(x, y, z);

                    //if (Definition.KindOf.Get(ObjectKinds.Vehicle))
                    //{
                    //    var normal = heightMap.GetNormal(x, y);
                    //    Transform.Rotation = Quaternion.CreateFromYawPitchRoll(normal.X, normal.Y, normal.Z);
                    //}
                    
                }
            }
            
        }

        internal void LocalLogicTick(in TimeInterval gameTime, float tickT)
        {
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
