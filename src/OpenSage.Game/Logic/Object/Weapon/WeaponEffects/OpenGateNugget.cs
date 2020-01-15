using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public class OpenGateNugget : WeaponEffectNuggetData
    {
        internal static OpenGateNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<OpenGateNugget> FieldParseTable = WeaponEffectNuggetData.FieldParseTable
            .Concat(new IniParseTable<OpenGateNugget>
            {
                { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
            });

        public int Radius { get; private set; }
    }
}
