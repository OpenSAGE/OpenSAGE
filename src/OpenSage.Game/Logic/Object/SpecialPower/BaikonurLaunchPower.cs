using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class BaikonurLaunchPower : ObjectBehavior
    {
        internal static BaikonurLaunchPower Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BaikonurLaunchPower> FieldParseTable = new IniParseTable<BaikonurLaunchPower>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "DetonationObject", (parser, x) => x.DetonationObject = parser.ParseAssetReference() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public string DetonationObject { get; private set; }
    }
}
