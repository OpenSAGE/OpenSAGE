using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dBox
    {
        public uint Version { get; private set; }

        public W3dBoxType BoxType { get; private set; }

        public W3dBoxCollisionTypes CollisionTypes { get; private set; }

        public string Name { get; private set; }

        public W3dRgb Color { get; private set; }

        public W3dVector Center { get; private set; }

        public W3dVector Extent { get; private set; }

        public static W3dBox Parse(BinaryReader reader)
        {
            var result = new W3dBox
            {
                Version = reader.ReadUInt32()
            };

            var flags = reader.ReadUInt32();

            result.BoxType = (W3dBoxType) (flags & 0b11);
            result.CollisionTypes = (W3dBoxCollisionTypes) (flags & 0xFF0);

            result.Name = reader.ReadFixedLengthString(W3dConstants.NameLength * 2);
            result.Color = W3dRgb.Parse(reader);
            result.Center = W3dVector.Parse(reader);
            result.Extent = W3dVector.Parse(reader);

            return result;
        }
    }
}
