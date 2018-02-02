using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Effects;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderBucket
    {
        public readonly List<RenderItem> RenderItems;
        public readonly List<RenderItem> CulledItems;

        public RenderBucket()
        {
            RenderItems = new List<RenderItem>();
            CulledItems = new List<RenderItem>();
        }

        public void Clear()
        {
            RenderItems.Clear();
            CulledItems.Clear();
        }

        public void AddRenderItemDraw(
            EffectMaterial material,
            DeviceBuffer vertexBuffer0,
            DeviceBuffer vertexBuffer1,
            CullFlags cullFlags,
            ICullable cullable,
            in Matrix4x4 world,
            uint vertexStart,
            uint vertexCount)
        {
            RenderItems.Add(new RenderItem(
                material,
                vertexBuffer0,
                vertexBuffer1,
                cullFlags,
                cullable,
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
            ICullable cullable,
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
                cullable,
                world,
                DrawCommand.DrawIndexed,
                0, 0,
                startIndex,
                indexCount,
                indexBuffer));
        }
    }

    internal sealed class RenderList
    {
        public readonly RenderBucket Opaque = new RenderBucket();
        public readonly RenderBucket Transparent = new RenderBucket();

        public void Clear()
        {
            Opaque.Clear();
            Transparent.Clear();
        }
    }
}
