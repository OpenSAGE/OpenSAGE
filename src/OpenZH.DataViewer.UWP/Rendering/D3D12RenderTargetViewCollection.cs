using System;
using System.Diagnostics;
using OpenZH.DataViewer.Rendering;
using SharpDX.Direct3D12;

namespace OpenZH.DataViewer.UWP.Rendering
{
    public class D3D12RenderTargetViewCollection : IDisposable
    {
        private readonly Device _device;
        private readonly DescriptorHeap _descriptorHeap;
        private readonly int _descriptorSize;
        private readonly D3D12RenderTargetView[] _views;

        public D3D12RenderTargetViewCollection(Device device, int count)
        {
            _device = device;

            var heapDesc = new DescriptorHeapDescription
            {
                DescriptorCount = count,
                Flags = DescriptorHeapFlags.None,
                Type = DescriptorHeapType.RenderTargetView
            };

            _descriptorHeap = device.CreateDescriptorHeap(heapDesc);

            _descriptorSize = device.GetDescriptorHandleIncrementSize(
                DescriptorHeapType.RenderTargetView);

            _views = new D3D12RenderTargetView[count];
        }

        public void Recreate(Resource[] renderTargets)
        {
            Debug.Assert(renderTargets.Length == _views.Length);

            var handle = _descriptorHeap.CPUDescriptorHandleForHeapStart;
            for (var n = 0; n < renderTargets.Length; n++)
            {
                _device.CreateRenderTargetView(renderTargets[n], null, handle);

                _views[n] = new D3D12RenderTargetView(handle);

                handle += _descriptorSize;
            }
        }

        public RenderTargetView Get(int index)
        {
            return _views[index];
        }

        public void Dispose()
        {
            _descriptorHeap.Dispose();
        }
    }
}
