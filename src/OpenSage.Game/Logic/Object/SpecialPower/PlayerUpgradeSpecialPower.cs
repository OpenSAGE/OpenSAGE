using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class PlayerUpgradeSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new PlayerUpgradeSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<PlayerUpgradeSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<PlayerUpgradeSpecialPowerModuleData>
            {
                { "UpgradeName", (parser, x) => x.UpgradeName = parser.ParseAssetReference() }
            });

        public string UpgradeName { get; private set; }
    }
}
