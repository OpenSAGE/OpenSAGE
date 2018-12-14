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

                var startPosition = reader.BaseStream.Position;

                loadedSize += currentChunk.ChunkSize;

                parseCallback(result, currentChunk);

                var endPosition = startPosition + currentChunk.ChunkSize;

                if (reader.BaseStream.Position != endPosition)
                {
                    throw new InvalidDataException($"Error while parsing asset '{typeof(T).Name}'. Expected reader to be at position {endPosition}, but was at {reader.BaseStream.Position}.");
                }
            } while (loadedSize < chunkSize);

            return result;
        }

        protected static Exception CreateUnknownChunkException(W3dChunkHeader header)
        {
            return new InvalidDataException($"Unrecognised chunk: {header.ChunkType}");
        }

        protected void WriteChunkTo(BinaryWriter writer, W3dChunkType type, bool hasSubChunks, Action writeCallback)
        {
            var headerPosition = writer.BaseStream.Position;

            // We'll back up and overwrite this later.
            writer.Seek(W3dChunkHeader.SizeInBytes, SeekOrigin.Current);

            var startPosition = writer.BaseStream.Position;

            writeCallback();

            var endPosition = writer.BaseStream.Position;

            var dataSize = endPosition - startPosition;

            // Back up and write header.
            var header = new W3dChunkHeader(type, (uint) dataSize, hasSubChunks);

            writer.BaseStream.Position = headerPosition;
            header.WriteTo(writer);
            writer.BaseStream.Position = endPosition;
        }
    }
}
