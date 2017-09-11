using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Data;
using OpenSage.Graphics;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class TextureFileContentViewModel : FileContentViewModel
    {
        private Texture _texture;
        private DescriptorSetLayout _descriptorSetLayout;
        private DescriptorSet _descriptorSet;
        private PipelineLayout _pipelineLayout;
        private PipelineState _pipelineState;

        private TextureConstants _textureConstants;
        private DynamicBuffer _textureConstantBuffer;

        public int TextureWidth => _texture?.Width ?? 0;
        public int TextureHeight => _texture?.Height ?? 0;

        public IEnumerable<uint> MipMapLevels => Enumerable
            .Range(0, _texture?.MipMapCount ?? 0)
            .Select(x => (uint) x);

        private uint _selectedMipMapLevel;
        public uint SelectedMipMapLevel
        {
            get { return _selectedMipMapLevel; }
            set
            {
                _selectedMipMapLevel = value;
                NotifyOfPropertyChange();
            }
        }

        public TextureFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
        }

        public void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            var uploadBatch = new ResourceUploadBatch(graphicsDevice);
            uploadBatch.Begin();

            _texture = TextureLoader.LoadTexture(graphicsDevice, uploadBatch, File);
            NotifyOfPropertyChange(nameof(TextureWidth));
            NotifyOfPropertyChange(nameof(TextureHeight));
            NotifyOfPropertyChange(nameof(MipMapLevels));

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
                InlineDescriptorLayouts = new[]
                {
                    new InlineDescriptorLayoutDescription
                    {
                        Visibility = ShaderStageVisibility.Pixel,
                        DescriptorType = DescriptorType.ConstantBuffer,
                        ShaderRegister = 0
                    }
                },
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

            _textureConstantBuffer = DynamicBuffer.Create<TextureConstants>(graphicsDevice);

            var shaderLibrary = new ShaderLibrary(graphicsDevice);

            var pixelShader = new Shader(shaderLibrary, "SpritePS");
            var vertexShader = new Shader(shaderLibrary, "SpriteVS");

            var pipelineStateDescription = PipelineStateDescription.Default();
            pipelineStateDescription.PipelineLayout = _pipelineLayout;
            pipelineStateDescription.PixelShader = pixelShader;
            pipelineStateDescription.RenderTargetFormat = graphicsDevice.BackBufferFormat;
            pipelineStateDescription.VertexShader = vertexShader;
            pipelineStateDescription.IsFrontCounterClockwise = false;
            pipelineStateDescription.IsDepthEnabled = false;
            pipelineStateDescription.IsDepthWriteEnabled = false;

            _pipelineState = new PipelineState(graphicsDevice, pipelineStateDescription);
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            _textureConstants.MipMapLevel = _selectedMipMapLevel;
            _textureConstantBuffer.SetData(ref _textureConstants);

            var renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.SetRenderTargetDescriptor(
                swapChain.GetNextRenderTarget(),
                LoadAction.Clear,
                new ColorRgba(0.5f, 0.5f, 0.5f, 1));

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            commandEncoder.SetPipelineLayout(_pipelineLayout);

            commandEncoder.SetPipelineState(_pipelineState);

            commandEncoder.SetInlineConstantBuffer(0, _textureConstantBuffer);

            commandEncoder.SetDescriptorSet(1, _descriptorSet);

            commandEncoder.SetViewport(new Viewport
            {
                X = 0,
                Y = 0,
                Width = swapChain.BackBufferWidth,
                Height = swapChain.BackBufferHeight,
                MinDepth = 0,
                MaxDepth = 1
            });

            commandEncoder.Draw(PrimitiveType.TriangleList, 0, 3);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TextureConstants
        {
            public uint MipMapLevel;
        }
    }
}
