using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class BaikonurLaunchPowerBehavior : ObjectBehavior
    {
        internal static BaikonurLaunchPowerBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<BaikonurLaunchPowerBehavior> FieldParseTable = new IniParseTable<BaikonurLaunchPowerBehavior>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "DetonationObject", (parser, x) => x.DetonationObject = parser.ParseAssetReference() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public string DetonationObject { get; private set; }
    }
}
