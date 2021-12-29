using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AttackPriority
    {
        internal static AttackPriority Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
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

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            Name = reader.ReadAsciiString();

            var unknown = reader.ReadUInt32(); // Probably default value?

            var numTargets = (ushort)Targets.Count;
            reader.ReadUInt16(ref numTargets);

            for (var i = 0; i < numTargets; i++)
            {
                var target = new AttackPriorityTarget();
                target.Load(reader);
                Targets.Add(target);
            }
        }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class AttackPriorityTarget
    {
        internal static AttackPriorityTarget Parse(IniParser parser)
        {
            return new AttackPriorityTarget
            {
                Target = parser.ParseAssetReference(),
                Value = parser.ParseUnsignedInteger()
            };
        }

        public string Target { get; private set; }
        public uint Value { get; private set; }

        internal void Load(SaveFileReader reader)
        {
            Target = reader.ReadAsciiString();
            Value = reader.ReadUInt32();
        }
    }
}
