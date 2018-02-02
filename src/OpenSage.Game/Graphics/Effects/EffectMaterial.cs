using System.Collections.Generic;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public class EffectMaterial : DisposableBase
    {
        private static ushort _nextID = 0;

        private readonly Dictionary<uint, EffectMaterialProperty> _properties;

        // TODO_VELDRID: Remove this.
        private readonly Dictionary<Texture, TextureView> _cachedTextureViews;

        public Effect Effect { get; }

        public EffectPipelineState PipelineState { get; set; }

        public ushort ID { get; }

        public virtual uint? SlotRenderItemConstantsVS => null;
        public virtual uint? SlotGlobalConstantsShared => null;
        public virtual uint? SlotGlobalConstantsVS => null;
        public virtual uint? SlotGlobalConstantsPS => null;
        public virtual uint? SlotLightingConstants_Object => null;
        public virtual uint? SlotLightingConstants_Terrain => null;

        public EffectMaterial(Effect effect)
        {
            // TODO: This can overflow.
            ID = _nextID++;

            Effect = effect;

            _properties = new Dictionary<uint, EffectMaterialProperty>();

            _cachedTextureViews = new Dictionary<Texture, TextureView>();
        }

        private EffectMaterialProperty EnsureProperty(uint slot)
        {
            if (!_properties.TryGetValue(slot, out var property))
            {
                var parameter = Effect.GetParameter(slot);
                _properties[slot] = property = AddDisposable(new EffectMaterialProperty(Effect.GraphicsDevice, parameter));
            }
            return property;
        }

        public void SetProperty(uint slot, BindableResource resource)
        {
            var property = EnsureProperty(slot);
            property.SetData(resource);
        }

        public void SetProperty(uint slot, Texture texture)
        {
            if (texture == null)
            {
                SetProperty(slot, (BindableResource) null);
                return;
            }

            if (!_cachedTextureViews.TryGetValue(texture, out var view))
            {
                _cachedTextureViews.Add(texture, view = AddDisposable(Effect.GraphicsDevice.ResourceFactory.CreateTextureView(texture)));
            }
            SetProperty(slot, view);
        }

        public void Apply(in OutputDescription outputDescription)
        {
            Effect.SetPipelineState(PipelineState.GetHandle(outputDescription));

            foreach (var property in _properties.Values)
            {
                property.Parameter.SetData(property.Data);
            }
        }
    }

    internal sealed class EffectMaterialProperty : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;

        // TODO_VELDRID: Remove this. It's only temporary, until we switch properly to ResourceSets.
        private readonly Dictionary<BindableResource, ResourceSet> _cachedResourceSets;

        public EffectParameter Parameter { get; }

        public ResourceSet Data { get; private set; }

        public EffectMaterialProperty(GraphicsDevice graphicsDevice, EffectParameter parameter)
        {
            _graphicsDevice = graphicsDevice;

            Parameter = parameter;

            _cachedResourceSets = new Dictionary<BindableResource, ResourceSet>();
        }

        public void SetData(BindableResource resource)
        {
            if (resource == null)
            {
                Data = null;
                return;
            }

            if (!_cachedResourceSets.TryGetValue(resource, out var result))
            {
                _cachedResourceSets.Add(resource, result = AddDisposable(_graphicsDevice.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(Parameter.ResourceLayout, resource))));
            }

            Data = result;
        }
    }
}
