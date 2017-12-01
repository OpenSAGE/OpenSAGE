using System.Collections.Generic;
using LLGfx;
using LLGfx.Util;

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

        public void SetProperty<T>(string constantBufferName, string fieldName, ref T value)
            where T : struct
        {
            var property = EnsureProperty(constantBufferName);

            var bytes = StructInteropUtility.ToBytes(ref value);
            property.ConstantBufferFields[fieldName] = bytes;
        }

        public void SetProperty<T>(string constantBufferName, string fieldName, T value)
            where T : struct
        {
            SetProperty(constantBufferName, fieldName, ref value);
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
        public Dictionary<string, byte[]> ConstantBufferFields = new Dictionary<string, byte[]>();

        public EffectMaterialProperty(EffectParameter parameter)
        {
            Parameter = parameter;
        }
    }
}
