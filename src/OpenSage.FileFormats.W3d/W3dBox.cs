using System.IO;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dBox(
    uint Version,
    W3dBoxType BoxType,
    W3dBoxCollisionTypes CollisionTypes,
    string Name,
    ColorRgb Color,
    Vector3 Center,
    Vector3 Extent) : W3dChunk(W3dChunkType.W3D_CHUNK_BOX)
{
    internal static W3dBox Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var version = reader.ReadUInt32();

            var flags = reader.ReadUInt32();

            var boxType = (W3dBoxType)(flags & 0b11);
            var collisionTypes = (W3dBoxCollisionTypes)(flags & 0xFF0);

            var name = reader.ReadFixedLengthString(W3dConstants.NameLength * 2);
            var color = reader.ReadColorRgb(true);
            var center = reader.ReadVector3();
            var extent = reader.ReadVector3();

            return new W3dBox(version, boxType, collisionTypes, name, color, center, extent);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Version);

        var flags = (uint)BoxType | (uint)CollisionTypes;
        writer.Write(flags);

        writer.WriteFixedLengthString(Name, W3dConstants.NameLength * 2);
        writer.Write(Color, true);
        writer.Write(Center);
        writer.Write(Extent);
    }
}
