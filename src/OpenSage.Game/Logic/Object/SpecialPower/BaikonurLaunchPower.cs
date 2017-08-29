using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class BaikonurLaunchPowerModuleData : SpecialPowerModuleData
    {
        internal static BaikonurLaunchPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<BaikonurLaunchPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<BaikonurLaunchPowerModuleData>
            {
                { "DetonationObject", (parser, x) => x.DetonationObject = parser.ParseAssetReference() }
            });

        public string DetonationObject { get; private set; }
    }
}
