namespace OpenSage.Logic.Object.Damage
{
    public struct DamageData
    {
        public DamageDataRequest Request;
        public DamageDataResult Result;

        internal void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            Request.Load(reader);
            Result.Load(reader);
        }
    }

    public struct DamageDataRequest
    {
        // These values are the damage inputs that are to be done to the body.
        public uint ObjectId;
        public ushort Unknown1;
        public DamageType DamageType;
        public DeathType DeathType;
        public float Unknown4;

        internal void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistObjectID(ref ObjectId);
            reader.PersistUInt16(ref Unknown1);
            reader.PersistEnum(ref DamageType);
            reader.PersistEnum(ref DeathType);
            reader.PersistSingle(ref Unknown4);
        }
    }

    public struct DamageDataResult
    {
        // These values are the actual damage that the body calculates for itself.
        public float Unknown1;
        public float Unknown2;

        internal void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistSingle(ref Unknown1);
            reader.PersistSingle(ref Unknown2);

            reader.SkipUnknownBytes(1);
        }
    }
}
