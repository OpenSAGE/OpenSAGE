using OpenZH.Data;
using OpenZH.Graphics;
using OpenZH.Graphics.LowLevel;

namespace OpenZH.DataViewer.ViewModels
{
    public sealed class TextureFileContentViewModel : RenderedFileContentViewModel
    {
        private Texture _texture;
        private DescriptorSetLayout _descriptorSetLayout;
        private DescriptorSet _descriptorSet;
        private PipelineLayout _pipelineLayout;
        private PipelineState _pipelineState;

        public override bool RedrawsOnTimer => false;

        public TextureFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
        }

        public override void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            var uploadBatch = new ResourceUploadBatch(graphicsDevice);
            uploadBatch.Begin();

            _texture = TextureLoader.LoadTexture(graphicsDevice, uploadBatch, File);

            uploadBatch.End();

            _descriptorSetLayout = new DescriptorSetLayout(new DescriptorSetLayoutDescription
            {
                Visibility = ShaderStageVisibility.Pixel,
                Bindings = new[]
                {
                    new DescriptorSetLayoutBinding(DescriptorType.Texture, 0, 1)
                }
            });

            _descriptorSet = new DescriptorSet(graphicsDevice, _descriptorSetLayout);

            _descriptorSet.SetTexture(0, _texture);

            _pipelineLayout = new PipelineLayout(graphicsDevice, new PipelineLayoutDescription
            {
                DescriptorSetLayouts = new[] { _descriptorSetLayout },
                StaticSamplerStates = new[]
                {
                    new StaticSamplerDescription
                    {
                        Visibility = ShaderStageVisibility.Pixel,
                        ShaderRegister = 0,
                        SamplerStateDescription = new SamplerStateDescription
                        {
                            Filter = SamplerFilter.MinMagMipPoint
                        }
                    }
                }
            });

            var shaderLibrary = new ShaderLibrary(graphicsDevice);

            var pixelShader = new Shader(shaderLibrary, "SpritePS");
            var vertexShader = new Shader(shaderLibrary, "SpriteVS");

            _pipelineState = new PipelineState(graphicsDevice, new PipelineStateDescription
            {
                PipelineLayout = _pipelineLayout,
                PixelShader = pixelShader,
                RenderTargetFormat = swapChain.BackBufferFormat,
                VertexShader = vertexShader
            });
        }

        public override void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            var renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.SetRenderTargetDescriptor(
                swapChain.GetNextRenderTarget(),
                LoadAction.Clear,
                new ColorRgba(0.5f, 0.5f, 0.5f, 1));

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            commandEncoder.SetPipelineState(_pipelineState);

            commandEncoder.SetPipelineLayout(_pipelineLayout);

            commandEncoder.SetDescriptorSet(0, _descriptorSet);

            commandEncoder.SetViewport(new Viewport
            {
                X = 0,
                Y = 0,
                Width = (int) swapChain.BackBufferWidth,
                Height = (int) swapChain.BackBufferHeight,
                MinDepth = 0,
                MaxDepth = 1
            });

            commandEncoder.Draw(PrimitiveType.TriangleList, 0, 3);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }
    }
}
