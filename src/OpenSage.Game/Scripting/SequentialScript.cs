using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Scripting
{
    internal sealed class SequentialScript
    {
        public uint Unknown1 { get; private set; }
        public uint TeamID { get; private set; }
        public string ScriptName { get; private set; }
        public uint ScriptActionIndex { get; private set; }
        public uint LoopsRemaining { get; private set; }
        public int Unknown2 { get; private set; }
        public byte Unknown3 { get; private set; }

        public void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();

            Unknown1 = reader.ReadUInt32();

            TeamID = reader.ReadUInt32();
            ScriptName = reader.ReadBytePrefixedAsciiString();
            ScriptActionIndex = reader.ReadUInt32();
            LoopsRemaining = reader.ReadUInt32();

            Unknown2 = reader.ReadInt32();
            if (Unknown2 != -1)
            {
                throw new InvalidDataException();
            }

            Unknown3 = reader.ReadByte();
            if (Unknown3 != 0)
            {
                throw new InvalidDataException();
            }
        }
    }
}
