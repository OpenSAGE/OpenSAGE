using System;
using OpenSage.Graphics.Shaders;
using OpenSage.Utilities;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    internal sealed class ShaderMaterialConstantsBuilder
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly ResourceType _resourceBinding;
        private readonly byte[] _constantsBytes;

        public ShaderMaterialConstantsBuilder(Effect effect)
        {
            _graphicsDevice = effect.GraphicsDevice;

            _resourceBinding = effect.GetParameter("MaterialConstants").ResourceBinding.Type;

            _constantsBytes = new byte[_resourceBinding.Size];
        }

        public void SetConstant<T>(string name, T value)
            where T : struct
        {
            SetConstant(name, StructInteropUtility.ToBytes(ref value));
        }

        public void SetConstant(string name, byte[] valueBytes)
        {
            var constantBufferField = _resourceBinding.GetMember(name);

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

        public DeviceBuffer CreateBuffer()
        {
            var constantsBuffer = _graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(
                    _resourceBinding.Size,
                    BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            _graphicsDevice.UpdateBuffer(constantsBuffer, 0, _constantsBytes);

            return constantsBuffer;
        }
    }
}
