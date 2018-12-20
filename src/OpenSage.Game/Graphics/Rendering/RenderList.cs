using System.Numerics;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    public sealed class RenderBucket
    {
        public readonly RenderItemCollection RenderItems;

        public RenderBucket()
        {
            RenderItems = new RenderItemCollection();
        }

        public void Clear()
        {
            RenderItems.Clear();
        }

        public void AddRenderItemDraw(
            EffectMaterial material,
            DeviceBuffer vertexBuffer0,
            DeviceBuffer vertexBuffer1,
            CullFlags cullFlags,
            in BoundingBox boundingBox,
            in Matrix4x4 world,
            uint vertexStart,
            uint vertexCount)
        {
            RenderItems.Add(new RenderItem(
                material,
                vertexBuffer0,
                vertexBuffer1,
                cullFlags,
                boundingBox,
                world,
                DrawCommand.Draw,
                vertexStart,
                vertexCount,
                0, 0, null));
        }

        public void AddRenderItemDrawIndexed(
            EffectMaterial material,
            DeviceBuffer vertexBuffer0,
            DeviceBuffer vertexBuffer1,
            CullFlags cullFlags,
            in BoundingBox boundingBox,
            in Matrix4x4 world,
            uint startIndex,
            uint indexCount,
            DeviceBuffer indexBuffer)
        {
            RenderItems.Add(new RenderItem(
                material,
                vertexBuffer0,
                vertexBuffer1,
                cullFlags,
                boundingBox,
                world,
                DrawCommand.DrawIndexed,
                0, 0,
                startIndex,
                indexCount,
                indexBuffer));
        }
    }

    public sealed class RenderList
    {
        public readonly RenderBucket Opaque = new RenderBucket();
        public readonly RenderBucket Transparent = new RenderBucket();

        public readonly RenderBucket Shadow = new RenderBucket();

        public void Clear()
        {
            Opaque.Clear();
            Transparent.Clear();

            Shadow.Clear();
        }
    }
}
