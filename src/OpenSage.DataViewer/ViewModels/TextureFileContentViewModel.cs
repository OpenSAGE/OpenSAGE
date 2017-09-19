using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class TextureFileContentViewModel : FileContentViewModel
    {
        private Texture _texture;
        private DescriptorSetLayout _descriptorSetLayout;
        private DescriptorSet _descriptorSet;
        private PipelineLayout _pipelineLayout;
        private PipelineState _pipelineState;

        private DynamicBuffer<TextureConstants> _textureConstantBuffer;

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
            var contentManager = AddDisposable(new ContentManager(File.FileSystem, graphicsDevice));

            _texture = contentManager.Load<Texture>(
                File.FilePath,
                uploadBatch: null, 
                options: new TextureLoadOptions
                {
                    GenerateMipMaps = false
                });

            NotifyOfPropertyChange(nameof(TextureWidth));
            NotifyOfPropertyChange(nameof(TextureHeight));
            NotifyOfPropertyChange(nameof(MipMapLevels));

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

            var pipelineLayoutDescription = new PipelineLayoutDescription
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
            };

            _pipelineLayout = new PipelineLayout(graphicsDevice, ref pipelineLayoutDescription);

            _textureConstantBuffer = DynamicBuffer<TextureConstants>.Create(graphicsDevice);

            var shaderLibrary = new ShaderLibrary(graphicsDevice);

            var pixelShader = new Shader(shaderLibrary, "SpritePS");
            var vertexShader = new Shader(shaderLibrary, "SpriteVS");

            var pipelineStateDescription = PipelineStateDescription.Default;
            pipelineStateDescription.PipelineLayout = _pipelineLayout;
            pipelineStateDescription.PixelShader = pixelShader;
            pipelineStateDescription.RenderTargetFormat = graphicsDevice.BackBufferFormat;
            pipelineStateDescription.VertexShader = vertexShader;
            pipelineStateDescription.RasterizerState = new RasterizerStateDescription
            {
                IsFrontCounterClockwise = false
            };
            pipelineStateDescription.DepthStencilState = DepthStencilStateDescription.None;

            _pipelineState = new PipelineState(graphicsDevice, pipelineStateDescription);
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            _textureConstantBuffer.UpdateData(new TextureConstants
            {
                MipMapLevel = _selectedMipMapLevel
            });

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
