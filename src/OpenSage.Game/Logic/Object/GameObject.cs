using System.Collections.Generic;
using System.Linq;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;

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

        public Team Owner { get; set; }

        public bool IsSelectable { get; set; }

        public GameObject(ObjectDefinition objectDefinition, ContentManager contentManager)
        {
            Definition = objectDefinition;

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

        internal void Update(GameTime gameTime)
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
                drawModule.BuildRenderList(renderList, camera, castsShadow);
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
    }
}
