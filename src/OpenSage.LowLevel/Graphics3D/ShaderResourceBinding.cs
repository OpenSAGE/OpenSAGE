using System;
using System.Collections.Generic;
using System.Linq;

namespace LL.Graphics3D
{
    public sealed class ShaderResourceBinding
    {
        private readonly Dictionary<string, ConstantBufferField> _constantBufferFieldsDictionary;

        public string Name { get; }
        public ShaderResourceType ResourceType { get; }
        public ShaderType ShaderType { get; }
        public int Slot { get; }

        public int ConstantBufferSizeInBytes { get; }
        public ConstantBufferField[] ConstantBufferFields { get; }

        public ShaderResourceBinding(
            string name, 
            ShaderResourceType resourceType, 
            ShaderType shaderStage, 
            int slot,
            int constantBufferSizeInBytes,
            ConstantBufferField[] constantBufferFields)
        {
            Name = name;
            ResourceType = resourceType;
            ShaderType = shaderStage;
            Slot = slot;

            ConstantBufferSizeInBytes = constantBufferSizeInBytes;
            ConstantBufferFields = constantBufferFields;

            _constantBufferFieldsDictionary = constantBufferFields?.ToDictionary(x => x.Name);
        }

        public ConstantBufferField GetConstantBufferField(string fieldName)
        {
            if (ResourceType != ShaderResourceType.ConstantBuffer)
            {
                throw new InvalidOperationException();
            }

            if (!_constantBufferFieldsDictionary.TryGetValue(fieldName, out var field))
            {
                throw new InvalidOperationException($"Missing field {fieldName} in constant buffer {Name}");
            }

            return field;
        }
    }
}
