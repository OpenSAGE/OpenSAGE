using System.Numerics;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Rendering;
using OpenSage.Utilities.Extensions;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Terrain
{
    public sealed class TerrainPatch : RenderObject
    {
        private readonly DeviceBuffer _vertexBuffer;
        private readonly DeviceBuffer _indexBuffer;
        private readonly uint _numIndices;

        public Rectangle Bounds { get; }

        public override MaterialPass MaterialPass { get; }

        public override string DebugName { get; }

        public override AxisAlignedBoundingBox BoundingBox { get; }

        public Triangle[] Triangles { get; }

        internal TerrainPatch(
            HeightMap heightMap,
            Rectangle patchBounds,
            GraphicsDevice graphicsDevice,
            TerrainPatchIndexBufferCache indexBufferCache,
            Material material)
        {
            DebugName = $"Terrain_{Bounds}";

            Bounds = patchBounds;

            _indexBuffer = indexBufferCache.GetIndexBuffer(
                patchBounds.Width,
                patchBounds.Height,
                out var indices);

            _numIndices = (uint) indices.Length;

            _vertexBuffer = AddDisposable(CreateVertexBuffer(
                graphicsDevice,
                heightMap,
                patchBounds,
                indices,
                out var boundingBox,
                out var triangles));

            BoundingBox = boundingBox;
            Triangles = triangles;

            MaterialPass = new MaterialPass(RenderBucketType.Terrain);
            MaterialPass.Passes["Forward"] = material;
        }

        private static DeviceBuffer CreateVertexBuffer(
           GraphicsDevice graphicsDevice,
           HeightMap heightMap,
           Rectangle patchBounds,
           ushort[] indices,
           out AxisAlignedBoundingBox boundingBox,
           out Triangle[] triangles)
        {
            var numVertices = patchBounds.Width * patchBounds.Height;

            var vertices = new TerrainShaderResources.TerrainVertex[numVertices];
            var points = new Vector3[numVertices];

            var vertexIndex = 0;
            for (var y = patchBounds.Y; y < patchBounds.Y + patchBounds.Height; y++)
            {
                for (var x = patchBounds.X; x < patchBounds.X + patchBounds.Width; x++)
                {
                    var position = heightMap.GetPosition(x, y);
                    points[vertexIndex] = position;
                    vertices[vertexIndex++] = new TerrainShaderResources.TerrainVertex
                    {
                        Position = position,
                        Normal = heightMap.Normals[x, y],
                        UV = new Vector2(x, y)
                    };
                }
            }

            boundingBox = AxisAlignedBoundingBox.CreateFromPoints(points);

            triangles = new Triangle[(patchBounds.Width - 1) * (patchBounds.Height) * 2];

            var triangleIndex = 0;
            var indexIndex = 0;
            for (var y = 0; y < patchBounds.Height - 1; y++)
            {
                for (var x = 0; x < patchBounds.Width - 1; x++)
                {
                    // Triangle 1
                    triangles[triangleIndex++] = new Triangle(
                        points[indices[indexIndex++]],
                        points[indices[indexIndex++]],
                        points[indices[indexIndex++]]);

                    // Triangle 2
                    triangles[triangleIndex++] = new Triangle(
                        points[indices[indexIndex++]],
                        points[indices[indexIndex++]],
                        points[indices[indexIndex++]]);
                }
            }

            return graphicsDevice.CreateStaticBuffer(vertices, BufferUsage.VertexBuffer);
        }

        internal void Intersect(
            Ray ray,
            ref float? closestIntersection)
        {
            if (!ray.Intersects(BoundingBox, out var _))
            {
                return;
            }

            for (var i = 0; i < Triangles.Length; i++)
            {
                if (ray.Intersects(Triangles[i], out var intersection))
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

        public override void Render(CommandList commandList)
        {
            commandList.SetVertexBuffer(0, _vertexBuffer);

            commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);

            commandList.DrawIndexed(_numIndices, 1, 0, 0, 0);
        }
    }
}
