using System.Numerics;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    [System.Flags]
    internal enum CullFlags
    {
        None = 0,
        AlwaysVisible = 0x1
    }

    internal enum DrawCommand
    {
        Draw,
        DrawIndexed
    }

    //internal readonly struct DrawItem
    //{
    //    public readonly Matrix4x4 World;

    //    public readonly Buffer VertexBuffer0;
    //    public readonly Buffer VertexBuffer1;

    //    public readonly EffectMaterial Material;

    //    public readonly uint IndexCount;
    //    public readonly uint NumInstances;
    //    public readonly Buffer<ushort> IndexBuffer;
    //    public readonly uint StartIndex;
    //}

    internal readonly struct RenderItem : System.IComparable<RenderItem>
    {
        public readonly Effect Effect;

        public readonly EffectMaterial Material;

        public readonly DeviceBuffer VertexBuffer0;
        public readonly DeviceBuffer VertexBuffer1;

        public readonly CullFlags CullFlags;
        public readonly ICullable Cullable;

        public readonly Matrix4x4 World;

        public readonly DrawCommand DrawCommand;

        // Draw
        public readonly uint VertexStart;
        public readonly uint VertexCount;

        // DrawIndexed
        public readonly uint StartIndex;
        public readonly uint IndexCount;
        public readonly DeviceBuffer IndexBuffer;

        public readonly uint Key;

        public RenderItem(
            EffectMaterial material,
            DeviceBuffer vertexBuffer0,
            DeviceBuffer vertexBuffer1,
            CullFlags cullFlags,
            ICullable cullable,
            in Matrix4x4 world,
            DrawCommand drawCommand,

            uint vertexStart,
            uint vertexCount,

            uint startIndex,
            uint indexCount,
            DeviceBuffer indexBuffer)
        {
            Effect = material.Effect;
            Material = material;

            VertexBuffer0 = vertexBuffer0;
            VertexBuffer1 = vertexBuffer1;
            CullFlags = cullFlags;
            Cullable = cullable;
            World = world;
            DrawCommand = drawCommand;

            VertexStart = vertexStart;
            VertexCount = vertexCount;

            StartIndex = startIndex;
            IndexCount = indexCount;
            IndexBuffer = indexBuffer;

            // Key.
            Key = 0;

            // Bit 24-31: Effect
            Key |= (uint) ((material.Effect.ID) << 24);

            // Bit 8-23: Material
            Key |= (uint) ((material.ID) << 8);
        }

        int System.IComparable<RenderItem>.CompareTo(RenderItem other)
        {
            return Key.CompareTo(other.Key);
        }
    }

    internal interface ICullable
    {
        bool VisibleInHierarchy { get; }
        BoundingBox BoundingBox { get; }
    }
}
