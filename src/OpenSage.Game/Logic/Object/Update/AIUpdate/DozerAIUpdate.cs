using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class DozerAIUpdate : AIUpdate
    {
        private readonly DozerAndWorkerState _state = new();

        internal DozerAIUpdate(GameObject gameObject, DozerAIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            _state.Persist(reader);
        }
    }

    internal sealed class DozerAndWorkerState
    {
        private readonly DozerSomething1[] _unknownList1 = new DozerSomething1[3];
        private readonly WorkerAIUpdateStateMachine1 _stateMachine = new();
        private int _unknown2;
        private readonly DozerSomething2[] _unknownList2 = new DozerSomething2[9];
        private int _unknown4;

        public void Persist(StatePersister reader)
        {
            reader.PersistArrayWithUInt32Length("UnknownList1", _unknownList1, static (StatePersister persister, ref DozerSomething1 item) =>
            {
                persister.PersistObjectValue(ref item);
            });

            reader.PersistObject("StateMachine", _stateMachine);
            reader.PersistInt32("Unknown2", ref _unknown2);

            var unknown3 = 3;
            reader.PersistInt32("Unknown3", ref unknown3);
            if (unknown3 != 3)
            {
                throw new InvalidStateException();
            }

            reader.PersistArray("UnknownList2", _unknownList2, static (StatePersister persister, ref DozerSomething2 item) =>
            {
                persister.PersistObjectValue(ref item);
            });

            reader.PersistInt32("Unknown4", ref _unknown4);
        }
    }

    internal struct DozerSomething1 : IPersistableObject
    {
        public uint ObjectId;
        public int Unknown;

        public void Persist(StatePersister persister)
        {
            persister.PersistObjectID("ObjectId", ref ObjectId);
            persister.PersistInt32("Unknown", ref Unknown);
        }
    }

    internal struct DozerSomething2 : IPersistableObject
    {
        public bool UnknownBool;
        public Vector3 UnknownPos;

        public void Persist(StatePersister persister)
        {
            persister.PersistBoolean("UnknownBool", ref UnknownBool);
            persister.PersistVector3("UnknownPos", ref UnknownPos);
        }
    }

    /// <summary>
    /// Allows the use of VoiceRepair, VoiceBuildResponse, VoiceNoBuild and VoiceTaskComplete 
    /// within UnitSpecificSounds section of the object.
    /// Requires Kindof = DOZER.
    /// </summary>
    public sealed class DozerAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static DozerAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<DozerAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<DozerAIUpdateModuleData>
            {
                { "RepairHealthPercentPerSecond", (parser, x) => x.RepairHealthPercentPerSecond = parser.ParsePercentage() },
                { "BoredTime", (parser, x) => x.BoredTime = parser.ParseInteger() },
                { "BoredRange", (parser, x) => x.BoredRange = parser.ParseInteger() },
            });

        public Percentage RepairHealthPercentPerSecond { get; private set; }
        public int BoredTime { get; private set; }
        public int BoredRange { get; private set; }

        internal override AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new DozerAIUpdate(gameObject, this);
        }
    }
}
