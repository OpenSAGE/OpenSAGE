namespace OpenSage.Logic.Object.Damage
{
    public struct DamageData
    {
        public DamageDataInput Input;
        public DamageDataOutput Output;

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            Input.Load(reader);
            Output.Load(reader);
        }
    }

    public struct DamageDataInput
    {
        public uint ObjectId;
        public ushort Unknown1;
        public DamageType DamageType;
        public DeathType DeathType;
        public float Unknown4;

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            ObjectId = reader.ReadObjectID();

            Unknown1 = reader.ReadUInt16();

            DamageType = reader.ReadEnum<DamageType>();

            DeathType = reader.ReadEnum<DeathType>();

            Unknown4 = reader.ReadSingle();
        }
    }

    public struct DamageDataOutput
    {
        // These are the damage that is to be applied to the body.
        public float Unknown1;
        public float Unknown2;

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            Unknown1 = reader.ReadSingle();
            Unknown2 = reader.ReadSingle();

            reader.SkipUnknownBytes(1);
        }
    }
}
