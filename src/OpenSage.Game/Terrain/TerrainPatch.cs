using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Data.Map;
using OpenSage.Mathematics;
using OpenSage.Terrain.Util;

namespace OpenSage.Terrain
{
    public sealed class TerrainPatch : GraphicsObject
    {
        private readonly Buffer _vertexBuffer;

        private readonly uint _numIndices;
        private readonly Buffer _indexBuffer;

        public Int32Rect Bounds { get; }
        public BoundingBox BoundingBox { get; }

        public Triangle[] Triangles { get; }

        public TerrainPatch(
            HeightMap heightMap,
            BlendTileData blendTileData,
            Int32Rect patchBounds,
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            TerrainPatchIndexBufferCache indexBufferCache)
        {
            Bounds = patchBounds;

            _numIndices = indexBufferCache.CalculateNumIndices(
                patchBounds.Width,
                patchBounds.Height);

            _indexBuffer = indexBufferCache.GetIndexBuffer(
                patchBounds.Width,
                patchBounds.Height,
                uploadBatch,
                out var indices);

            _vertexBuffer = AddDisposable(CreateVertexBuffer(
                graphicsDevice,
                uploadBatch,
                heightMap,
                patchBounds,
                indices,
                out var boundingBox,
                out var triangles));
            BoundingBox = boundingBox;
            Triangles = triangles;
        }

        private static StaticBuffer CreateVertexBuffer(
           GraphicsDevice graphicsDevice,
           ResourceUploadBatch uploadBatch,
           HeightMap heightMap,
           Int32Rect patchBounds,
           ushort[] indices,
           out BoundingBox boundingBox,
           out Triangle[] triangles)
        {
            var numVertices = patchBounds.Width * patchBounds.Height;

            var vertices = new TerrainVertex[numVertices];
            var points = new Vector3[numVertices];

            var vertexIndex = 0;
            for (var y = patchBounds.Y; y < patchBounds.Y + patchBounds.Height; y++)
            {
                for (var x = patchBounds.X; x < patchBounds.X + patchBounds.Width; x++)
                {
                    var position = heightMap.GetPosition(x, y);
                    points[vertexIndex] = position;
                    vertices[vertexIndex++] = new TerrainVertex
                    {
                        Position = position,
                        Normal = heightMap.Normals[x, y],
                        UV = new Vector2(x, y)
                    };
                }
            }

            boundingBox = BoundingBox.CreateFromPoints(points);

            triangles = new Triangle[(patchBounds.Width - 1) * (patchBounds.Height) * 2];

            var triangleIndex = 0;
            var indexIndex = 0;
            for (var y = 0; y < patchBounds.Height - 1; y++)
            {
                for (var x = 0; x < patchBounds.Width - 1; x++)
                {
                    // Triangle 1
                    triangles[triangleIndex++] = new Triangle
                    {
                        V0 = points[indices[indexIndex++]],
                        V1 = points[indices[indexIndex++]],
                        V2 = points[indices[indexIndex++]]
                    };

                    // Triangle 2
                    triangles[triangleIndex++] = new Triangle
                    {
                        V0 = points[indices[indexIndex++]],
                        V1 = points[indices[indexIndex++]],
                        V2 = points[indices[indexIndex++]]
                    };
                }
            }

            return StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                vertices,
                false);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TerrainVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 UV;
        }

        public void Intersect(
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

        public void Draw(CommandEncoder commandEncoder)
        {
            commandEncoder.SetVertexBuffer(0, _vertexBuffer);

            commandEncoder.DrawIndexed(
                PrimitiveType.TriangleList,
                _numIndices,
                IndexType.UInt16,
                _indexBuffer,
                0);
        }
    }
}
