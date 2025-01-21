using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenSage.FileFormats.W3d
{
    public abstract record W3dChunk(W3dChunkType ChunkType, bool HasSubChunks=false)
    {
        internal static T ParseChunk<T>(
            BinaryReader reader,
            W3dParseContext context,
            Func<W3dChunkHeader, T> parseCallback)
            where T : W3dChunk
        {
            var chunkHeader = W3dChunkHeader.Parse(reader);
            var startPosition = reader.BaseStream.Position;
            var endPosition = startPosition + chunkHeader.ChunkSize;

            context.PushChunk(typeof(T).Name, endPosition);

            var result = parseCallback(chunkHeader);

            result.StartPosition = startPosition;
            result.EndPosition = endPosition;

            context.PopAsset();

            if (reader.BaseStream.Position != endPosition)
            {
                throw new InvalidDataException($"Error while parsing asset '{typeof(T).Name}'. Expected reader to be at position {endPosition}, but was at {reader.BaseStream.Position}.");
            }

            return result;
        }

        public virtual IEnumerable<W3dChunk> GetSubChunks()
        {
            return Enumerable.Empty<W3dChunk>();
        }

        // For debugging.
        internal long StartPosition { get; private set; }
        internal long EndPosition { get; private set; }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write((uint) ChunkType);

            var headerPosition = writer.BaseStream.Position;

            // We'll back up and overwrite this later.
            writer.Seek(W3dChunkHeader.SizeInBytes, SeekOrigin.Current);

            var startPosition = writer.BaseStream.Position;

            WriteToOverride(writer);

            var endPosition = writer.BaseStream.Position;

            var dataSize = endPosition - startPosition;

            // Back up and write header.
            var header = new W3dChunkHeader((uint) dataSize, HasSubChunks);

            writer.BaseStream.Position = headerPosition;
            header.WriteTo(writer);
            writer.BaseStream.Position = endPosition;
        }

        protected abstract void WriteToOverride(BinaryWriter writer);
    }

    public abstract record W3dContainerChunk(W3dChunkType ChunkType) : W3dChunk(ChunkType, true)
    {
        public sealed override IEnumerable<W3dChunk> GetSubChunks()
        {
            return GetSubChunksOverride();
        }

        protected abstract IEnumerable<W3dChunk> GetSubChunksOverride();

        protected sealed override void WriteToOverride(BinaryWriter writer)
        {
            foreach (var subChunk in GetSubChunks())
            {
                subChunk.WriteTo(writer);
            }
        }

        internal static void ParseChunks(BinaryReader reader, long endPosition, Action<W3dChunkType> parseCallback)
        {
            while (reader.BaseStream.Position < endPosition)
            {
                var chunkType = reader.ReadUInt32AsEnum<W3dChunkType>();
                parseCallback(chunkType);
            }
        }

        internal static Exception CreateUnknownChunkException(W3dChunkType chunkType)
        {
            return new InvalidDataException($"Unrecognised chunk: {chunkType}");
        }
    }

    public abstract record W3dListChunk<TList, TItem>(W3dChunkType ChunkType) : W3dChunk(ChunkType)
        where TList : W3dListChunk<TList, TItem>, new()
    {
        public List<TItem> Items { get; } = new List<TItem>();

        internal static TList ParseList(BinaryReader reader, W3dParseContext context, Func<BinaryReader, TItem> parseItem)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new TList();
                while (reader.BaseStream.Position < context.CurrentEndPosition)
                {
                    result.Items.Add(parseItem(reader));
                }
                return result;
            });
        }

        protected sealed override void WriteToOverride(BinaryWriter writer)
        {
            foreach (var item in Items)
            {
                WriteItem(writer, item);
            }
        }

        protected abstract void WriteItem(BinaryWriter writer, TItem item);
    }

    public abstract record W3dStructListChunk<TList, TItem>(TItem[] Items, W3dChunkType ChunkType) : W3dChunk(ChunkType)
        where TList : W3dStructListChunk<TList, TItem>, new()
        where TItem : unmanaged
    {
        internal static unsafe TList ParseList(BinaryReader reader, W3dParseContext context, Func<BinaryReader, TItem> parseItem)
        {
            var itemSize = sizeof(TItem);

            return ParseChunk(reader, context, header =>
            {
                var itemCount = header.ChunkSize / itemSize;

                var result = new TList
                {
                    Items = new TItem[itemCount]
                };

                for (var i = 0; i < itemCount; i++)
                {
                    result.Items[i] = parseItem(reader);
                }

                return result;
            });
        }

        protected sealed override void WriteToOverride(BinaryWriter writer)
        {
            for (var i = 0; i < Items.Length; i++)
            {
                WriteItem(writer, Items[i]);
            }
        }

        protected abstract void WriteItem(BinaryWriter writer, in TItem item);
    }

    public abstract record W3dListContainerChunk<TList, TItem>(List<TItem> Items, W3dChunkType ItemType, W3dChunkType ChunkType) : W3dContainerChunk(ChunkType)
        where TList : W3dListContainerChunk<TList, TItem>, new()
        where TItem : W3dChunk
    {
        internal static TList ParseList(BinaryReader reader, W3dParseContext context, Func<BinaryReader, W3dParseContext, TItem> parseItem)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new TList();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    if (chunkType != result.ItemType)
                    {
                        throw CreateUnknownChunkException(chunkType);
                    }

                    result.Items.Add(parseItem(reader, context));
                });

                return result;
            });
        }

        protected sealed override IEnumerable<W3dChunk> GetSubChunksOverride()
        {
            return Items;
        }
    }
}
