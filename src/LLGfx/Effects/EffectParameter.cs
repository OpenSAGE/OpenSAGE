using System;

namespace LLGfx.Effects
{
    internal sealed class ConstantBuffer : GraphicsObject
    {
        private readonly Buffer _buffer;

        public ConstantBuffer(int sizeInBytes)
        {

        }
    }

    internal sealed class EffectParameter : GraphicsObject
    {
        private readonly ShaderResourceBinding _resourceBinding;

        private readonly ConstantBuffer _constantBuffer;

        private object _data;
        private bool _isDirty;

        public string Name => _resourceBinding.Name;

        public EffectParameter(ShaderResourceBinding resourceBinding)
        {
            _resourceBinding = resourceBinding;

            if (resourceBinding.ResourceType == ShaderResourceType.ConstantBuffer)
            {
                //_constantBuffer = AddDisposable(new ConstantBuffer(resourceBinding.));
            }
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

            switch (_resourceBinding.ResourceType)
            {
                // TODO
                case ShaderResourceType.ConstantBuffer:
                    //    switch (ShaderStage)
                    //    {
                    //        case EffectResourceShaderStage.VertexShader:
                    //            commandEncoder.SetVertexConstantBuffer(Slot, (Buffer) _data);
                    //            break;

                    //        case EffectResourceShaderStage.PixelShader:
                    //            commandEncoder.SetFragmentConstantBuffer(Slot, (Buffer) _data);
                    //            break;

                    //        default:
                    //            throw new InvalidOperationException();
                    //    }
                    break;

                case ShaderResourceType.StructuredBuffer:
                    switch (_resourceBinding.ShaderType)
                    {
                        case ShaderType.VertexShader:
                            commandEncoder.SetVertexStructuredBuffer(_resourceBinding.Slot, (Buffer) _data);
                            break;

                        case ShaderType.PixelShader:
                            commandEncoder.SetFragmentStructuredBuffer(_resourceBinding.Slot, (Buffer) _data);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;

                case ShaderResourceType.Texture:
                    switch (_resourceBinding.ShaderType)
                    {
                        case ShaderType.VertexShader:
                            commandEncoder.SetVertexTexture(_resourceBinding.Slot, (Texture) _data);
                            break;

                        case ShaderType.PixelShader:
                            commandEncoder.SetFragmentTexture(_resourceBinding.Slot, (Texture) _data);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;

                case ShaderResourceType.Sampler:
                    switch (_resourceBinding.ShaderType)
                    {
                        case ShaderType.VertexShader:
                            commandEncoder.SetVertexSampler(_resourceBinding.Slot, (SamplerState) _data);
                            break;

                        case ShaderType.PixelShader:
                            commandEncoder.SetFragmentSampler(_resourceBinding.Slot, (SamplerState) _data);
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
