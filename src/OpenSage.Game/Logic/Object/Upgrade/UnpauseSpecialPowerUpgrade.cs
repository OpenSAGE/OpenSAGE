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
                { "ObeyRechageOnTrigger", (parser, x) => x.ObeyRechageOnTrigger = parser.ParseBoolean() },
            });

        public string SpecialPowerTemplate { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ObeyRechageOnTrigger { get; private set; }
    }
}
