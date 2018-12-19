using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dShaderPs2
    {
        public byte[] Unknown { get; private set; }

        internal static W3dShaderPs2 Parse(BinaryReader reader)
        {
            return new W3dShaderPs2
            {
                Unknown = reader.ReadBytes(12)
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(Unknown);
        }
    }
}
