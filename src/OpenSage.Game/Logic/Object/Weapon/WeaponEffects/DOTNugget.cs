using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public class DOTNugget : DamageNuggetData
    {
        internal static new DOTNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DOTNugget> FieldParseTable = DamageNuggetData.FieldParseTable
            .Concat(new IniParseTable<DOTNugget>
            {
                { "DamageInterval", (parser, x) => x.DamageInterval = parser.ParseInteger() },
                { "DamageDuration", (parser, x) => x.DamageDuration = parser.ParseInteger() },
            });

        public int DamageInterval { get; private set; }
        public int DamageDuration { get; private set; }
    }
}
