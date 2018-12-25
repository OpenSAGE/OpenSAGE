using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class InvisibilitySpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new InvisibilitySpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<InvisibilitySpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<InvisibilitySpecialPowerModuleData>
            {
                { "BroadcastRadius", (parser, x) => x.BroadcastRadius = parser.ParseFloat() },
                { "InvisibilityNugget", (parser, x) => x.InvisibilityNugget = InvisibilityNugget.Parse(parser) },
                { "ObjectFilter", (parser, x) => x.ObjectFilter = ObjectFilter.Parse(parser) },
                { "Duration", (parser, x) => x.Duration = parser.ParseInteger() }
            });

        public float BroadcastRadius { get; private set; }
        public InvisibilityNugget InvisibilityNugget { get; private set; }
        public ObjectFilter ObjectFilter { get; private set; }
        public int Duration { get; private set; }
    }
}
