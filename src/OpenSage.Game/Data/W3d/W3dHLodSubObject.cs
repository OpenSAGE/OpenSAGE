using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHLodSubObject
    {
        public uint BoneIndex { get; private set; }

        public string Name { get; private set; }

        public static W3dHLodSubObject Parse(BinaryReader reader)
        {
            return new W3dHLodSubObject
            {
                BoneIndex = reader.ReadUInt32(),
                Name = reader.ReadFixedLengthString(W3dConstants.NameLength * 2)
            };
        }
    }
}
