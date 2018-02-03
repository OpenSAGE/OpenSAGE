using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics.Effects
{
    public sealed class Effect : DisposableBase
    {
        private static byte _nextID = 0;

        private readonly GraphicsDevice _graphicsDevice;

        private VertexLayoutDescription[] _vertexDescriptors;
        private readonly Shader _vertexShader;
        private readonly Shader _pixelShader;

        private readonly ResourceLayout[] _resourceLayouts;

        private readonly Dictionary<EffectPipelineStateHandle, Pipeline> _cachedPipelineStates;

        private readonly EffectParameter[] _parameters;

        private EffectPipelineStateHandle _pipelineStateHandle;
        private Pipeline _pipelineState;

        private EffectDirtyFlags _dirtyFlags;

        public GraphicsDevice GraphicsDevice => _graphicsDevice;

        public byte ID { get; }

        [Flags]
        private enum EffectDirtyFlags
        {
            None = 0,

            PipelineState = 0x1
        }

        public Effect(
            GraphicsDevice graphicsDevice,
            string shaderName,
            VertexLayoutDescription vertexDescriptor,
            ResourceLayoutElementDescription[] resourceLayoutDescriptions,
            bool useNewShaders = false)
            : this(graphicsDevice, shaderName, new[] { vertexDescriptor }, resourceLayoutDescriptions, useNewShaders)
        {

        }

        public Effect(
            GraphicsDevice graphicsDevice,
            string shaderName,
            VertexLayoutDescription[] vertexDescriptors,
            ResourceLayoutElementDescription[] resourceLayoutDescriptions,
            bool useNewShaders = false)
        {
            _graphicsDevice = graphicsDevice;

            ID = _nextID++;

            if (useNewShaders)
            {
                using (var shaderStream = typeof(Effect).Assembly.GetManifestResourceStream($"OpenSage.Graphics.Shaders.Compiled.{shaderName}-vertex.hlsl.bytes"))
                {
                    var vertexShaderBytecode = shaderStream.ReadAllBytes();
                    _vertexShader = AddDisposable(graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytecode, "VS")));
                }

                using (var shaderStream = typeof(Effect).Assembly.GetManifestResourceStream($"OpenSage.Graphics.Shaders.Compiled.{shaderName}-fragment.hlsl.bytes"))
                {
                    var pixelShaderBytecode = shaderStream.ReadAllBytes();
                    _pixelShader = AddDisposable(graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, pixelShaderBytecode, "PS")));
                }
            }
            else
            {
                using (var shaderStream = typeof(Effect).Assembly.GetManifestResourceStream($"OpenSage.Graphics.Shaders.{shaderName}.fxo"))
                using (var shaderReader = new BinaryReader(shaderStream))
                {
                    var vertexShaderBytecodeLength = shaderReader.ReadInt32();
                    var vertexShaderBytecode = shaderReader.ReadBytes(vertexShaderBytecodeLength);
                    _vertexShader = AddDisposable(graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytecode, "VS")));

                    var pixelShaderBytecodeLength = shaderReader.ReadInt32();
                    var pixelShaderBytecode = shaderReader.ReadBytes(pixelShaderBytecodeLength);
                    _pixelShader = AddDisposable(graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, pixelShaderBytecode, "PS")));
                }
            }

            _cachedPipelineStates = new Dictionary<EffectPipelineStateHandle, Pipeline>();

            _vertexDescriptors = vertexDescriptors;

            _parameters = new EffectParameter[resourceLayoutDescriptions.Length];
            _resourceLayouts = new ResourceLayout[_parameters.Length];
            for (var i = 0u; i < _resourceLayouts.Length; i++)
            {
                _parameters[i] = AddDisposable(new EffectParameter(graphicsDevice, resourceLayoutDescriptions[i], i));
                _resourceLayouts[i] = _parameters[i].ResourceLayout;
            }
        }

        internal EffectParameter GetParameter(uint slot)
        {
            return _parameters[slot];
        }

        public void Begin(CommandList commandEncoder)
        {
            _dirtyFlags |= EffectDirtyFlags.PipelineState;

            foreach (var parameter in _parameters)
            {
                parameter.SetDirty();
            }
        }

        public void Apply(CommandList commandEncoder)
        {
            if (_dirtyFlags.HasFlag(EffectDirtyFlags.PipelineState))
            {
                commandEncoder.SetPipeline(_pipelineState);

                _dirtyFlags &= ~EffectDirtyFlags.PipelineState;
            }

            foreach (var parameter in _parameters)
            {
                parameter.ApplyChanges(commandEncoder);
            }
        }

        public void SetPipelineState(EffectPipelineStateHandle pipelineStateHandle)
        {
            if (_pipelineStateHandle == pipelineStateHandle)
            {
                return;
            }

            _pipelineStateHandle = pipelineStateHandle;
            _pipelineState = GetPipelineState(pipelineStateHandle);
            _dirtyFlags |= EffectDirtyFlags.PipelineState;
        }

        private Pipeline GetPipelineState(EffectPipelineStateHandle pipelineStateHandle)
        {
            if (!_cachedPipelineStates.TryGetValue(pipelineStateHandle, out var result))
            {
                var description = new GraphicsPipelineDescription(
                    pipelineStateHandle.EffectPipelineState.BlendState,
                    pipelineStateHandle.EffectPipelineState.DepthStencilState,
                    pipelineStateHandle.EffectPipelineState.RasterizerState,
                    PrimitiveTopology.TriangleList,
                    new ShaderSetDescription(
                        _vertexDescriptors,
                        new[] { _vertexShader, _pixelShader }),
                    _resourceLayouts,
                    pipelineStateHandle.EffectPipelineState.OutputDescription);

                _cachedPipelineStates[pipelineStateHandle] = result = AddDisposable(_graphicsDevice.ResourceFactory.CreateGraphicsPipeline(ref description));
            }

            return result;
        }
    }
}
