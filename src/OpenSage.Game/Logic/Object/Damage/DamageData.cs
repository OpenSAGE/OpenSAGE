namespace OpenSage.Logic.Object.Damage
{
    public struct DamageData : IPersistableObject
    {
        public DamageDataRequest Request;
        public DamageDataResult Result;

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistObject(ref Request);
            reader.PersistObject(ref Result);
        }
    }

    public struct DamageDataRequest : IPersistableObject
    {
        // These values are the damage inputs that are to be done to the body.
        public uint ObjectId;
        public ushort Unknown1;
        public DamageType DamageType;
        public DeathType DeathType;
        public float Unknown4;

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistObjectID(ref ObjectId);
            reader.PersistUInt16(ref Unknown1);
            reader.PersistEnum(ref DamageType);
            reader.PersistEnum(ref DeathType);
            reader.PersistSingle(ref Unknown4);
        }
    }

    public struct DamageDataResult : IPersistableObject
    {
        // These values are the actual damage that the body calculates for itself.
        public float Unknown1;
        public float Unknown2;

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistSingle(ref Unknown1);
            reader.PersistSingle(ref Unknown2);

            reader.SkipUnknownBytes(1);
        }
    }
}
