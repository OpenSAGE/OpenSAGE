using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows you to steal money from an enemy supply center. The special power specified in
    /// <see cref="SpecialPowerTemplate"/> must use the <see cref="SpecialPowerType.CashHack"/> type.
    /// </summary>
    public sealed class CashHackSpecialPower : ObjectBehavior
    {
        internal static CashHackSpecialPower Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CashHackSpecialPower> FieldParseTable = new IniParseTable<CashHackSpecialPower>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "UpgradeMoneyAmount", (parser, x) => x.UpgradeMoneyAmounts.Add(UpgradeMoneyAmount.Parse(parser)) },
            { "MoneyAmount", (parser, x) => x.MoneyAmount = parser.ParseInteger() }
        };

        public string SpecialPowerTemplate { get; private set; }

        public List<UpgradeMoneyAmount> UpgradeMoneyAmounts { get; } = new List<UpgradeMoneyAmount>();

        /// <summary>
        /// Amount of money to steal.
        /// </summary>
        public int MoneyAmount { get; private set; }
    }

    public sealed class UpgradeMoneyAmount
    {
        internal static UpgradeMoneyAmount Parse(IniParser parser)
        {
            return new UpgradeMoneyAmount
            {
                Science = parser.ParseAssetReference(),
                MoneyAmount = parser.ParseInteger()
            };
        }

        public string Science { get; private set; }
        public int MoneyAmount { get; private set; }
    }
}
