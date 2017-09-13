using System.Collections.Generic;
using LLGfx;

namespace OpenSage.Terrain.Util
{
    public sealed class TerrainPatchIndexBufferCache : GraphicsObject
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<TerrainPatchSize, Buffer> _cachedIndexBuffers;

        public TerrainPatchIndexBufferCache(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _cachedIndexBuffers = new Dictionary<TerrainPatchSize, Buffer>();
        }

        public Buffer GetIndexBuffer(
            int width, 
            int height, 
            ResourceUploadBatch uploadBatch)
        {
            var size = new TerrainPatchSize
            {
                Width = width,
                Height = height
            };
            if (!_cachedIndexBuffers.TryGetValue(size, out var result))
            {
                _cachedIndexBuffers[size] = result = AddDisposable(CreateIndexBuffer(uploadBatch, size));
            }
            return result;
        }

        public uint CalculateNumIndices(int width, int height) => (uint) ((width - 1) * (height - 1) * 6);

        private StaticBuffer CreateIndexBuffer(
            ResourceUploadBatch uploadBatch,
            TerrainPatchSize size)
        {
            // TODO: Could use triangle strip

            var numIndices = CalculateNumIndices(size.Width, size.Height);

            var indices = new ushort[numIndices];

            for (int y = 0, indexIndex = 0; y < size.Height - 1; y++)
            {
                var yThis = y * size.Width;
                var yNext = (y + 1) * size.Width;

                for (var x = 0; x < size.Width - 1; x++)
                {
                    // Triangle 1
                    indices[indexIndex++] = (ushort) (yThis + x);
                    indices[indexIndex++] = (ushort) (yThis + x + 1);
                    indices[indexIndex++] = (ushort) (yNext + x);

                    // Triangle 2
                    indices[indexIndex++] = (ushort) (yNext + x);
                    indices[indexIndex++] = (ushort) (yThis + x + 1);
                    indices[indexIndex++] = (ushort) (yNext + x + 1);
                }
            }

            return StaticBuffer.Create(
                _graphicsDevice,
                uploadBatch,
                indices,
                false);
        }

        private struct TerrainPatchSize
        {
            public int Width;
            public int Height;
        }
    }
}
