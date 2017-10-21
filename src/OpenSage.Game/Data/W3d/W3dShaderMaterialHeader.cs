using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dShaderMaterialHeader : W3dChunk
    {
        public byte Number { get; private set; }
        public string TypeName { get; private set; }

        public static W3dShaderMaterialHeader Parse(BinaryReader reader, uint chunkSize)
        {
            var result = new W3dShaderMaterialHeader
            {
                Number = reader.ReadByte(),
                TypeName = reader.ReadFixedLengthString(W3dConstants.NameLength * 2)
            };

            reader.ReadUInt32(); // Reserved

            return result;
        }
    }
}
