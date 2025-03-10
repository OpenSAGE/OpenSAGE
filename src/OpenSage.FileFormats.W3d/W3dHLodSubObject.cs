using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dHLodSubObject(uint BoneIndex, string Name) : W3dChunk(W3dChunkType.W3D_CHUNK_HLOD_SUB_OBJECT)
{
    internal static W3dHLodSubObject Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var boneIndex = reader.ReadUInt32();
            var name = reader.ReadFixedLengthString(W3dConstants.NameLength * 2);

            return new W3dHLodSubObject(boneIndex, name);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(BoneIndex);
        writer.WriteFixedLengthString(Name, W3dConstants.NameLength * 2);
    }
}
