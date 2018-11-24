using System;
using System.Runtime.InteropServices;
using Veldrid;

namespace OpenSage.Utilities.Extensions
{
    internal static class ResourceFactoryExtensions
    {
        public static DeviceBuffer CreateStaticBuffer<T>(
            this GraphicsDevice graphicsDevice,
            T data,
            BufferUsage usage,
            uint structureByteStride = 0u)
            where T : struct
        {
            return graphicsDevice.CreateStaticBuffer(
                new ReadOnlySpan<T>(new[] { data }),
                usage,
                structureByteStride);
        }

        public static DeviceBuffer CreateStaticBuffer<T>(
            this GraphicsDevice graphicsDevice,
            T[] data,
            BufferUsage usage,
            uint structureByteStride = 0u)
            where T : struct
        {
            return graphicsDevice.CreateStaticBuffer(
                new ReadOnlySpan<T>(data),
                usage,
                structureByteStride);
        }

        public static unsafe DeviceBuffer CreateStaticBuffer<T>(
            this GraphicsDevice graphicsDevice,
            ReadOnlySpan<T> data,
            BufferUsage usage,
            uint structureByteStride = 0u)
            where T : struct
        {
            var bufferSize = (uint) (data.Length * Marshal.SizeOf<T>());

            if (usage == BufferUsage.UniformBuffer)
            {
                bufferSize = GetUniformBufferSize(bufferSize);
            }

            var staging = graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(bufferSize, BufferUsage.Staging, 0));

            var rawBuffer = usage == BufferUsage.StructuredBufferReadOnly;

            var result = graphicsDevice.ResourceFactory.CreateBuffer(
                new BufferDescription(bufferSize, usage, structureByteStride, rawBuffer));

            var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
            commandList.Begin();

            var map = graphicsDevice.Map(staging, MapMode.Write, 0);

            var destinationSpan = new Span<T>(map.Data.ToPointer(), (int) bufferSize);
            data.CopyTo(destinationSpan);

            graphicsDevice.Unmap(staging, 0);

            commandList.CopyBuffer(staging, 0, result, 0, bufferSize);

            commandList.End();

            graphicsDevice.SubmitCommands(commandList);

            graphicsDevice.DisposeWhenIdle(commandList);
            graphicsDevice.DisposeWhenIdle(staging);

            return result;
        }

        /// <summary>
        /// Buffer sizes must be multiples of 16.
        /// </summary>
        private static uint GetUniformBufferSize(uint size)
        {
            return (size % 16 == 0)
                ? size
                : (16 - size % 16) + size;
        }

        public static DeviceBuffer CreateStaticStructuredBuffer<T>(
           this GraphicsDevice graphicsDevice,
           T[] data)
           where T : struct
        {
            return graphicsDevice.CreateStaticBuffer(
                data,
                BufferUsage.StructuredBufferReadOnly,
                (uint) Marshal.SizeOf<T>());
        }

        public unsafe static Texture CreateStaticTexture2D(
            this GraphicsDevice graphicsDevice,
            uint width, uint height, uint arrayLayers,
            in TextureMipMapData mipMapData,
            PixelFormat pixelFormat)
        {
            return graphicsDevice.CreateStaticTexture2D(
                width, height, arrayLayers,
                new[]
                {
                    mipMapData
                },
                pixelFormat,
                false);
        }

        public unsafe static Texture CreateStaticTexture2D(
            this GraphicsDevice graphicsDevice,
            uint width, uint height, uint arrayLayers,
            TextureMipMapData[] mipMapData,
            PixelFormat pixelFormat,
            bool isCubemap)
        {
            var mipMapLevels = (uint) mipMapData.Length / arrayLayers;

            var staging = graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    width,
                    height,
                    mipMapLevels,
                    1,
                    pixelFormat,
                    TextureUsage.Staging));

            var usage = TextureUsage.Sampled;
            if (isCubemap)
            {
                usage |= TextureUsage.Cubemap;
            }

            var result = graphicsDevice.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    width,
                    height,
                    mipMapLevels,
                    arrayLayers,
                    pixelFormat,
                    usage));

            var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
            commandList.Begin();

            for (var arrayLayer = 0u; arrayLayer < arrayLayers; arrayLayer++)
            {
                for (var level = 0u; level < mipMapLevels; level++)
                {
                    var mipMap = mipMapData[(arrayLayer * mipMapLevels) + level];

                    var mipMapWidth = Math.Min(width, mipMap.Width);
                    var mipMapHeight = Math.Min(height, mipMap.Height);

                    fixed (void* pin = mipMap.Data)
                    {
                        graphicsDevice.UpdateTexture(
                            staging,
                            new IntPtr(pin),
                            (uint) mipMap.Data.Length,
                            0, 0, 0,
                            mipMapWidth,
                            mipMapHeight,
                            1,
                            level,
                            0);

                        commandList.CopyTexture(
                            staging, 0, 0, 0, level, 0,
                            result, 0, 0, 0, level, arrayLayer,
                            mipMapWidth,
                            mipMapHeight,
                            1, 1);
                    }
                }
            }

            commandList.End();

            graphicsDevice.SubmitCommands(commandList);

            graphicsDevice.DisposeWhenIdle(commandList);
            graphicsDevice.DisposeWhenIdle(staging);

            return result;
        }
    }
}
