using System.Collections.Generic;
using LL.Graphics3D;
using LL.Graphics3D.Util;

namespace OpenSage.Graphics.Effects
{
    public class EffectMaterial
    {
        private readonly Dictionary<string, EffectMaterialProperty> _properties;

        public Effect Effect { get; }

        public EffectMaterial(Effect effect)
        {
            Effect = effect;

            _properties = new Dictionary<string, EffectMaterialProperty>();
        }

        private EffectMaterialProperty EnsureProperty(string name)
        {
            if (!_properties.TryGetValue(name, out var property))
            {
                var parameter = Effect.GetParameter(name);
                _properties[name] = property = new EffectMaterialProperty(parameter);
            }
            return property;
        }

        private void SetPropertyImpl(string name, object value)
        {
            var property = EnsureProperty(name);

            property.Data = value;
        }

        public void SetProperty(string name, Buffer buffer)
        {
            SetPropertyImpl(name, buffer);
        }

        public void SetProperty(string name, Texture texture)
        {
            SetPropertyImpl(name, texture);
        }

        public void SetProperty(string name, SamplerState sampler)
        {
            SetPropertyImpl(name, sampler);
        }

        public void Apply()
        {
            foreach (var property in _properties.Values)
            {
                property.Parameter.SetData(property.Data);
            }
        }
    }

    internal sealed class EffectMaterialProperty
    {
        public EffectParameter Parameter { get; }

        public object Data;

        public EffectMaterialProperty(EffectParameter parameter)
        {
            Parameter = parameter;
        }
    }
}
