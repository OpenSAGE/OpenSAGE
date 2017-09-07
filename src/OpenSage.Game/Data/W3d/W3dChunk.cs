using System;
using System.IO;

namespace OpenSage.Data.W3d
{
    public abstract class W3dChunk
    {
        protected static T ParseChunk<T>(
            BinaryReader reader, 
            uint chunkSize,
            Action<T, W3dChunkHeader> parseCallback)
            where T : W3dChunk, new()
        {
            var result = new T();

            uint loadedSize = 0;

            do
            {
                loadedSize += W3dChunkHeader.SizeInBytes;
                var currentChunk = W3dChunkHeader.Parse(reader);

                var currentPosition = reader.BaseStream.Position;

                loadedSize += currentChunk.ChunkSize;

                parseCallback(result, currentChunk);

                if (reader.BaseStream.Position != currentPosition + currentChunk.ChunkSize)
                {
                    throw new InvalidDataException();
                }
            } while (loadedSize < chunkSize);

            return result;
        }
    }
}
