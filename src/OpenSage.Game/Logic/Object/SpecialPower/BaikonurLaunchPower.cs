using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class BaikonurLaunchPowerModuleData : SpecialPowerModuleData
    {
        internal static new BaikonurLaunchPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<BaikonurLaunchPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<BaikonurLaunchPowerModuleData>
            {
                { "DetonationObject", (parser, x) => x.DetonationObject = parser.ParseAssetReference() }
            });

        public string DetonationObject { get; private set; }
    }
}
