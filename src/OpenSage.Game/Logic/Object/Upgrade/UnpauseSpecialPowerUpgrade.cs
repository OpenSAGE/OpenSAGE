using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class UnpauseSpecialPowerUpgrade : ObjectBehavior
    {
        internal static UnpauseSpecialPowerUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<UnpauseSpecialPowerUpgrade> FieldParseTable = new IniParseTable<UnpauseSpecialPowerUpgrade>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public string TriggeredBy { get; private set; }
    }
}
