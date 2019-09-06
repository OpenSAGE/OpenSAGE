﻿using System.Collections.Generic;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Terrain
{
    internal sealed class TerrainPatchIndexBufferCache : DisposableBase
    {
        private struct CacheEntry
        {
            public DeviceBuffer Buffer;
            public ushort[] Indices;
        }

        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<TerrainPatchSize, CacheEntry> _cachedIndexBuffers;
        public TerrainPatchIndexBufferCache(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _cachedIndexBuffers = new Dictionary<TerrainPatchSize, CacheEntry>();
        }

        public DeviceBuffer GetIndexBuffer(
            int width, 
            int height, 
            out ushort[] indices)
        {
            var size = new TerrainPatchSize
            {
                Width = width,
                Height = height
            };
            if (!_cachedIndexBuffers.TryGetValue(size, out var result))
            {
                _cachedIndexBuffers[size] = result = new CacheEntry
                {
                    Buffer = AddDisposable(CreateIndexBuffer(size, out indices)),
                    Indices = indices
                };
            }
            else
            {
                indices = result.Indices;
            }
            return result.Buffer;
        }

        private uint CalculateNumIndices(int width, int height) => (uint) ((width - 1) * (height - 1) * 6);

        private DeviceBuffer CreateIndexBuffer(
            TerrainPatchSize size,
            out ushort[] indices)
        {
            // TODO: Could use triangle strip

            var numIndices = CalculateNumIndices(size.Width, size.Height);

            indices = new ushort[numIndices];

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

            return _graphicsDevice.CreateStaticBuffer(indices, BufferUsage.IndexBuffer);
        }

        private struct TerrainPatchSize
        {
            public int Width;
            public int Height;
        }
    }
}
