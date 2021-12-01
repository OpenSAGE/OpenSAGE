using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class CashHackSpecialPower : SpecialPowerModule
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    /// <summary>
    /// Allows you to steal money from an enemy supply center. The special power specified in
    /// <see cref="SpecialPowerTemplate"/> must use the <see cref="SpecialPowerType.CashHack"/> type.
    /// </summary>
    public sealed class CashHackSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new CashHackSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CashHackSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<CashHackSpecialPowerModuleData>
            {
                { "UpgradeMoneyAmount", (parser, x) => x.UpgradeMoneyAmounts.Add(CashHackSpecialPowerUpgrade.Parse(parser)) },
                { "MoneyAmount", (parser, x) => x.MoneyAmount = parser.ParseInteger() }
            });

        public List<CashHackSpecialPowerUpgrade> UpgradeMoneyAmounts { get; } = new List<CashHackSpecialPowerUpgrade>();

        /// <summary>
        /// Amount of money to steal.
        /// </summary>
        public int MoneyAmount { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new CashHackSpecialPower();
        }
    }

    public sealed class CashHackSpecialPowerUpgrade
    {
        internal static CashHackSpecialPowerUpgrade Parse(IniParser parser)
        {
            return new CashHackSpecialPowerUpgrade
            {
                Science = parser.ParseAssetReference(),
                MoneyAmount = parser.ParseInteger()
            };
        }

        public string Science { get; private set; }
        public int MoneyAmount { get; private set; }
    }
}
