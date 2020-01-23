using System;
using System.Collections.Generic;
using OpenSage.Utilities;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ShaderMaterialResourceSetBuilder : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly ShaderMaterialShaderResources _shaderResources;
        private readonly Dictionary<string, ResourceBinding> _resourceBindings;
        private readonly ResourceType _resourceType;
        private readonly byte[] _constantsBytes;
        private readonly DeviceBuffer _constantsBuffer;
        private readonly BindableResource[] _resources;

        public ShaderMaterialResourceSetBuilder(
            GraphicsDevice graphicsDevice,
            ShaderMaterialShaderResources shaderResources)
        {
            _graphicsDevice = graphicsDevice;
            _shaderResources = shaderResources;

            _resourceBindings = shaderResources.MaterialResourceBindings;

            _resourceType = _resourceBindings["MaterialConstants"].Type;

            _constantsBytes = new byte[_resourceType.Size];

            _constantsBuffer = AddDisposable(_graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(
                    _resourceType.Size,
                    BufferUsage.UniformBuffer | BufferUsage.Dynamic)));

            _resources = new BindableResource[_resourceBindings.Count];
            _resources[0] = _constantsBuffer;
        }

        public void SetTexture(string name, Texture value)
        {
            var resourceBinding = _resourceBindings[name];
            _resources[resourceBinding.Binding] = value;
        }

        public void SetConstant<T>(string name, T value)
            where T : struct
        {
            SetConstant(name, StructInteropUtility.ToBytes(ref value));
        }

        public void SetConstant(string name, byte[] valueBytes)
        {
            var constantBufferField = _resourceType.GetMember(name);

            if (constantBufferField == null)
            {
                // TODO
                return;
            }

            if (valueBytes.Length != constantBufferField.Size)
            {
                throw new InvalidOperationException();
            }

            Buffer.BlockCopy(
                valueBytes,
                0,
                _constantsBytes,
                (int) constantBufferField.Offset,
                (int) constantBufferField.Size);
        }

        public ResourceSet CreateResourceSet()
        {
            _graphicsDevice.UpdateBuffer(_constantsBuffer, 0, _constantsBytes);

            return AddDisposable(_shaderResources.CreateMaterialResourceSet(_resources));
        }
    }
}
