﻿using System;
using System.Collections.Generic;
using OpenSage.Content;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public class EffectMaterial : DisposableBase
    {
        private static ushort _nextID = 0;

        private readonly ContentManager _contentManager;
        private readonly Dictionary<string, EffectMaterialProperty> _properties;

        public Effect Effect { get; }

        public EffectPipelineState PipelineState { get; set; }

        public ushort ID { get; }

        public virtual LightingType LightingType => LightingType.None;

        public EffectMaterial(ContentManager contentManager, Effect effect)
        {
            // TODO: This can overflow.
            ID = _nextID++;

            _contentManager = contentManager;

            Effect = effect;

            _properties = new Dictionary<string, EffectMaterialProperty>();
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
            if (resource == null)
            {
                switch (property.Parameter.ResourceBinding.Kind)
                {
                    case ResourceKind.StructuredBufferReadOnly:
                        resource = _contentManager.GetNullStructuredBuffer(property.Parameter.ResourceBinding.Type.Size);
                        break;

                    case ResourceKind.TextureReadOnly:
                    case ResourceKind.TextureReadWrite:
                        // TODO: This only supports Texture2D shader parameters.
                        resource = _contentManager.NullTexture;
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
            property.SetData(resource);
        }

        public void ApplyPipelineState()
        {
            Effect.SetPipelineState(PipelineState.Handle);
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
                throw new ArgumentNullException(nameof(resource));
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
