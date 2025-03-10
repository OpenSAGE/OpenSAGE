using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenSage.FileFormats.W3d;

public abstract record W3dChunk(W3dChunkType ChunkType)
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

    public virtual bool HasSubChunks { get; } = false;

    public virtual IEnumerable<W3dChunk> GetSubChunks()
    {
        return Enumerable.Empty<W3dChunk>();
    }

    // For debugging.
    internal long StartPosition { get; private set; }
    internal long EndPosition { get; private set; }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write((uint)ChunkType);

        var headerPosition = writer.BaseStream.Position;

        // We'll back up and overwrite this later.
        writer.Seek(W3dChunkHeader.SizeInBytes, SeekOrigin.Current);

        var startPosition = writer.BaseStream.Position;

        WriteToOverride(writer);

        var endPosition = writer.BaseStream.Position;

        var dataSize = endPosition - startPosition;

        // Back up and write header.
        var header = new W3dChunkHeader((uint)dataSize, HasSubChunks);

        writer.BaseStream.Position = headerPosition;
        header.WriteTo(writer);
        writer.BaseStream.Position = endPosition;
    }

    protected abstract void WriteToOverride(BinaryWriter writer);
}

public abstract record W3dContainerChunk(W3dChunkType ChunkType) : W3dChunk(ChunkType)
{
    public sealed override bool HasSubChunks => true;

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

public abstract record W3dListChunk<TItem>(W3dChunkType ChunkType, IReadOnlyList<TItem> Items) : W3dChunk(ChunkType)
{
    internal static List<TItem> ParseItems(BinaryReader reader, W3dParseContext context, Func<BinaryReader, TItem> parseItem)
    {
        var items = new List<TItem>();
        while (reader.BaseStream.Position < context.CurrentEndPosition)
        {
            items.Add(parseItem(reader));
        }
        return items;
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

public abstract record W3dStructListChunk<TItem>(W3dChunkType ChunkType, TItem[] Items) : W3dChunk(ChunkType)
    where TItem : unmanaged
{
    internal static unsafe TItem[] ParseItems(W3dChunkHeader header, BinaryReader reader, Func<BinaryReader, TItem> parseItem)
    {
        var itemCount = header.ChunkSize / sizeof(TItem);

        var items = new TItem[itemCount];

        for (var i = 0; i < itemCount; i++)
        {
            items[i] = parseItem(reader);
        }

        return items;
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

public abstract record W3dListContainerChunk<TItem>(W3dChunkType ChunkType, IReadOnlyList<TItem> Items)
    : W3dContainerChunk(ChunkType)
    where TItem : W3dChunk
{
    internal static IReadOnlyList<TItem> ParseItems(BinaryReader reader, W3dParseContext context, W3dChunkType itemType,
        Func<BinaryReader, W3dParseContext, TItem> parseItem)
    {
        var items = new List<TItem>();

        ParseChunks(reader, context.CurrentEndPosition, chunkType =>
        {
            if (chunkType != itemType)
            {
                throw CreateUnknownChunkException(chunkType);
            }

            items.Add(parseItem(reader, context));
        });

        return items;
    }

    protected sealed override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        return Items;
    }
}
