using System;
using OpenSage.LowLevel.Graphics3D;
using Buffer = OpenSage.LowLevel.Graphics3D.Buffer;

namespace OpenSage.Graphics.Effects
{
    internal sealed class EffectParameter : GraphicsObject
    {
        private object _data;
        private bool _isDirty;

        public ShaderResourceBinding ResourceBinding { get; }

        public string Name => ResourceBinding.Name;

        public EffectParameter(GraphicsDevice graphicsDevice, ShaderResourceBinding resourceBinding)
        {
            ResourceBinding = resourceBinding;
        }

        public void ResetDirty()
        {
            _isDirty = true;
        }

        public void SetData(object data)
        {
            if (ReferenceEquals(_data, data))
            {
                return;
            }

            _data = data;
            _isDirty = true;
        }

        public void ApplyChanges(CommandEncoder commandEncoder)
        {
            if (!_isDirty)
            {
                return;
            }

            switch (ResourceBinding.ResourceType)
            {
                case ShaderResourceType.ConstantBuffer:
                    switch (ResourceBinding.ShaderType)
                    {
                        case ShaderType.VertexShader:
                            commandEncoder.SetVertexShaderConstantBuffer(ResourceBinding.Slot, (Buffer) _data);
                            break;

                        case ShaderType.PixelShader:
                            commandEncoder.SetPixelShaderConstantBuffer(ResourceBinding.Slot, (Buffer) _data);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;

                case ShaderResourceType.StructuredBuffer:
                    switch (ResourceBinding.ShaderType)
                    {
                        case ShaderType.VertexShader:
                            commandEncoder.SetVertexShaderStructuredBuffer(ResourceBinding.Slot, (Buffer) _data);
                            break;

                        case ShaderType.PixelShader:
                            commandEncoder.SetPixelShaderStructuredBuffer(ResourceBinding.Slot, (Buffer) _data);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;

                case ShaderResourceType.Texture:
                    switch (ResourceBinding.ShaderType)
                    {
                        case ShaderType.VertexShader:
                            commandEncoder.SetVertexShaderTexture(ResourceBinding.Slot, (Texture) _data);
                            break;

                        case ShaderType.PixelShader:
                            commandEncoder.SetPixelShaderTexture(ResourceBinding.Slot, (Texture) _data);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;

                case ShaderResourceType.Sampler:
                    switch (ResourceBinding.ShaderType)
                    {
                        case ShaderType.VertexShader:
                            commandEncoder.SetVertexShaderSampler(ResourceBinding.Slot, (SamplerState) _data);
                            break;

                        case ShaderType.PixelShader:
                            commandEncoder.SetPixelShaderSampler(ResourceBinding.Slot, (SamplerState) _data);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;

                default:
                    throw new InvalidOperationException();
            }

            _isDirty = false;
        }
    }
}
