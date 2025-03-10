using System.IO;
using System.Numerics;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dCollectionPlaceholder(
    Vector3 TransformA,
    Vector3 TransformB,
    Vector3 TransformC,
    Vector3 TransformD,
    string Name,
    byte[] UnknownBytes) : W3dChunk(W3dChunkType.W3D_CHUNK_PLACEHOLDER)
{
    internal static W3dCollectionPlaceholder Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var transformA = reader.ReadVector3();
            var transformB = reader.ReadVector3();
            var transformC = reader.ReadVector3();
            var transformD = reader.ReadVector3();
            var name = reader.ReadFixedLengthString(W3dConstants.NameLength);
            var unknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            // TODO: Determine W3dCollectionPlaceholder UnknownBytes

            return new W3dCollectionPlaceholder(transformA, transformB, transformC, transformD, name, unknownBytes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(TransformA);
        writer.Write(TransformB);
        writer.Write(TransformC);
        writer.Write(TransformD);
        writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
        writer.Write(UnknownBytes);
    }
}
