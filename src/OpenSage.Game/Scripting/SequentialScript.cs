namespace OpenSage.Scripting
{
    internal sealed class SequentialScript
    {
        public uint Unknown1;
        public uint TeamID;
        public string ScriptName;
        public uint ScriptActionIndex;
        public uint LoopsRemaining;
        public int Unknown2 = -1;

        public void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            reader.ReadUInt32(ref Unknown1);
            reader.ReadUInt32(ref TeamID);
            reader.ReadAsciiString(ref ScriptName);
            reader.ReadUInt32(ref ScriptActionIndex);
            reader.ReadUInt32(ref LoopsRemaining);

            reader.ReadInt32(ref Unknown2);
            if (Unknown2 != -1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(1);
        }
    }
}
