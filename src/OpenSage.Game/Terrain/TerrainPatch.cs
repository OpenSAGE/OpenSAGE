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

        private readonly DescriptorSet _terrainPatchDescriptorSet;

        public TerrainPatch(
            HeightMap heightMap,
            BlendTileData blendTileData,
            Int32Rect patchBounds,
            GraphicsDevice graphicsDevice,
            ResourceUploadBatch uploadBatch,
            TerrainPatchIndexBufferCache indexBufferCache,
            DescriptorSetLayout terrainPatchDescriptorSetLayout)
        {
            _vertexBuffer = AddDisposable(CreateVertexBuffer(
                graphicsDevice,
                uploadBatch,
                heightMap,
                patchBounds));

            _numIndices = indexBufferCache.CalculateNumIndices(
                patchBounds.Width, 
                patchBounds.Height);

            _indexBuffer = indexBufferCache.GetIndexBuffer(
                patchBounds.Width, 
                patchBounds.Height, 
                uploadBatch);

            _terrainPatchDescriptorSet = AddDisposable(new DescriptorSet(
                graphicsDevice,
                terrainPatchDescriptorSetLayout));

            var textureIDs = new uint[(patchBounds.Width - 1) * (patchBounds.Height - 1) * 2];

            var textureIDIndex = 0;
            for (var y = patchBounds.Y; y < patchBounds.Y + patchBounds.Height - 1; y++)
            {
                for (var x = patchBounds.X; x < patchBounds.X + patchBounds.Width - 1; x++)
                {
                    var tile = blendTileData.Tiles[x, y];
                    var textureIndex = blendTileData.TextureIndices[tile];
                    textureIDs[textureIDIndex++] = (uint) textureIndex.TextureIndex;
                    textureIDs[textureIDIndex++] = (uint) textureIndex.TextureIndex;
                }
            }

            var textureIndicesBuffer = AddDisposable(StaticBuffer.Create(
                graphicsDevice,
                uploadBatch,
                textureIDs,
                false));

            _terrainPatchDescriptorSet.SetTypedBuffer(0, textureIndicesBuffer, PixelFormat.UInt32);
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
                        Normal = heightMap.Normals[x, y],
                        UV = new Vector2( 
                            x - patchBounds.X,
                            y - patchBounds.Y)
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

        public void Draw(CommandEncoder commandEncoder)
        {
            commandEncoder.SetDescriptorSet(2, _terrainPatchDescriptorSet);

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
