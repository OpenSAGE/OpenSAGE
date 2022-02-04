using System;
using System.Numerics;
using OpenSage.Mathematics;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    internal readonly struct RenderItem : IComparable<RenderItem>
    {
        public readonly string DebugName;

        public readonly Material Material;
        public readonly AxisAlignedBoundingBox BoundingBox;
        public readonly Matrix4x4 World;
        public readonly BeforeRenderDelegate BeforeRenderCallback;
        public readonly uint StartIndex;
        public readonly uint IndexCount;
        public readonly DeviceBuffer IndexBuffer;

        public readonly int Key;

        public RenderItem(
            string debugName,
            Material material,
            in AxisAlignedBoundingBox boundingBox,
            in Matrix4x4 world,
            uint startIndex,
            uint indexCount,
            DeviceBuffer indexBuffer,
            BeforeRenderDelegate beforeRenderCallback)
        {
            DebugName = debugName;
            Material = material;
            BoundingBox = boundingBox;
            World = world;
            StartIndex = startIndex;
            IndexCount = indexCount;
            IndexBuffer = indexBuffer;
            BeforeRenderCallback = beforeRenderCallback;

            // Key.
            Key = 0;

            // Bit 24-31: ShaderSet
            Key |= (material.ShaderSet.Id << 24);

            // Bit 16-23: Material
            Key |= (material.Id) << 16;

            // TODO: Vertex buffer?
        }

        int IComparable<RenderItem>.CompareTo(RenderItem other)
        {
            return Key.CompareTo(other.Key);
        }
    }

    internal delegate void BeforeRenderDelegate(CommandList commandList, in RenderItem renderItem);
}
