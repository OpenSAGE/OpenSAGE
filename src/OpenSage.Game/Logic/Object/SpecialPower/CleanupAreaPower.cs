using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class CleanupAreaPower : SpecialPowerModule
    {
        internal CleanupAreaPower(GameObject gameObject, GameContext context, CleanupAreaPowerModuleData moduleData) : base(gameObject, context, moduleData)
        {
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            // no idea what these are for
            byte unknown1 = 0;
            reader.PersistByte(ref unknown1);
            byte unknown2 = 0;
            reader.PersistByte(ref unknown2);
            byte unknown3 = 0;
            reader.PersistByte(ref unknown3);

            reader.SkipUnknownBytes(14);
        }
    }

    public sealed class CleanupAreaPowerModuleData : SpecialPowerModuleData
    {
        internal static new CleanupAreaPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CleanupAreaPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<CleanupAreaPowerModuleData>
            {
                { "MaxMoveDistanceFromLocation", (parser, x) => x.MaxMoveDistanceFromLocation = parser.ParseFloat() }
            });

        public float MaxMoveDistanceFromLocation { get; private set; }

        internal override CleanupAreaPower CreateModule(GameObject gameObject, GameContext context)
        {
            return new CleanupAreaPower(gameObject, context, this);
        }
    }
}
