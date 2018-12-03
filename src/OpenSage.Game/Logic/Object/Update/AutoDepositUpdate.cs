using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class AutoDepositUpdateModuleData : UpdateModuleData
    {
        internal static AutoDepositUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AutoDepositUpdateModuleData> FieldParseTable = new IniParseTable<AutoDepositUpdateModuleData>
        {
            { "DepositTiming", (parser, x) => x.DepositTiming = parser.ParseInteger() },
            { "DepositAmount", (parser, x) => x.DepositAmount = parser.ParseInteger() },
            { "InitialCaptureBonus", (parser, x) => x.InitialCaptureBonus = parser.ParseInteger() },
            { "ActualMoney", (parser, x) => x.ActualMoney = parser.ParseBoolean() },
            { "UpgradedBoost", (parser, x) => x.UpgradedBoost = BoostUpgrade.Parse(parser) },
            { "Upgrade", (parser, x) => x.Upgrade = parser.ParseAssetReference() },
            { "UpgradeBonusPercent", (parser, x) => x.UpgradeBonusPercent = parser.ParsePercentage() },
            { "UpgradeMustBePresent", (parser, x) => x.UpgradeMustBePresent = ObjectFilter.Parse(parser) }
        };

        /// <summary>
        /// How often, in milliseconds, to give money to the owning player.
        /// </summary>
        public int DepositTiming { get; private set; }

        /// <summary>
        /// Amount of cash to deposit after every <see cref="DepositTiming"/>.
        /// </summary>
        public int DepositAmount { get; private set; }

        /// <summary>
        /// One-time capture bonus.
        /// </summary>
        public int InitialCaptureBonus { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool ActualMoney { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public BoostUpgrade UpgradedBoost { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string Upgrade { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float UpgradeBonusPercent { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectFilter UpgradeMustBePresent { get; private set; }
    }
}
