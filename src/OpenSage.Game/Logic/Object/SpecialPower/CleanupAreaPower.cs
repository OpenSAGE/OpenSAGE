using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CleanupAreaPower : ObjectBehavior
    {
        internal static CleanupAreaPower Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CleanupAreaPower> FieldParseTable = new IniParseTable<CleanupAreaPower>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "MaxMoveDistanceFromLocation", (parser, x) => x.MaxMoveDistanceFromLocation = parser.ParseFloat() },
            { "InitiateSound", (parser, x) => x.InitiateSound = parser.ParseAssetReference() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public float MaxMoveDistanceFromLocation { get; private set; }
        public string InitiateSound { get; private set; }
    }
}
