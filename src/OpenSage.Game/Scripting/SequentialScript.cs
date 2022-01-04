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

            reader.PersistUInt32("Unknown1", ref Unknown1);
            reader.PersistUInt32("TeamId", ref TeamID);
            reader.PersistAsciiString("ScriptName", ref ScriptName);
            reader.PersistUInt32("ScriptActionIndex", ref ScriptActionIndex);
            reader.PersistUInt32("LoopsRemaining", ref LoopsRemaining);

            reader.PersistInt32(ref Unknown2);
            if (Unknown2 != -1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(1);
        }
    }
}
