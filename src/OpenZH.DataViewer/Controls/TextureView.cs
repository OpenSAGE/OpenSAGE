using System;
using System.IO;
using OpenZH.Data.Dds;
using OpenZH.Data.Tga;
using OpenZH.DataViewer.ViewModels;
using OpenZH.Graphics;

namespace OpenZH.DataViewer.Controls
{
    public sealed class TextureView : RenderedView
    {
        private Texture _texture;
        private DescriptorSetLayout _descriptorSetLayout;
        private DescriptorSet _descriptorSet;
        private PipelineLayout _pipelineLayout;
        private PipelineState _pipelineState;

        public TextureFormat TextureFormat { get; set; }
        public Func<Stream> OpenStream { get; set; }

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            switch (TextureFormat)
            {
                case TextureFormat.Dds:
                    DdsFile ddsFile;
                    using (var textureStream = OpenStream())
                    {
                        ddsFile = DdsFile.FromStream(textureStream);
                    }
                    _texture = CreateTextureFromDds(graphicsDevice, ddsFile);
                    break;

                case TextureFormat.Tga:
                    TgaFile tgaFile;
                    using (var textureStream = OpenStream())
                    {
                        tgaFile = TgaFile.FromStream(textureStream);
                    }
                    _texture = CreateTextureFromTga(graphicsDevice, tgaFile);
                    break;

                default:
                    throw new InvalidOperationException();
            }

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
                DescriptorSetLayouts = new[] { _descriptorSetLayout }
            });

            var shaderLibrary = new ShaderLibrary(graphicsDevice);

            var pixelShader = new Shader(shaderLibrary, "TestPS");
            var vertexShader = new Shader(shaderLibrary, "TestVS");

            var vertexDescriptor = new VertexDescriptor();
            vertexDescriptor.SetAttributeDescriptor(0, "POSITION", 0, VertexFormat.Float3, 0, 0);
            vertexDescriptor.SetLayoutDescriptor(0, 12);

            _pipelineState = new PipelineState(graphicsDevice, new PipelineStateDescription
            {
                PipelineLayout = _pipelineLayout,
                PixelShader = pixelShader,
                RenderTargetFormat = PixelFormat.Rgba8UNorm,
                VertexDescriptor = vertexDescriptor,
                VertexShader = vertexShader
            });
        }

        private static Texture CreateTextureFromDds(GraphicsDevice graphicsDevice, DdsFile ddsFile)
        {
            var texture = Texture.CreateTexture2D(
                graphicsDevice,
                ToPixelFormat(ddsFile.ImageFormat),
                (int) ddsFile.Header.Width,
                (int) ddsFile.Header.Height,
                (int) ddsFile.Header.MipMapCount);

            var uploadBatch = new ResourceUploadBatch(graphicsDevice);
            uploadBatch.Begin();

            for (var i = 0; i < ddsFile.Header.MipMapCount; i++)
            {
                texture.SetData(
                    uploadBatch,
                    i,
                    ddsFile.MipMaps[i].Data,
                    (int) ddsFile.MipMaps[i].RowPitch);
            }

            uploadBatch.End(graphicsDevice.CommandQueue);

            return texture;
        }

        private static PixelFormat ToPixelFormat(DdsImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case DdsImageFormat.Bc1:
                    return PixelFormat.Bc1;

                case DdsImageFormat.Bc2:
                    return PixelFormat.Bc2;

                case DdsImageFormat.Bc3:
                    return PixelFormat.Bc3;

                default:
                    throw new ArgumentOutOfRangeException(nameof(imageFormat));
            }
        }

        private static Texture CreateTextureFromTga(GraphicsDevice graphicsDevice, TgaFile tgaFile)
        {
            if (tgaFile.Header.ImageType != TgaImageType.UncompressedRgb)
            {
                throw new InvalidOperationException();
            }

            var texture = Texture.CreateTexture2D(
                graphicsDevice,
                PixelFormat.Rgba8UNorm,
                tgaFile.Header.Width,
                tgaFile.Header.Height,
                1);

            var uploadBatch = new ResourceUploadBatch(graphicsDevice);
            uploadBatch.Begin();

            var data = ConvertTgaPixels(tgaFile.Header.ImagePixelSize, tgaFile.Data);

            texture.SetData(
                uploadBatch,
                0,
                data,
                tgaFile.Header.Width * 4);

            uploadBatch.End(graphicsDevice.CommandQueue);

            return texture;
        }

        private static byte[] ConvertTgaPixels(byte pixelSize, byte[] data)
        {
            switch (pixelSize)
            {
                case 24: // BGR
                     {
                        var result = new byte[data.Length / 3 * 4];
                        var resultIndex = 0;
                        for (var i = 0; i < data.Length; i += 3)
                        {
                            result[resultIndex++] = data[i + 2]; // R
                            result[resultIndex++] = data[i + 1]; // G
                            result[resultIndex++] = data[i + 0]; // B
                            result[resultIndex++] = 255;         // A
                        }
                        return result;
                    }

                case 32: // BGRA
                    {
                        var result = new byte[data.Length];
                        var resultIndex = 0;
                        for (var i = 0; i < data.Length; i += 4)
                        {
                            result[resultIndex++] = data[i + 3]; // R
                            result[resultIndex++] = data[i + 2]; // G
                            result[resultIndex++] = data[i + 1]; // B
                            result[resultIndex++] = data[i + 0]; // A
                        }
                        return result;
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(pixelSize));
            }
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

            //commandEncoder.SetVertexBuffer(0, _vertexBuffer);

            //commandEncoder.DrawIndexed(
            //    PrimitiveType.TriangleList,
            //    4,
            //    IndexType.UInt16,
            //    indexBuffer,
            //    0);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }
    }
}
