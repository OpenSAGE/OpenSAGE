using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.LivingWorld.AutoResolve
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AutoResolveReinforcementSchedule : BaseAsset
    {
        internal static AutoResolveReinforcementSchedule Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("AutoResolveReinforcementSchedule", name),
                FieldParseTable,
                new IniArbitraryFieldParserProvider<AutoResolveReinforcementSchedule>(
                    (x, id) => x.ArmyAvailableAt.Add(parser.ParseInteger())));
        }

        private static readonly IniParseTable<AutoResolveReinforcementSchedule> FieldParseTable = new IniParseTable<AutoResolveReinforcementSchedule>
        {
            { "EachRemaining", (parser, x) => x.EachRemaining = parser.ParseInteger() },
        };

        public int EachRemaining { get; private set; }

        //defines in which round an army is available * 50,
        // so if it is available at round 3 the value is 150
        List<int> ArmyAvailableAt { get; } = new List<int>();
    }
}
