using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dAggregateSubObject
    {
        public string BoneName;
        public string SubObjectName;

        internal static W3dAggregateSubObject Parse(BinaryReader reader)
        {
            return new W3dAggregateSubObject
            {
                BoneName = reader.ReadFixedLengthString(W3dConstants.NameLength * 2),
                SubObjectName = reader.ReadFixedLengthString(W3dConstants.NameLength * 2)
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.WriteFixedLengthString(BoneName, W3dConstants.NameLength * 2);
            writer.WriteFixedLengthString(SubObjectName, W3dConstants.NameLength * 2);
        }
    }
}
