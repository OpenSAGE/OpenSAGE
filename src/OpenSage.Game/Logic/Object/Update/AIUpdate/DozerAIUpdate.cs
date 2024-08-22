#nullable enable

using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class DozerAIUpdate : AIUpdate, IBuilderAIUpdate
    {
        public GameObject? BuildTarget => _state.BuildTarget;
        public GameObject? RepairTarget => _state.RepairTarget;

        private readonly GameContext _context;

        private readonly DozerAndWorkerState _state;

        internal DozerAIUpdate(GameObject gameObject, GameContext context, DozerAIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
            _context = context;
            _state = new DozerAndWorkerState(gameObject, context, moduleData);
        }

        internal override void Stop()
        {
            _state.ClearDozerTasks();
            base.Stop();
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
            _state.SetBuildTarget(gameObject, _context.GameLogic.CurrentFrame.Value);
        }

        public void SetRepairTarget(GameObject gameObject)
        {
            // note that the order here is important, as SetTargetPoint will clear any existing repairTarget
            SetTargetPoint(gameObject.Translation);
            _state.SetRepairTarget(gameObject, _context.GameLogic.CurrentFrame.Value);
        }

        internal override void SetTargetPoint(Vector3 targetPoint)
        {
            _state.ClearDozerTasks();
            base.SetTargetPoint(targetPoint);
        }

        protected override void ArrivedAtDestination()
        {
            _state.ArrivedAtDestination();
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);
            _state.Update(context);
        }
    }

    /// <summary>
    /// Allows the use of VoiceRepair, VoiceBuildResponse, VoiceNoBuild and VoiceTaskComplete
    /// within UnitSpecificSounds section of the object.
    /// Requires Kindof = DOZER.
    /// </summary>
    public sealed class DozerAIUpdateModuleData : AIUpdateModuleData, IBuilderAIUpdateData
    {
        internal new static DozerAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<DozerAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<DozerAIUpdateModuleData>
            {
                { "RepairHealthPercentPerSecond", (parser, x) => x.RepairHealthPercentPerSecond = parser.ParsePercentage() },
                { "BoredTime", (parser, x) => x.BoredTime = parser.ParseTimeMillisecondsToLogicFrames() },
                { "BoredRange", (parser, x) => x.BoredRange = parser.ParseInteger() },
            });

        public Percentage RepairHealthPercentPerSecond { get; private set; }
        public LogicFrameSpan BoredTime { get; private set; }
        public int BoredRange { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new DozerAIUpdate(gameObject, context, this);
        }
    }
}
