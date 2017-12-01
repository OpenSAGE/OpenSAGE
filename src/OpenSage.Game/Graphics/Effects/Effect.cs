using System;
using System.Collections.Generic;
using System.Linq;
using LLGfx;
using Buffer = LLGfx.Buffer;

namespace OpenSage.Graphics.Effects
{
    public abstract partial class Effect : GraphicsObject
    {
        private readonly GraphicsDevice _graphicsDevice;

        private VertexDescriptor _vertexDescriptor;
        private readonly Shader _vertexShader;
        private readonly Shader _pixelShader;

        private readonly Dictionary<EffectPipelineStateHandle, PipelineState> _cachedPipelineStates;

        private readonly Dictionary<string, EffectParameter> _parameters;

        private PipelineState _pipelineState;

        private EffectDirtyFlags _dirtyFlags;

        public GraphicsDevice GraphicsDevice => _graphicsDevice;

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

            _vertexShader = AddDisposable(new Shader(graphicsDevice.ShaderLibrary, vertexShaderName));
            _pixelShader = AddDisposable(new Shader(graphicsDevice.ShaderLibrary, pixelShaderName));

            _cachedPipelineStates = new Dictionary<EffectPipelineStateHandle, PipelineState>();

            _vertexDescriptor = vertexDescriptor;

            _parameters = _vertexShader.ResourceBindings
                .Concat(_pixelShader.ResourceBindings)
                .Select(x => AddDisposable(new EffectParameter(graphicsDevice, x)))
                // TODO: This is awful.
                .Where(x => x.Name != "GlobalConstantsVS" && x.Name != "GlobalConstantsPS" && x.Name != "LightingConstants")
                .ToDictionary(x => x.Name);
        }

        internal EffectParameter GetParameter(string name) => _parameters[name];

        public void Begin(CommandEncoder commandEncoder)
        {
            _dirtyFlags |= EffectDirtyFlags.PipelineState;

            foreach (var parameter in _parameters.Values)
            {
                parameter.ResetDirty();
            }

            OnBegin();
        }

        protected virtual void OnBegin() { }

        public void Apply(CommandEncoder commandEncoder)
        {
            if (_dirtyFlags.HasFlag(EffectDirtyFlags.PipelineState))
            {
                commandEncoder.SetPipelineState(_pipelineState);

                _dirtyFlags &= ~EffectDirtyFlags.PipelineState;
            }

            OnApply();

            foreach (var parameter in _parameters.Values)
            {
                parameter.ApplyChanges(commandEncoder);
            }
        }

        protected virtual void OnApply() { }

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

        public void SetConstantBufferField<T>(string name, string fieldName, T value)
            where T : struct
        {
            SetConstantBufferField(name, fieldName, ref value);
        }

        public void SetConstantBufferField<T>(string name, string fieldName, ref T value)
            where T : struct
        {
            if (!_parameters.TryGetValue(name, out var parameter))
            {
                throw new InvalidOperationException();
            }

            parameter.SetConstantBufferField(fieldName, ref value);
        }

        private void SetValueImpl(string name, object value)
        {
            if (!_parameters.TryGetValue(name, out var parameter))
            {
                throw new InvalidOperationException();
            }

            parameter.SetData(value);
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
}
