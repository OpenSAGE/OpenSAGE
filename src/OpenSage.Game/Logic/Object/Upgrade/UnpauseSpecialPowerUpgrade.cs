using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class UnpauseSpecialPowerUpgradeModuleData : UpgradeModuleData
    {
        internal static UnpauseSpecialPowerUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<UnpauseSpecialPowerUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<UnpauseSpecialPowerUpgradeModuleData>
            {
                { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            });

        public string SpecialPowerTemplate { get; private set; }
    }
}
