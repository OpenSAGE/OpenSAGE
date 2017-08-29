using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CleanupAreaPowerModuleData : SpecialPowerModuleData
    {
        internal static CleanupAreaPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CleanupAreaPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<CleanupAreaPowerModuleData>
            {
                { "MaxMoveDistanceFromLocation", (parser, x) => x.MaxMoveDistanceFromLocation = parser.ParseFloat() },
                { "InitiateSound", (parser, x) => x.InitiateSound = parser.ParseAssetReference() }
            });

        public float MaxMoveDistanceFromLocation { get; private set; }
        public string InitiateSound { get; private set; }
    }
}
