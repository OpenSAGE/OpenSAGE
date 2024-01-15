using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class DozerAIUpdate : AIUpdate, IBuilderAIUpdate
    {
        private readonly DozerAndWorkerState _state = new();

        private GameObject _buildTarget;

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

        public void SetBuildTarget(GameObject gameObject)
        {
            // note that the order here is important, as SetTargetPoint will clear any existing buildTarget
            // TODO: target should not be directly on the building, but rather a point along the foundation perimeter
            SetTargetPoint(gameObject.Translation);
            _buildTarget = gameObject;
        }

        protected override void ArrivedAtDestination()
        {
            base.ArrivedAtDestination();

            if (_buildTarget is not null)
            {
                _buildTarget.Construct();
                GameObject.ModelConditionFlags.Set(ModelConditionFlag.ActivelyConstructing, true);
            }
        }

        internal override void SetTargetPoint(Vector3 targetPoint)
        {
            base.SetTargetPoint(targetPoint);
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.ActivelyConstructing, false);
            _buildTarget?.PauseConstruction();
            ClearBuildTarget();
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);

            if (_buildTarget is { BuildProgress: >= 1 })
            {
                ClearBuildTarget();
                GameObject.ModelConditionFlags.Set(ModelConditionFlag.ActivelyConstructing, false);
                GameObject.GameContext.AudioSystem.PlayAudioEvent(GameObject, GameObject.Definition.VoiceTaskComplete.Value);
            }
        }

        private void ClearBuildTarget()
        {
            _buildTarget = null;
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
            reader.PersistArrayWithUInt32Length(
                _unknownList1,
                static (StatePersister persister, ref DozerSomething1 item) =>
                {
                    persister.PersistObjectValue(ref item);
                });

            reader.PersistObject(_stateMachine);
            reader.PersistInt32(ref _unknown2);

            var unknown3 = 3;
            reader.PersistInt32(ref unknown3);
            if (unknown3 != 3)
            {
                throw new InvalidStateException();
            }

            reader.PersistArray(
                _unknownList2,
                static (StatePersister persister, ref DozerSomething2 item) =>
                {
                    persister.PersistObjectValue(ref item);
                });

            reader.PersistInt32(ref _unknown4);
        }
    }

    internal struct DozerSomething1 : IPersistableObject
    {
        public uint ObjectId;
        public int Unknown;

        public void Persist(StatePersister persister)
        {
            persister.PersistObjectID(ref ObjectId);
            persister.PersistInt32(ref Unknown);
        }
    }

    internal struct DozerSomething2 : IPersistableObject
    {
        public bool UnknownBool;
        public Vector3 UnknownPos;

        public void Persist(StatePersister persister)
        {
            persister.PersistBoolean(ref UnknownBool);
            persister.PersistVector3(ref UnknownPos);
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

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new DozerAIUpdate(gameObject, this);
        }
    }
}
