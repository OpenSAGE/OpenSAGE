using System.Numerics;
using System.Runtime.InteropServices;
using LL.Graphics3D;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Terrain
{
    public sealed class TerrainPatchComponent : RenderableComponent
    {
        private readonly Buffer<TerrainVertex> _vertexBuffer;
        private readonly Buffer<ushort> _indexBuffer;

        private readonly TerrainMaterial _terrainMaterial;
        private readonly EffectPipelineStateHandle _pipelineStateHandle;

        public Rectangle Bounds { get; }

        internal override BoundingBox LocalBoundingBox { get; }

        public override BoundingBox BoundingBox { get; }

        public Triangle[] Triangles { get; }

        internal TerrainPatchComponent(
            TerrainMaterial terrainMaterial,
            EffectPipelineStateHandle pipelineStateHandle,
            Rectangle patchBounds,
            Buffer<TerrainVertex> vertexBuffer,
            Buffer<ushort> indexBuffer,
            Triangle[] triangles,
            BoundingBox boundingBox)
        {
            _terrainMaterial = terrainMaterial;
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
                _terrainMaterial,
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

        public static readonly VertexDescriptor VertexDescriptor = new VertexDescriptor(
            new[]
            {
                new VertexAttributeDescription(InputClassification.PerVertexData, "POSITION", 0, VertexFormat.Float3, 0, 0),
                new VertexAttributeDescription(InputClassification.PerVertexData, "NORMAL", 0, VertexFormat.Float3, 12, 0),
                new VertexAttributeDescription(InputClassification.PerVertexData, "TEXCOORD", 0, VertexFormat.Float2, 24, 0)
            },
            new[]
            {
                new VertexLayoutDescription(32)
            });
    }
}
