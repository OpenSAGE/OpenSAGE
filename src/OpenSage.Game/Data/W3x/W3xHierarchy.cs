using System.IO;
using OpenSage.Data.StreamFS;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.W3x
{
    public sealed class W3xHierarchy
    {
        public W3xPivot[] Pivots { get; private set; }

        internal static W3xHierarchy Parse(BinaryReader reader, AssetEntry header)
        {
            return new W3xHierarchy
            {
                Pivots = reader.ReadArrayAtOffset(() => W3xPivot.Parse(reader, header))
            };
        }
    }
}
