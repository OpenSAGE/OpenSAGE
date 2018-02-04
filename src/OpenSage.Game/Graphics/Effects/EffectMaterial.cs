using System.Collections.Generic;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public class EffectMaterial : DisposableBase
    {
        private static ushort _nextID = 0;

        private readonly Dictionary<string, EffectMaterialProperty> _properties;

        // TODO_VELDRID: Remove this.
        private readonly Dictionary<Texture, TextureView> _cachedTextureViews;

        public Effect Effect { get; }

        public EffectPipelineState PipelineState { get; set; }

        public ushort ID { get; }

        public EffectMaterial(Effect effect)
        {
            // TODO: This can overflow.
            ID = _nextID++;

            Effect = effect;

            _properties = new Dictionary<string, EffectMaterialProperty>();

            _cachedTextureViews = new Dictionary<Texture, TextureView>();
        }

        private EffectMaterialProperty EnsureProperty(string name)
        {
            if (!_properties.TryGetValue(name, out var property))
            {
                var parameter = Effect.GetParameter(name);
                _properties[name] = property = AddDisposable(new EffectMaterialProperty(Effect.GraphicsDevice, parameter));
            }
            return property;
        }

        public void SetProperty(string name, BindableResource resource)
        {
            var property = EnsureProperty(name);
            property.SetData(resource);
        }

        public void SetProperty(string name, Texture texture)
        {
            if (texture == null)
            {
                SetProperty(name, (BindableResource) null);
                return;
            }

            if (!_cachedTextureViews.TryGetValue(texture, out var view))
            {
                _cachedTextureViews.Add(texture, view = AddDisposable(Effect.GraphicsDevice.ResourceFactory.CreateTextureView(texture)));
            }
            SetProperty(name, view);
        }

        public void ApplyPipelineState(in OutputDescription outputDescription)
        {
            Effect.SetPipelineState(PipelineState.GetHandle(outputDescription));
        }

        public void ApplyProperties()
        {
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
        private ResourceSet _nullResourceSet;

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
                if (_nullResourceSet == null)
                {
                    _nullResourceSet = AddDisposable(_graphicsDevice.ResourceFactory.CreateResourceSet(
                        new ResourceSetDescription(Parameter.ResourceLayout, new BindableResource[] { null })));
                }
                Data = _nullResourceSet;
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
