using System.IO;
using System.Numerics;
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

        public Vector3 Center { get; private set; }

        public Vector3 Extent { get; private set; }

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
            result.Center = reader.ReadVector3();
            result.Extent = reader.ReadVector3();

            return result;
        }
    }
}
