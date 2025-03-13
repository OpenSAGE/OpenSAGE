using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dHModelAuxData(
    uint Attributes,
    uint MeshCount,
    uint CollisionCount,
    uint SkinCount,
    uint[] FutureCounts,
    float LodMin,
    float LodMax,
    byte[] FutureUse) : W3dChunk(W3dChunkType.OBSOLETE_W3D_CHUNK_HMODEL_AUX_DATA)
{
    internal static W3dHModelAuxData Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var attributes = reader.ReadUInt32();
            var meshCount = reader.ReadUInt32();
            var collisionCount = reader.ReadUInt32();
            var skinCount = reader.ReadUInt32();
            var futureCounts = new uint[8];

            for (var i = 0; i < 8; i++)
            {
                futureCounts[i] = reader.ReadUInt32();
            }

            var lodMin = reader.ReadSingle();
            var lodMax = reader.ReadSingle();

            var futureUse = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            // TODO: Determine If FutureUse are used anywhere or are different between games?

            return new W3dHModelAuxData(attributes, meshCount, collisionCount, skinCount, futureCounts, lodMin, lodMax,
                futureUse);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Attributes);
        writer.Write(MeshCount);
        writer.Write(CollisionCount);
        writer.Write(SkinCount);

        for (var i = 0; i < FutureCounts.Length; i++)
        {
            writer.Write(FutureCounts[i]);
        }

        writer.Write(LodMin);
        writer.Write(LodMax);
        writer.Write(LodMax);
        writer.Write(FutureUse);
    }
}
