using System;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace LL.Graphics3D
{
    partial class Shader
    {
        internal byte[] DeviceBytecode { get; set; }
        internal DeviceChild DeviceShader { get; private set; }

        internal override string PlatformGetDebugName() => DeviceShader.DebugName;
        internal override void PlatformSetDebugName(string value) => DeviceShader.DebugName = value;

        private void PlatformConstruct(
            GraphicsDevice graphicsDevice,
            byte[] deviceBytecode, 
            out ShaderType shaderType,
            out ShaderResourceBinding[] resourceBindings)
        {
            DeviceBytecode = deviceBytecode;

            using (var shaderBytecode = new ShaderBytecode(DeviceBytecode))
            {
                switch (shaderBytecode.GetVersion().Version)
                {
                    case ShaderVersion.VertexShader:
                        DeviceShader = AddDisposable(new VertexShader(graphicsDevice.Device, DeviceBytecode));
                        shaderType = ShaderType.VertexShader;
                        break;

                    case ShaderVersion.PixelShader:
                        DeviceShader = AddDisposable(new PixelShader(graphicsDevice.Device, DeviceBytecode));
                        shaderType = ShaderType.PixelShader;
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            using (var reflection = new ShaderReflection(DeviceBytecode))
            {
                resourceBindings = new ShaderResourceBinding[reflection.Description.BoundResources];

                for (var i = 0; i < resourceBindings.Length; i++)
                {
                    var resourceDescription = reflection.GetResourceBindingDescription(i);

                    int constantBufferSizeInBytes;
                    ConstantBufferField[] constantBufferFields;
                    if (resourceDescription.Type == ShaderInputType.ConstantBuffer)
                    {
                        using (var constantBufferDesc = reflection.GetConstantBuffer(resourceDescription.Name))
                        {
                            constantBufferSizeInBytes = constantBufferDesc.Description.Size;
                            constantBufferFields = new ConstantBufferField[constantBufferDesc.Description.VariableCount];
                            for (var j = 0; j < constantBufferFields.Length; j++)
                            {
                                var variable = constantBufferDesc.GetVariable(j);

                                constantBufferFields[j] = new ConstantBufferField(
                                    variable.Description.Name,
                                    variable.Description.StartOffset,
                                    variable.Description.Size);
                            }
                        }
                    }
                    else
                    {
                        constantBufferSizeInBytes = 0;
                        constantBufferFields = null;
                    }

                    resourceBindings[i] = new ShaderResourceBinding(
                        resourceDescription.Name,
                        GetResourceType(resourceDescription.Type),
                        shaderType,
                        resourceDescription.BindPoint,
                        constantBufferSizeInBytes,
                        constantBufferFields);
                }
            }
        }

        private static ShaderResourceType GetResourceType(ShaderInputType type)
        {
            switch (type)
            {
                case ShaderInputType.ConstantBuffer:
                    return ShaderResourceType.ConstantBuffer;

                case ShaderInputType.Texture:
                    return ShaderResourceType.Texture;

                case ShaderInputType.Sampler:
                    return ShaderResourceType.Sampler;

                case ShaderInputType.Structured:
                    return ShaderResourceType.StructuredBuffer;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
