using System;
using System.Collections.Generic;
using System.Linq;

namespace LLGfx.Effects
{
    public abstract partial class Effect : GraphicsObject
    {
        private readonly GraphicsDevice _graphicsDevice;

        private VertexDescriptor _vertexDescriptor;
        private readonly VertexShader _vertexShader;
        private readonly PixelShader _pixelShader;

        private readonly Dictionary<EffectPipelineStateHandle, PipelineState> _cachedPipelineStates;

        private readonly Dictionary<string, EffectResourceBinding> _resourceBindings;

        private PipelineState _pipelineState;

        private EffectDirtyFlags _dirtyFlags;

        protected GraphicsDevice GraphicsDevice => _graphicsDevice;

        [Flags]
        private enum EffectDirtyFlags
        {
            None = 0,

            PipelineState = 0x1
        }

        protected Effect(
            GraphicsDevice graphicsDevice,
            string vertexShaderName,
            string pixelShaderName,
            VertexDescriptor vertexDescriptor)
        {
            _graphicsDevice = graphicsDevice;

            _vertexShader = AddDisposable(new VertexShader(graphicsDevice.ShaderLibrary, vertexShaderName));
            _pixelShader = AddDisposable(new PixelShader(graphicsDevice.ShaderLibrary, pixelShaderName));

            _cachedPipelineStates = new Dictionary<EffectPipelineStateHandle, PipelineState>();

            _vertexDescriptor = vertexDescriptor;

            PlatformDoReflection(
                _vertexShader,
                _pixelShader,
                out var vertexShaderResourceBindings,
                out var pixelShaderResourceBindings);

            _resourceBindings = vertexShaderResourceBindings
                .Concat(pixelShaderResourceBindings)
                .ToDictionary(x => x.Name);
        }

        public void Begin(CommandEncoder commandEncoder)
        {
            _dirtyFlags |= EffectDirtyFlags.PipelineState;

            foreach (var resourceBinding in _resourceBindings.Values)
            {
                resourceBinding.ResetDirty();
            }

            OnBegin(commandEncoder);
        }

        protected abstract void OnBegin(CommandEncoder commandEncoder);

        public void Apply(CommandEncoder commandEncoder)
        {
            if (_dirtyFlags.HasFlag(EffectDirtyFlags.PipelineState))
            {
                commandEncoder.SetPipelineState(_pipelineState);

                _dirtyFlags &= ~EffectDirtyFlags.PipelineState;
            }

            foreach (var resourceBinding in _resourceBindings.Values)
            {
                resourceBinding.ApplyChanges(commandEncoder);
            }

            OnApply(commandEncoder);
        }

        protected abstract void OnApply(CommandEncoder commandEncoder);

        public void SetPipelineState(EffectPipelineStateHandle pipelineStateHandle)
        {
            _pipelineState = GetPipelineState(pipelineStateHandle);
            _dirtyFlags |= EffectDirtyFlags.PipelineState;
        }

        private PipelineState GetPipelineState(EffectPipelineStateHandle pipelineStateHandle)
        {
            if (!_cachedPipelineStates.TryGetValue(pipelineStateHandle, out var result))
            {
                var description = PipelineStateDescription.Default;

                description.RenderTargetFormat = _graphicsDevice.BackBufferFormat;
                description.VertexDescriptor = _vertexDescriptor;
                description.VertexShader = _vertexShader;
                description.PixelShader = _pixelShader;

                description.RasterizerState = pipelineStateHandle.EffectPipelineState.RasterizerState;
                description.DepthStencilState = pipelineStateHandle.EffectPipelineState.DepthStencilState;
                description.BlendState = pipelineStateHandle.EffectPipelineState.BlendState;

                _cachedPipelineStates[pipelineStateHandle] = result = AddDisposable(new PipelineState(_graphicsDevice, description));
            }

            return result;
        }

        private void SetValueImpl(string name, object value)
        {
            if (!_resourceBindings.TryGetValue(name, out var resourceBinding))
            {
                throw new InvalidOperationException();
            }

            resourceBinding.SetData(value);
        }

        public void SetValue(string name, Buffer buffer)
        {
            SetValueImpl(name, buffer);
        }

        public void SetValue(string name, Texture texture)
        {
            SetValueImpl(name, texture);
        }

        public void SetValue(string name, SamplerState sampler)
        {
            SetValueImpl(name, sampler);
        }
    }

    internal enum EffectResourceShaderStage
    {
        VertexShader,
        PixelShader
    }

    internal enum EffectResourceType
    {
        ConstantBuffer,
        StructuredBuffer,
        Texture,
        Sampler
    }

    internal sealed partial class EffectResourceBinding
    {
        public string Name { get; }
        public EffectResourceType ResourceType { get; }
        public EffectResourceShaderStage ShaderStage { get; }
        public int Slot { get; }

        private object _data;
        private bool _isDirty;

        public EffectResourceBinding(string name, EffectResourceType resourceType, EffectResourceShaderStage shaderStage, int slot)
        {
            Name = name;
            ResourceType = resourceType;
            ShaderStage = shaderStage;
            Slot = slot;

            ResetDirty();
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

            switch (ResourceType)
            {
                // TODO
                case EffectResourceType.ConstantBuffer:
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

                case EffectResourceType.StructuredBuffer:
                    switch (ShaderStage)
                    {
                        case EffectResourceShaderStage.VertexShader:
                            commandEncoder.SetVertexStructuredBuffer(Slot, (Buffer) _data);
                            break;

                        case EffectResourceShaderStage.PixelShader:
                            commandEncoder.SetFragmentStructuredBuffer(Slot, (Buffer) _data);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;

                case EffectResourceType.Texture:
                    switch (ShaderStage)
                    {
                        case EffectResourceShaderStage.VertexShader:
                            commandEncoder.SetVertexTexture(Slot, (Texture) _data);
                            break;

                        case EffectResourceShaderStage.PixelShader:
                            commandEncoder.SetFragmentTexture(Slot, (Texture) _data);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    break;

                case EffectResourceType.Sampler:
                    switch (ShaderStage)
                    {
                        case EffectResourceShaderStage.VertexShader:
                            commandEncoder.SetVertexSampler(Slot, (SamplerState) _data);
                            break;

                        case EffectResourceShaderStage.PixelShader:
                            commandEncoder.SetFragmentSampler(Slot, (SamplerState) _data);
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
