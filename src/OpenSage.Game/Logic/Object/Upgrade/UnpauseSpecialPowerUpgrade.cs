using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class UnpauseSpecialPowerUpgrade : UpgradeModule
    {
        internal UnpauseSpecialPowerUpgrade(UnpauseSpecialPowerUpgradeModuleData moduleData)
            : base(moduleData)
        {
        }

        // TODO
    }

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

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new UnpauseSpecialPowerUpgrade(this);
        }
    }
}
