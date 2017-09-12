using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Mathematics;

namespace OpenSage.Terrain
{
    public sealed class TerrainPatch : GraphicsObject
    {
        private readonly Buffer _vertexBuffer;

        private readonly uint _numIndices;
        private readonly Buffer _indexBuffer;

        public TerrainPatch(
            HeightMap heightMap,
            Int32Rect patchBounds,
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch)
        {
            _vertexBuffer = AddDisposable(CreateVertexBuffer(
                graphicsDevice,
                uploadBatch,
                heightMap,
                patchBounds));

            StaticBuffer indexBuffer;
            (indexBuffer, _numIndices) = CreateIndexBuffer(
                graphicsDevice,
                uploadBatch,
                patchBounds);
            _indexBuffer = AddDisposable(indexBuffer);
        }

        private static StaticBuffer CreateVertexBuffer(
           GraphicsDevice graphicsDevice,
           ResourceUploadBatch uploadBatch,
           HeightMap heightMap,
           Int32Rect patchBounds)
        {
            var numVertices = patchBounds.Width * patchBounds.Height;

            var vertices = new TerrainVertex[numVertices];

            var vertexIndex = 0;
            for (var y = patchBounds.Y; y < patchBounds.Y + patchBounds.Height; y++)
            {
                for (var x = patchBounds.X; x < patchBounds.X + patchBounds.Width; x++)
                {
                    vertices[vertexIndex++] = new TerrainVertex
                    {
                        Position = heightMap.GetPosition(x, y),
                        Normal = heightMap.Normals[x, y]
                    };
                }
            }

            return StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                vertices,
                false);
        }

        private static (StaticBuffer buffer, uint numIndices) CreateIndexBuffer(
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            Int32Rect patchBounds)
        {
            // TODO: Could use triangle strip

            // TODO: Split terrain into patches.
            var patchHeight = System.Math.Min(
                patchBounds.Height, 
                ushort.MaxValue / (patchBounds.Width - 1) / 6);

            var numIndices = (uint) ((patchBounds.Width - 1) * (patchHeight - 1) * 6);

            var indices = new ushort[numIndices];

            for (int y = 0, indexIndex = 0; y < patchHeight - 1; y++)
            {
                for (var x = 0; x < patchBounds.Width - 1; x++)
                {
                    // Triangle 1
                    indices[indexIndex++] = (ushort) (y * (patchBounds.Width - 1) + x);
                    indices[indexIndex++] = (ushort) (y * (patchBounds.Width - 1) + x + 1);
                    indices[indexIndex++] = (ushort) ((y + 1) * (patchBounds.Width - 1) + x);

                    // Triangle 2
                    indices[indexIndex++] = (ushort) ((y + 1) * (patchBounds.Width - 1) + x);
                    indices[indexIndex++] = (ushort) (y * (patchBounds.Width - 1) + x + 1);
                    indices[indexIndex++] = (ushort) ((y + 1) * (patchBounds.Width - 1) + x + 1);
                }
            }

            var buffer = StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                indices,
                false);

            return (buffer, numIndices);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TerrainVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
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
