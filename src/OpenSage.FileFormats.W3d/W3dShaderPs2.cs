using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dShaderPs2(byte[] Unknown)
{
    internal static W3dShaderPs2 Parse(BinaryReader reader)
    {
        var unknown = reader.ReadBytes(12);

        return new W3dShaderPs2(unknown);
    }

    internal void WriteTo(BinaryWriter writer)
    {
        writer.Write(Unknown);
    }
}
