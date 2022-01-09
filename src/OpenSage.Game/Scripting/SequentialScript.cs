namespace OpenSage.Scripting
{
    internal sealed class SequentialScript : IPersistableObject
    {
        public uint Unknown1;
        public uint TeamID;
        public string ScriptName;
        public uint ScriptActionIndex;
        public uint LoopsRemaining;
        public int Unknown2 = -1;

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistUInt32(ref Unknown1);
            reader.PersistUInt32(ref TeamID);
            reader.PersistAsciiString(ref ScriptName);
            reader.PersistUInt32(ref ScriptActionIndex);
            reader.PersistUInt32(ref LoopsRemaining);

            reader.PersistInt32(ref Unknown2);
            if (Unknown2 != -1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(1);
        }
    }
}
