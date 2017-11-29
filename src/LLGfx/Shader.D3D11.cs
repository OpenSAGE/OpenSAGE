using System;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace LLGfx
{
    partial class Shader
    {
        internal byte[] DeviceBytecode { get; set; }
        internal DeviceChild DeviceShader { get; private set; }

        private void PlatformConstruct(
            ShaderLibrary shaderLibrary, 
            string shaderName, 
            out ShaderType shaderType,
            out ShaderResourceBinding[] resourceBindings)
        {
            DeviceBytecode = shaderLibrary.GetShader(shaderName);

            using (var shaderBytecode = new ShaderBytecode(DeviceBytecode))
            {
                switch (shaderBytecode.GetVersion().Version)
                {
                    case ShaderVersion.VertexShader:
                        DeviceShader = AddDisposable(new VertexShader(shaderLibrary.GraphicsDevice.Device, DeviceBytecode));
                        shaderType = ShaderType.VertexShader;
                        break;

                    case ShaderVersion.PixelShader:
                        DeviceShader = AddDisposable(new PixelShader(shaderLibrary.GraphicsDevice.Device, DeviceBytecode));
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

                    resourceBindings[i] = new ShaderResourceBinding(
                        resourceDescription.Name,
                        GetResourceType(resourceDescription.Type),
                        shaderType,
                        resourceDescription.BindPoint);
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
