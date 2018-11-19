using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class HitReactionBehaviorData : BehaviorModuleData
    {
        internal static HitReactionBehaviorData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<HitReactionBehaviorData> FieldParseTable = new IniParseTable<HitReactionBehaviorData>
        {
            { "HitReactionLifeTimer1", (parser, x) => x.HitReactionLifeTimer1 = parser.ParseInteger() },
            { "HitReactionLifeTimer2", (parser, x) => x.HitReactionLifeTimer2 = parser.ParseInteger() },
            { "HitReactionLifeTimer3", (parser, x) => x.HitReactionLifeTimer3 = parser.ParseInteger() },
            { "HitReactionThreshold1", (parser, x) => x.HitReactionThreshold1 = parser.ParseFloat() },
            { "HitReactionThreshold2", (parser, x) => x.HitReactionThreshold2 = parser.ParseFloat() },
            { "HitReactionThreshold3", (parser, x) => x.HitReactionThreshold3 = parser.ParseFloat() },
        };

        public int HitReactionLifeTimer1 { get; private set; }
        public int HitReactionLifeTimer2 { get; private set; }
        public int HitReactionLifeTimer3 { get; private set; }

        public float HitReactionThreshold1 { get; private set; }
        public float HitReactionThreshold2 { get; private set; }
        public float HitReactionThreshold3 { get; private set; }     
    }
}
