using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dVolumeRandomizer(
    uint ClassId,
    float Value1,
    float Value2,
    float Value3)
{
    internal static W3dVolumeRandomizer Parse(BinaryReader reader)
    {
        var classId = reader.ReadUInt32();
        var value1 = reader.ReadSingle();
        var value2 = reader.ReadSingle();
        var value3 = reader.ReadSingle();

        reader.ReadBytes(4 * sizeof(uint)); // Pad

        return new W3dVolumeRandomizer(classId, value1, value2, value3);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write(ClassId);
        writer.Write(Value1);
        writer.Write(Value2);
        writer.Write(Value3);

        for (var i = 0; i < 4; i++)
        {
            writer.Write(0u);
        }
    }
}
