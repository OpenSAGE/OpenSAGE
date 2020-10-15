using System.IO;
using System.Numerics;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.Bfme)]
    public sealed class CastleTemplate
    {
        public string Name { get; private set; }
        public string TemplateName { get; private set; }
        public Vector3 Offset { get; private set; }
        public float Angle { get; private set; }

        public uint Priority { get; private set; }
        public uint Phase { get; private set; }

        internal static CastleTemplate Parse(BinaryReader reader, ushort version)
        {
            var result = new CastleTemplate
            {
                Name = reader.ReadUInt16PrefixedAsciiString(),
                TemplateName = reader.ReadUInt16PrefixedAsciiString(),
                Offset = reader.ReadVector3(),
                Angle = reader.ReadSingle()
            };

            if (version >= 4)
            {
                result.Priority = reader.ReadUInt32();
                result.Phase = reader.ReadUInt32();
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer, ushort version)
        {
            writer.WriteUInt16PrefixedAsciiString(Name);
            writer.WriteUInt16PrefixedAsciiString(TemplateName);
            writer.Write(Offset);
            writer.Write(Angle);

            if (version >= 4)
            {
                writer.Write(Priority);
                writer.Write(Phase);
            }
        }
    }
}
