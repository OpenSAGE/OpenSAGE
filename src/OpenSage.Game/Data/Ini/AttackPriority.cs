using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AttackPriority
    {
        internal static AttackPriority Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AttackPriority> FieldParseTable = new IniParseTable<AttackPriority>
        {
            { "Default", (parser, x) => x.Default = parser.ParseInteger() },
            { "Target", (parser, x) => x.Targets.Add(AttackPriorityTarget.Parse(parser)) }
        };

        public string Name { get; private set; }

        public int Default { get; private set; }
        public List<AttackPriorityTarget> Targets { get; } = new List<AttackPriorityTarget>();
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class AttackPriorityTarget
    {
        internal static AttackPriorityTarget Parse(IniParser parser)
        {
            return new AttackPriorityTarget
            {
                Target = parser.ParseAssetReference(),
                Value = parser.ParseInteger()
            };
        }

        public string Target { get; private set; }
        public int Value { get; private set; }
    }
}
