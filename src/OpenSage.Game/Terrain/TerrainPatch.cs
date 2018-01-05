using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Graphics;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Terrain
{
    public sealed class TerrainPatch : ICullable
    {
        private readonly Buffer<TerrainVertex> _vertexBuffer;
        private readonly Buffer<ushort> _indexBuffer;

        private readonly TerrainMaterial _terrainMaterial;

        public Rectangle Bounds { get; }

        public BoundingBox BoundingBox { get; }

        public Triangle[] Triangles { get; }

        bool ICullable.VisibleInHierarchy => true;

        BoundingBox ICullable.BoundingBox => BoundingBox;

        internal TerrainPatch(
            TerrainMaterial terrainMaterial,
            Rectangle patchBounds,
            Buffer<TerrainVertex> vertexBuffer,
            Buffer<ushort> indexBuffer,
            Triangle[] triangles,
            BoundingBox boundingBox)
        {
            _terrainMaterial = terrainMaterial;

            Bounds = patchBounds;

            _vertexBuffer = vertexBuffer;
            _indexBuffer = indexBuffer;

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

        internal void BuildRenderList(RenderList renderList)
        {
            renderList.Opaque.AddRenderItemDrawIndexed(
                _terrainMaterial,
                _vertexBuffer,
                null,
                CullFlags.None,
                this,
                Matrix4x4.Identity,
                0,
                _indexBuffer.ElementCount,
                _indexBuffer);
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
                new VertexAttributeDescription("POSITION", 0, VertexFormat.Float3, 0, 0),
                new VertexAttributeDescription("NORMAL", 0, VertexFormat.Float3, 12, 0),
                new VertexAttributeDescription("TEXCOORD", 0, VertexFormat.Float2, 24, 0)
            },
            new[]
            {
                new VertexLayoutDescription(InputClassification.PerVertexData, 32)
            });
    }
}
