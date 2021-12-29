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
                (x, name) => x._name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AttackPriority> FieldParseTable = new IniParseTable<AttackPriority>
        {
            { "Default", (parser, x) => x._default = parser.ParseInteger() },
            { "Target", (parser, x) => x.Targets.Add(AttackPriorityTarget.Parse(parser)) }
        };

        private string _name;
        public string Name => _name;

        private int _default;
        public int Default => _default;

        public List<AttackPriorityTarget> Targets { get; } = new List<AttackPriorityTarget>();

        internal void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            reader.ReadAsciiString(ref _name);

            reader.ReadInt32(ref _default);
            if (_default != 1)
            {
                throw new InvalidStateException();
            }

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
                _target = parser.ParseAssetReference(),
                _value = parser.ParseUnsignedInteger()
            };
        }

        private string _target;
        public string Target => _target;

        private uint _value;
        public uint Value => _value;

        internal void Load(StatePersister reader)
        {
            reader.ReadAsciiString(ref _target);
            reader.ReadUInt32(ref _value);
        }
    }
}
