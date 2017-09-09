using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHLodHeader
    {
        public uint Version { get; private set; }
        public uint LodCount { get; private set; }
        public string Name { get; private set; }

        /// <summary>
        /// Name of the hierarchy tree to use.
        /// </summary>
        public string HierarchyName { get; private set; }

        public static W3dHLodHeader Parse(BinaryReader reader)
        {
            return new W3dHLodHeader
            {
                Version = reader.ReadUInt32(),
                LodCount = reader.ReadUInt32(),
                Name = reader.ReadFixedLengthString(W3dConstants.NameLength),
                HierarchyName = reader.ReadFixedLengthString(W3dConstants.NameLength)
            };
        }
    }
}
