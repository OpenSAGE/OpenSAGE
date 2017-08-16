using System;
using SharpDX.Direct3D12;

namespace LLGfx.Util
{
    internal struct DynamicAllocation
    {
        public Resource Buffer;
        public uint Offset;
        public uint Size;
        public IntPtr CpuAddress;
        public long GpuAddress;
    }
}
