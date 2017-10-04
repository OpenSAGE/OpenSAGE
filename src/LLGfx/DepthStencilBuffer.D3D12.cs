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

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, float clearValue)
        {
            _fence = AddDisposable(graphicsDevice.Device.CreateFence(0, FenceFlags.None));
            _nextFenceValue = 1;

            _pool = AddDisposable(new DepthStencilBufferPool(graphicsDevice, width, height, clearValue));
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

        protected override void Dispose(bool disposeManagedResources)
        {
            //Release();

            base.Dispose(disposeManagedResources);
        }
    }

    internal sealed class DepthStencilBufferImpl : GraphicsObject
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly DescriptorTablePoolEntry _descriptorTablePoolEntry;

        internal CpuDescriptorHandle CpuDescriptorHandle => _descriptorTablePoolEntry.CpuDescriptorHandle;

        public DepthStencilBufferImpl(GraphicsDevice graphicsDevice, int width, int height, float clearValue)
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
                new ClearValue
                {
                    Format = format,
                    DepthStencil = new DepthStencilValue { Depth = clearValue }
                }));

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
        private readonly float _clearValue;

        public DepthStencilBufferPool(GraphicsDevice graphicsDevice, int width, int height, float clearValue)
            : base(graphicsDevice)
        {
            _width = width;
            _height = height;
            _clearValue = clearValue;
        }

        protected override DepthStencilBufferImpl CreateResource()
        {
            return new DepthStencilBufferImpl(GraphicsDevice, _width, _height, _clearValue);
        }

        protected override void ResetResource(DepthStencilBufferImpl resource)
        {

        }
    }
}
