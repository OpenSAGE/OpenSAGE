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

        public void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            Unknown1 = reader.ReadUInt32();

            TeamID = reader.ReadUInt32();
            ScriptName = reader.ReadAsciiString();
            ScriptActionIndex = reader.ReadUInt32();
            LoopsRemaining = reader.ReadUInt32();

            Unknown2 = reader.ReadInt32();
            if (Unknown2 != -1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(1);
        }
    }
}
