using System.IO;
using System.Numerics;
using OpenSage.Data.StreamFS;
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

        public static W3xPivot Parse(BinaryReader reader, AssetEntry header)
        {
            string name = null;
            uint? nameID = null;
            switch (header.TypeHash)
            {
                case 1002596986u: // Cnc3
                    name = reader.ReadUInt32PrefixedAsciiStringAtOffset();
                    break;

                case 3985956449u: // Ra3
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
