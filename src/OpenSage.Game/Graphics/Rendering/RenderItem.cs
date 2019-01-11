using System.Numerics;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    [System.Flags]
    public enum CullFlags
    {
        None = 0,
        AlwaysVisible = 0x1
    }

    public readonly struct RenderItem : System.IComparable<RenderItem>
    {
        public readonly Effect Effect;

        public readonly EffectMaterial Material;

        public readonly DeviceBuffer VertexBuffer0;
        public readonly DeviceBuffer VertexBuffer1;

        public readonly CullFlags CullFlags;
        public readonly BoundingBox BoundingBox;

        public readonly Matrix4x4 World;

        public readonly ColorRgb? HouseColor;

        public readonly uint StartIndex;
        public readonly uint IndexCount;
        public readonly DeviceBuffer IndexBuffer;

        public readonly uint Key;

        public RenderItem(
            EffectMaterial material,
            DeviceBuffer vertexBuffer0,
            DeviceBuffer vertexBuffer1,
            CullFlags cullFlags,
            in BoundingBox boundingBox,
            in Matrix4x4 world,
            uint startIndex,
            uint indexCount,
            DeviceBuffer indexBuffer,
            in ColorRgb? houseColor = null)
        {
            Effect = material.Effect;
            Material = material;

            VertexBuffer0 = vertexBuffer0;
            VertexBuffer1 = vertexBuffer1;
            CullFlags = cullFlags;
            BoundingBox = boundingBox;
            World = world;
            HouseColor = houseColor;

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
}
