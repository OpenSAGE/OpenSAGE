using LLGfx.Util;
using SharpDX.Direct3D12;

namespace LLGfx
{
    partial class DepthStencilBuffer : GraphicsDeviceChild
    {
        private readonly object _fenceLock = new object();
        private Fence _fence;
        private long _nextFenceValue;
        private DepthStencilBufferPool _pool;
        private DepthStencilBufferImpl _currentDepthStencilBuffer;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height)
        {
            _fence = AddDisposable(graphicsDevice.Device.CreateFence(0, FenceFlags.None));
            _nextFenceValue = 1;

            _pool = AddDisposable(new DepthStencilBufferPool(graphicsDevice, width, height));
        }

        internal CpuDescriptorHandle Acquire()
        {
            _currentDepthStencilBuffer = _pool.AcquireResource(_fence.CompletedValue);
            return _currentDepthStencilBuffer.CpuDescriptorHandle;
        }

        internal void Release()
        {
            long fenceValue;

            lock (_fenceLock)
            {
                GraphicsDevice.CommandQueue.DeviceCommandQueue.Signal(_fence, _nextFenceValue);

                fenceValue = _nextFenceValue;

                _nextFenceValue++;
            }

            _pool.ReleaseResource(fenceValue, _currentDepthStencilBuffer);
        }
    }

    internal sealed class DepthStencilBufferImpl : GraphicsObject
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly DescriptorTablePoolEntry _descriptorTablePoolEntry;

        internal CpuDescriptorHandle CpuDescriptorHandle => _descriptorTablePoolEntry.CpuDescriptorHandle;

        public DepthStencilBufferImpl(GraphicsDevice graphicsDevice, int width, int height)
        {
            _graphicsDevice = graphicsDevice;

            var format = SharpDX.DXGI.Format.D32_Float;

            var resourceDescription = ResourceDescription.Texture2D(
                format,
                width,
                height,
                mipLevels: 1,
                flags: ResourceFlags.AllowDepthStencil | ResourceFlags.DenyShaderResource);

            var texture = AddDisposable(graphicsDevice.Device.CreateCommittedResource(
                new HeapProperties(HeapType.Default),
                HeapFlags.None,
                resourceDescription,
                ResourceStates.DepthWrite,
                new ClearValue { Format = SharpDX.DXGI.Format.D32_Float, DepthStencil = new DepthStencilValue { Depth = 1.0f } }));

            _descriptorTablePoolEntry = graphicsDevice.DescriptorHeapDsv.Reserve(1);

            graphicsDevice.Device.CreateDepthStencilView(
                texture,
                new DepthStencilViewDescription
                {
                    Dimension = DepthStencilViewDimension.Texture2D,
                    Format = format,
                    Flags = DepthStencilViewFlags.None,
                    Texture2D =
                    {
                        MipSlice = 0
                    }
                },
                _descriptorTablePoolEntry.CpuDescriptorHandle);
        }

        protected override void Dispose(bool disposing)
        {
            _graphicsDevice.DescriptorHeapDsv.Release(_descriptorTablePoolEntry);

            base.Dispose(disposing);
        }
    }

    internal sealed class DepthStencilBufferPool : GraphicsResourcePool<DepthStencilBufferImpl>
    {
        private readonly int _width;
        private readonly int _height;

        public DepthStencilBufferPool(GraphicsDevice graphicsDevice, int width, int height)
            : base(graphicsDevice)
        {
            _width = width;
            _height = height;
        }

        protected override DepthStencilBufferImpl CreateResource()
        {
            return new DepthStencilBufferImpl(GraphicsDevice, _width, _height);
        }

        protected override void ResetResource(DepthStencilBufferImpl resource)
        {

        }
    }
}
