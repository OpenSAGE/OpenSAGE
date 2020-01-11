using System.Collections.Generic;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics
{
    internal sealed class StandardGraphicsResources : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<uint, DeviceBuffer> _nullStructuredBuffers;

        public Sampler LinearClampSampler { get; }
        public Sampler PointClampSampler { get; }

        public Texture NullTexture { get; }
        public Texture SolidWhiteTexture { get; }
        public Texture PlaceholderTexture { get; }

        public StandardGraphicsResources(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            var linearClampSamplerDescription = SamplerDescription.Linear;
            linearClampSamplerDescription.AddressModeU = SamplerAddressMode.Clamp;
            linearClampSamplerDescription.AddressModeV = SamplerAddressMode.Clamp;
            linearClampSamplerDescription.AddressModeW = SamplerAddressMode.Clamp;
            LinearClampSampler = AddDisposable(
                graphicsDevice.ResourceFactory.CreateSampler(ref linearClampSamplerDescription));

            var pointClampSamplerDescription = SamplerDescription.Point;
            pointClampSamplerDescription.AddressModeU = SamplerAddressMode.Clamp;
            pointClampSamplerDescription.AddressModeV = SamplerAddressMode.Clamp;
            pointClampSamplerDescription.AddressModeW = SamplerAddressMode.Clamp;
            PointClampSampler = AddDisposable(
                graphicsDevice.ResourceFactory.CreateSampler(ref pointClampSamplerDescription));

            NullTexture = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled)));
            NullTexture.Name = "Null Texture";

            SolidWhiteTexture = AddDisposable(graphicsDevice.CreateStaticTexture2D(
                1, 1, 1,
                new TextureMipMapData(
                    new byte[] { 255, 255, 255, 255 },
                    4, 4, 1, 1),
                PixelFormat.R8_G8_B8_A8_UNorm));
            SolidWhiteTexture.Name = "Solid White Texture";

            PlaceholderTexture = AddDisposable(graphicsDevice.CreateStaticTexture2D(
                1, 1, 1,
                new TextureMipMapData(
                    new byte[] { 255, 105, 180, 255 },
                    4, 4, 1, 1),
                PixelFormat.R8_G8_B8_A8_UNorm));
            PlaceholderTexture.Name = "Placeholder Texture";

            _nullStructuredBuffers = new Dictionary<uint, DeviceBuffer>();
        }

        internal DeviceBuffer GetNullStructuredBuffer(uint size)
        {
            if (!_nullStructuredBuffers.TryGetValue(size, out var result))
            {
                _nullStructuredBuffers.Add(size, result = AddDisposable(_graphicsDevice.ResourceFactory.CreateBuffer(
                    new BufferDescription(
                        size,
                        BufferUsage.StructuredBufferReadOnly,
                        size,
                        true))));
            }
            return result;
        }
    }
}
