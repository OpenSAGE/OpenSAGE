using System;
using System.Collections.Generic;

namespace LLGfx.Effects
{
    public abstract class Effect : GraphicsObject
    {
        private readonly GraphicsDevice _graphicsDevice;

        private PipelineLayout _pipelineLayout;

        private VertexDescriptor _vertexDescriptor;
        private readonly Shader _vertexShader;
        private readonly Shader _pixelShader;

        private readonly Dictionary<EffectPipelineStateHandle, PipelineState> _cachedPipelineStates;

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
            VertexDescriptor vertexDescriptor,
            PipelineLayoutDescription pipelineLayoutDescription)
        {
            _graphicsDevice = graphicsDevice;

            _vertexShader = AddDisposable(new Shader(graphicsDevice.ShaderLibrary, vertexShaderName));
            _pixelShader = AddDisposable(new Shader(graphicsDevice.ShaderLibrary, pixelShaderName));

            _cachedPipelineStates = new Dictionary<EffectPipelineStateHandle, PipelineState>();

            _vertexDescriptor = vertexDescriptor;

            _pipelineLayout = AddDisposable(new PipelineLayout(_graphicsDevice, ref pipelineLayoutDescription));
        }

        public void Begin(CommandEncoder commandEncoder)
        {
            commandEncoder.SetPipelineLayout(_pipelineLayout);

            _dirtyFlags |= EffectDirtyFlags.PipelineState;

            OnBegin();
        }

        protected abstract void OnBegin();

        public void Apply(CommandEncoder commandEncoder)
        {
            if (_dirtyFlags.HasFlag(EffectDirtyFlags.PipelineState))
            {
                commandEncoder.SetPipelineState(_pipelineState);

                _dirtyFlags &= ~EffectDirtyFlags.PipelineState;
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

                description.PipelineLayout = _pipelineLayout;
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
    }
}
