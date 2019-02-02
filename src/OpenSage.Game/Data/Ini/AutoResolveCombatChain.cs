using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AutoResolveCombatChain
    {
        internal static AutoResolveCombatChain Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AutoResolveCombatChain> FieldParseTable = new IniParseTable<AutoResolveCombatChain>
        {
            { "Target", (parser, x) => x.TargetPriorities.Add(TargetPriority.Parse(parser)) },
        };

        public string Name { get; private set; }

        public List<TargetPriority> TargetPriorities { get; } = new List<TargetPriority>();
    }

    public class TargetPriority
    {
        internal static TargetPriority Parse(IniParser parser)
        {
            return new TargetPriority
            {
                Target = parser.ParseAttributeIdentifier("Target"),
                Priority = parser.ParseAttributeInteger("Priority")
            };
        }

        //Target = Target:AutoResolveUnit_Soldier Priority:50
        public string Target { get; private set; }
        public int Priority { get; private set; }
    }
}
