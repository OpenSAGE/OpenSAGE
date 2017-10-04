using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using LLGfx.Effects;
using OpenSage.Graphics;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Terrain
{
    public sealed class TerrainPatchComponent : RenderableComponent
    {
        private readonly StaticBuffer<TerrainVertex> _vertexBuffer;
        private readonly StaticBuffer<ushort> _indexBuffer;

        private readonly TerrainEffect _terrainEffect;
        private readonly EffectPipelineStateHandle _pipelineStateHandle;

        public Int32Rect Bounds { get; }

        internal override BoundingBox LocalBoundingBox { get; }

        public override BoundingBox BoundingBox { get; }

        public Triangle[] Triangles { get; }

        internal TerrainPatchComponent(
            TerrainEffect terrainEffect,
            EffectPipelineStateHandle pipelineStateHandle,
            Int32Rect patchBounds,
            StaticBuffer<TerrainVertex> vertexBuffer,
            StaticBuffer<ushort> indexBuffer,
            Triangle[] triangles,
            BoundingBox boundingBox)
        {
            _terrainEffect = terrainEffect;
            _pipelineStateHandle = pipelineStateHandle;

            Bounds = patchBounds;

            _vertexBuffer = vertexBuffer;
            _indexBuffer = indexBuffer;

            LocalBoundingBox = boundingBox;
            BoundingBox = boundingBox;
            Triangles = triangles;
        }

        internal void Intersect(
            Ray ray,
            ref float? closestIntersection)
        {
            if (ray.Intersects(BoundingBox) == null)
            {
                return;
            }

            for (var i = 0; i < Triangles.Length; i++)
            {
                if (ray.Intersects(ref Triangles[i], out var intersection))
                {
                    if (closestIntersection != null)
                    {
                        if (intersection < closestIntersection)
                        {
                            closestIntersection = intersection;
                        }
                    }
                    else
                    {
                        closestIntersection = intersection;
                    }
                }
            }
        }

        internal override void BuildRenderList(RenderList renderList)
        {
            renderList.AddRenderItem(new RenderItem(
                this,
                _terrainEffect,
                _pipelineStateHandle,
                (commandEncoder, effect, pipelineStateHandle, instanceData) =>
                {
                    effect.Apply(commandEncoder);

                    commandEncoder.SetVertexBuffer(0, _vertexBuffer);

                    commandEncoder.DrawIndexed(
                        PrimitiveType.TriangleList,
                        _indexBuffer.ElementCount,
                        _indexBuffer,
                        0);
                }));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TerrainVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;
    }
}
