using LLGfx;
using OpenSage.Graphics.Util;

namespace OpenSage.Graphics.Effects
{
    public abstract class Effect : GraphicsObject
    {
        private readonly GraphicsDevice _graphicsDevice;

        private PipelineLayout _pipelineLayout;

        private VertexDescriptor _vertexDescriptor;
        private readonly Shader _vertexShader;
        private readonly Shader _pixelShader;

        private readonly PipelineStateCache _pipelineStateCache;

        protected GraphicsDevice GraphicsDevice => _graphicsDevice;

        protected Effect(
            GraphicsDevice graphicsDevice,
            string vertexShaderName,
            string pixelShaderName)
        {
            _graphicsDevice = graphicsDevice;

            _vertexShader = AddDisposable(new Shader(graphicsDevice.ShaderLibrary, vertexShaderName));
            _pixelShader = AddDisposable(new Shader(graphicsDevice.ShaderLibrary, pixelShaderName));

            _pipelineStateCache = AddDisposable(new PipelineStateCache(graphicsDevice));
        }

        protected void Initialize(
            ref VertexDescriptor vertexDescriptor, 
            ref PipelineLayoutDescription pipelineLayoutDescription)
        {
            _vertexDescriptor = vertexDescriptor;

            _pipelineLayout = AddDisposable(new PipelineLayout(_graphicsDevice, ref pipelineLayoutDescription));
        }

        protected PipelineState GetPipelineState(ref PipelineStateDescription description)
        {
            description.PipelineLayout = _pipelineLayout;
            description.RenderTargetFormat = _graphicsDevice.BackBufferFormat;
            description.VertexDescriptor = _vertexDescriptor;
            description.VertexShader = _vertexShader;
            description.PixelShader = _pixelShader;

            return _pipelineStateCache.GetPipelineState(description);
        }

        public void Apply(CommandEncoder commandEncoder)
        {
            commandEncoder.SetPipelineLayout(_pipelineLayout);
        }
    }
}
