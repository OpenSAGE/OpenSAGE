using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <param name="ModelCount"></param>
/// <param name="MaxScreenSize">If model is bigger than this, switch to higher LOD</param>
public sealed record W3dHLodArrayHeader(uint ModelCount, float MaxScreenSize)
    : W3dChunk(W3dChunkType.W3D_CHUNK_HLOD_SUB_OBJECT_ARRAY_HEADER)
{
    internal static W3dHLodArrayHeader Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var modelCount = reader.ReadUInt32();
            var maxScreenSize = reader.ReadSingle();

            return new W3dHLodArrayHeader(modelCount, maxScreenSize);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(ModelCount);
        writer.Write(MaxScreenSize);
    }
}
