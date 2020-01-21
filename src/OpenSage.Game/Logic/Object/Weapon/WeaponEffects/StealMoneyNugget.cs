using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public class StealMoneyNugget : WeaponEffectNugget
    {
        internal static StealMoneyNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<StealMoneyNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<StealMoneyNugget>
            {
                { "AmountStolenPerAttack", (parser, x) => x.AmountStolenPerAttack = parser.ParseInteger() },
            });

        public int AmountStolenPerAttack { get; private set; }
    }
}
