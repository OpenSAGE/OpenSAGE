using System;
using System.Numerics;
using LLGfx;
using LLGfx.Util;
using Buffer = LLGfx.Buffer;

namespace OpenSage.Graphics.Effects
{
    internal sealed class EffectParameter : GraphicsObject
    {
        private readonly ShaderResourceBinding _resourceBinding;

        private readonly ConstantBuffer _constantBuffer;

        private object _data;
        private bool _isDirty;

        public string Name => _resourceBinding.Name;

        public bool IsConstantBuffer => _resourceBinding.ResourceType == ShaderResourceType.ConstantBuffer;

        public EffectParameter(GraphicsDevice graphicsDevice, ShaderResourceBinding resourceBinding)
        {
            _resourceBinding = resourceBinding;

            if (resourceBinding.ResourceType == ShaderResourceType.ConstantBuffer)
            {
                _constantBuffer = AddDisposable(new ConstantBuffer(
                    graphicsDevice, 
                    (uint) resourceBinding.ConstantBufferSizeInBytes,
                    resourceBinding.ConstantBufferFields));
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

        public void SetConstantBufferField(string fieldName, byte[] bytes)
        {
            _constantBuffer.SetData(fieldName, bytes);

            _isDirty = true;
        }

        public void SetConstantBufferField<T>(string fieldName, ref T value)
            where T : struct
        {
            var bytes = StructInteropUtility.ToBytes(ref value);

            SetConstantBufferField(fieldName, bytes);
        }

        public void ApplyChanges(CommandEncoder commandEncoder)
        {
            if (!_isDirty)
            {
                return;
            }

            switch (_resourceBinding.ResourceType)
            {
                case ShaderResourceType.ConstantBuffer:
                    _constantBuffer.ApplyChanges(commandEncoder);
                    switch (_resourceBinding.ShaderType)
                    {
                        case ShaderType.VertexShader:
                            commandEncoder.SetVertexShaderConstantBuffer(_resourceBinding.Slot, _constantBuffer.Buffer);
                            break;

                        case ShaderType.PixelShader:
                            commandEncoder.SetPixelShaderConstantBuffer(_resourceBinding.Slot, _constantBuffer.Buffer);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;

                case ShaderResourceType.StructuredBuffer:
                    switch (_resourceBinding.ShaderType)
                    {
                        case ShaderType.VertexShader:
                            commandEncoder.SetVertexShaderStructuredBuffer(_resourceBinding.Slot, (Buffer) _data);
                            break;

                        case ShaderType.PixelShader:
                            commandEncoder.SetPixelShaderStructuredBuffer(_resourceBinding.Slot, (Buffer) _data);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;

                case ShaderResourceType.Texture:
                    switch (_resourceBinding.ShaderType)
                    {
                        case ShaderType.VertexShader:
                            commandEncoder.SetVertexShaderTexture(_resourceBinding.Slot, (Texture) _data);
                            break;

                        case ShaderType.PixelShader:
                            commandEncoder.SetPixelShaderTexture(_resourceBinding.Slot, (Texture) _data);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;

                case ShaderResourceType.Sampler:
                    switch (_resourceBinding.ShaderType)
                    {
                        case ShaderType.VertexShader:
                            commandEncoder.SetVertexShaderSampler(_resourceBinding.Slot, (SamplerState) _data);
                            break;

                        case ShaderType.PixelShader:
                            commandEncoder.SetPixelShaderSampler(_resourceBinding.Slot, (SamplerState) _data);
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
