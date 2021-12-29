namespace OpenSage.Logic.Object.Damage
{
    public struct DamageData
    {
        public DamageDataRequest Request;
        public DamageDataResult Result;

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

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

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            reader.ReadObjectID(ref ObjectId);
            reader.ReadUInt16(ref Unknown1);
            reader.ReadEnum(ref DamageType);
            reader.ReadEnum(ref DeathType);
            reader.ReadSingle(ref Unknown4);
        }
    }

    public struct DamageDataResult
    {
        // These values are the actual damage that the body calculates for itself.
        public float Unknown1;
        public float Unknown2;

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            reader.ReadSingle(ref Unknown1);
            reader.ReadSingle(ref Unknown2);

            reader.SkipUnknownBytes(1);
        }
    }
}
