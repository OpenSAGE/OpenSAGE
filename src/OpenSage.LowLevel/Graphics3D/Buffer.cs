using System;

namespace LL.Graphics3D
{
    public partial class Buffer : GraphicsDeviceChild
    {
        public static Buffer CreateDynamic(
            GraphicsDevice graphicsDevice,
            uint sizeInBytes,
            BufferBindFlags flags)
        {
            return new Buffer(
                graphicsDevice,
                sizeInBytes,
                1u,
                flags,
                ResourceUsage.Dynamic,
                null);
        }

        public uint ElementSizeInBytes { get; }
        public uint ElementCount { get; }

        public uint SizeInBytes { get; }

        public BufferBindFlags BindFlags { get; }
        public ResourceUsage Usage { get; }

        protected Buffer(
            GraphicsDevice graphicsDevice,
            uint elementSizeInBytes,
            uint elementCount,
            BufferBindFlags flags,
            ResourceUsage usage,
            byte[] initialData)
            : base(graphicsDevice)
        {
            if (usage == ResourceUsage.Dynamic && initialData != null)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (usage == ResourceUsage.Static && initialData == null)
            {
                throw new ArgumentOutOfRangeException();
            }

            ElementSizeInBytes = elementSizeInBytes;
            ElementCount = elementCount;

            var alignedElementSize = flags.HasFlag(BufferBindFlags.ConstantBuffer)
                ? PlatformGetAlignedSize(elementSizeInBytes)
                : elementSizeInBytes;

            SizeInBytes = alignedElementSize * elementCount;

            BindFlags = flags;
            Usage = usage;

            PlatformConstruct(
                graphicsDevice,
                SizeInBytes,
                elementSizeInBytes,
                flags,
                usage,
                initialData);
        }

        protected void EnsureDynamic()
        {
            if (Usage != ResourceUsage.Dynamic)
            {
                throw new InvalidOperationException();
            }
        }

        public void SetData(byte[] data)
        {
            EnsureDynamic();

            PlatformSetData(data, data.Length);
        }
    }
}
