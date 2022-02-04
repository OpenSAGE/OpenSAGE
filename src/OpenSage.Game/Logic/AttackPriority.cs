using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AttackPriority : IPersistableObject
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

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistAsciiString(ref _name);

            reader.PersistInt32(ref _default);
            if (_default != 1)
            {
                throw new InvalidStateException();
            }

            reader.PersistList(
                Targets,
                static (StatePersister persister, ref AttackPriorityTarget item) =>
                {
                    item ??= new AttackPriorityTarget();
                    persister.PersistObjectValue(item);
                });
        }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class AttackPriorityTarget : IPersistableObject
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

        public void Persist(StatePersister reader)
        {
            reader.PersistAsciiString(ref _target);
            reader.PersistUInt32(ref _value);
        }
    }
}
