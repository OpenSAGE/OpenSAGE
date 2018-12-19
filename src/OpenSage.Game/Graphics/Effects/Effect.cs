using System;
using System.Collections.Generic;
using OpenSage.Graphics.Shaders;
using OpenSage.Utilities.Extensions;
using Veldrid;
using Veldrid.SPIRV;

namespace OpenSage.Graphics.Effects
{
    public sealed class Effect : DisposableBase
    {
        private static byte _nextID = 0;
        private readonly VertexLayoutDescription[] _vertexDescriptors;
        private readonly Shader _vertexShader;
        private readonly Shader _pixelShader;

        private readonly ResourceLayout[] _resourceLayouts;

        private readonly Dictionary<EffectPipelineStateHandle, Pipeline> _cachedPipelineStates;

        private readonly Dictionary<string, EffectParameter> _parameters;

        private readonly PrimitiveTopology _primitiveTopology;

        private EffectPipelineStateHandle _pipelineStateHandle;
        private Pipeline _pipelineState;

        private EffectDirtyFlags _dirtyFlags;

        public GraphicsDevice GraphicsDevice { get; }
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
            PrimitiveTopology primitiveTopology,
            params VertexLayoutDescription[] vertexDescriptors)
        {
            _primitiveTopology = primitiveTopology;

            GraphicsDevice = graphicsDevice;

            ID = _nextID++;

#if DEBUG
            const bool debug = true;
#else
            const bool debug = false;
#endif

            ShaderDefinition shaderDefinition;

            var resources = typeof(Effect).Assembly.GetManifestResourceNames();

            byte[] ReadShader(string shaderType)
            {
                var bytecodeShaderName = $"OpenSage.Assets.Shaders.{shaderName}.{shaderType}.spv";
                using (var shaderStream = typeof(Effect).Assembly.GetManifestResourceStream(bytecodeShaderName))
                {
                    return shaderStream.ReadAllBytes();
                }
            }

            var vsBytes = ReadShader("vert");
            var fsBytes = ReadShader("frag");

            var shaders = graphicsDevice.ResourceFactory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, vsBytes, "main", debug),
                new ShaderDescription(ShaderStages.Fragment, fsBytes, "main", debug),
                new CrossCompileOptions());

            _vertexShader = AddDisposable(shaders[0]);
            _pixelShader = AddDisposable(shaders[1]);

            _vertexShader.Name = $"{shaderName}.vert";
            _pixelShader.Name = $"{shaderName}.frag";

            shaderDefinition = ShaderDefinitions.GetShaderDefinition(shaderName);

            _cachedPipelineStates = new Dictionary<EffectPipelineStateHandle, Pipeline>();

            _vertexDescriptors = vertexDescriptors;

            _parameters = new Dictionary<string, EffectParameter>();
            _resourceLayouts = new ResourceLayout[shaderDefinition.ResourceBindings.Count];

            for (var i = 0u; i < shaderDefinition.ResourceBindings.Count; i++)
            {
                var resourceBinding = shaderDefinition.ResourceBindings[(int) i];
                var resourceLayoutDescription = new ResourceLayoutElementDescription(
                    resourceBinding.Name,
                    resourceBinding.Kind,
                    resourceBinding.Stages);

                var parameter = AddDisposable(new EffectParameter(
                    graphicsDevice,
                    resourceBinding,
                    resourceLayoutDescription,
                    i));

                _parameters[parameter.Name] = parameter;
                _resourceLayouts[i] = parameter.ResourceLayout;
            }
        }

        internal EffectParameter GetParameter(string name, bool throwIfMissing = true)
        {
            if (!_parameters.TryGetValue(name, out var result))
            {
                if (throwIfMissing)
                {
                    throw new InvalidOperationException($"Could not find parameter with name '{name}'.");
                }
                else
                {
                    return null;
                }
            }
            return result;
        }

        public void Begin(CommandList commandEncoder)
        {
            _dirtyFlags |= EffectDirtyFlags.PipelineState;

            foreach (var parameter in _parameters.Values)
            {
                parameter.SetDirty();
            }
        }

        public void ApplyPipelineState(CommandList commandEncoder)
        {
            if (_dirtyFlags.HasFlag(EffectDirtyFlags.PipelineState))
            {
                commandEncoder.SetPipeline(_pipelineState);

                _dirtyFlags &= ~EffectDirtyFlags.PipelineState;
            }
        }

        public void ApplyParameters(CommandList commandEncoder)
        {
            foreach (var parameter in _parameters.Values)
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
                    _primitiveTopology,
                    new ShaderSetDescription(
                        _vertexDescriptors,
                        new[] { _vertexShader, _pixelShader }),
                    _resourceLayouts,
                    pipelineStateHandle.EffectPipelineState.OutputDescription);

                _cachedPipelineStates[pipelineStateHandle] = result = AddDisposable(GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(ref description));
            }

            return result;
        }
    }
}
