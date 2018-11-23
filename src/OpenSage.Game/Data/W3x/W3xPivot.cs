using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3x
{
    public sealed class W3xPivot
    {
        public string Name { get; private set; }
        public uint? NameID { get; private set; }
        public int ParentIdx { get; private set; }
        public Vector3 Translation { get; private set; }
        public Quaternion Rotation { get; private set; }

        public static W3xPivot Parse(BinaryReader reader, SageGame game)
        {
            string name = null;
            uint? nameID = null;
            switch (game)
            {
                case SageGame.Cnc3:
                case SageGame.Cnc3KanesWrath:
                    name = reader.ReadUInt32PrefixedAsciiStringAtOffset();
                    break;

                case SageGame.Ra3:
                case SageGame.Ra3Uprising:
                case SageGame.Cnc4:
                    nameID = reader.ReadUInt32();
                    break;

                default:
                    throw new InvalidDataException();
            }

            var result = new W3xPivot
            {
                Name = name,
                NameID = nameID,
                ParentIdx = reader.ReadInt32(),
                Translation = reader.ReadVector3(),
                Rotation = reader.ReadQuaternion()
            };

            // Don't need fixup matrix
            reader.BaseStream.Seek(64, SeekOrigin.Current);

            return result;
        }
    }
}
