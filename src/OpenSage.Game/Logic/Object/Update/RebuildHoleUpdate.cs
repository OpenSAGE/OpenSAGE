using FixedMath.NET;
using ImGuiNET;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// This is RebuildHoleBehavior in the inis, but appears to be an update module
    /// </summary>
    public sealed class RebuildHoleUpdate : UpdateModule, ISelfHealable
    {
        private readonly GameObject _gameObject;
        private readonly GameContext _context;
        private readonly RebuildHoleUpdateModuleData _moduleData;

        private readonly Percentage _healPercentagePerFrame;

        private uint _workerId; // the worker that is building the structure
        private uint _structureId; // the structure that is being built
        private uint _originalStructureId; // the structure that was destroyed to create this hole

        private LogicFrameSpan _framesUntilConstructionBegins;

        private string _workerObjectName; // the worker to spawn to build the structure
        private ObjectDefinition WorkerObjectDefinition => _context.Game.AssetStore.ObjectDefinitions.GetByName(_workerObjectName);
        private string _structureObjectName; // the structure we're rebuilding
        private ObjectDefinition StructureObjectDefinition => _context.Game.AssetStore.ObjectDefinitions.GetByName(_structureObjectName);

        internal RebuildHoleUpdate(GameObject gameObject, GameContext context, RebuildHoleUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _context = context;
            _moduleData = moduleData;

            _healPercentagePerFrame = new Percentage((float)moduleData.HoleHealthRegenPercentPerSecond / Game.LogicFramesPerSecond);
            _workerObjectName = moduleData.WorkerObjectDefinition.Value.Name;

            ResetConstructionCounter();
        }

        // set on creation in RebuildHoleExposeDie
        public void SetOriginalStructure(GameObject originalStructure)
        {
            _originalStructureId = originalStructure.ID;
            _structureObjectName = originalStructure.Definition.Name;
        }

        private void ResetConstructionCounter()
        {
            _framesUntilConstructionBegins = _moduleData.WorkerRespawnDelay;
        }

        // scaffolds have a status of 0001 0000 0000 0000 0000 0100 in sav file (0100 is under construction)
        private const int UnknownScaffoldStatus = 20;

        private protected override void RunUpdate(BehaviorUpdateContext context)
        {
            if (!_gameObject.IsFullHealth)
            {
                if (_gameObject.HealthPercentage == Fix64.Zero)
                {
                    return;
                }
                _gameObject.HealDirectly(_healPercentagePerFrame);
            }

            if (_framesUntilConstructionBegins != LogicFrameSpan.Zero)
            {
                _framesUntilConstructionBegins--;
                return;
            }

            GameObject worker;
            if (_workerId == 0)
            {
                // spawn worker first
                worker = _context.GameLogic.CreateObject(WorkerObjectDefinition, _gameObject.Owner);
                worker.SetTransformMatrix(_gameObject.TransformMatrix);
                worker.SetSelectable(false); // we have no control over this worker
                _workerId = worker.ID;
            }
            else
            {
                worker = _context.GameLogic.GetObjectById(_workerId);
            }

            GameObject structure;
            if (_structureId == 0)
            {
                // spawn structure after spawning worker
                structure = _context.GameLogic.CreateObject(StructureObjectDefinition, _gameObject.Owner);
                structure.SetTransformMatrix(_gameObject.TransformMatrix);
                // todo: some special property that disables the cancel construction button?
                _structureId = structure.ID;
                structure.SetIsBeingConstructed();
                structure.SetObjectStatus(ObjectStatus.UnderConstruction, true);
                structure.SetUnknownStatus(UnknownScaffoldStatus, true);
                _gameObject.SetObjectStatus(ObjectStatus.InsideGarrison, true); // yup, seriously
            }
            else
            {
                structure = _context.GameLogic.GetObjectById(_structureId);
            }

            if (worker == null || worker.IsDead)
            {
                // he's dead, jim
                ResetConstructionCounter();
                _workerId = 0;
            }
            else if (structure == null || structure.IsDead)
            {
                // if the structure dies, we reset the worker as well
                ResetConstructionCounter();
                _structureId = 0;
                _workerId = 0;
                worker.Destroy();
                _gameObject.SetObjectStatus(ObjectStatus.InsideGarrison, false); // the hole is no longer protected
            }
            else if (!structure.IsBeingConstructed())
            {
                // construction complete - we're done here
                _workerId = 0;
                _structureId = 0;
                structure.SetUnknownStatus(UnknownScaffoldStatus, false);
                worker.Destroy();
                _gameObject.Destroy();
            }
            else if (worker.AIUpdate is WorkerAIUpdate workerAiUpdate)
            {
                // assign the worker to the structure if we haven't already
                if (workerAiUpdate.BuildTarget != structure)
                {
                    workerAiUpdate.SetBuildTarget(structure);
                }
            }
            else
            {
                // AIUpdate should always be WorkerAIUpdate, so throw if it's not
                throw new InvalidStateException("worker does not have WorkerAIUpdate module");
            }
        }

        internal override void OnDie(BehaviorUpdateContext context, DeathType deathType, BitArray<ObjectStatus> status)
        {
            var worker = _context.GameLogic.GetObjectById(_workerId);
            var structure = _context.GameLogic.GetObjectById(_structureId);
            worker?.Destroy();
            structure?.Destroy();
            base.OnDie(context, deathType, status);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistObjectID(ref _workerId);
            reader.PersistObjectID(ref _structureId);
            reader.PersistObjectID(ref _originalStructureId);

            reader.PersistLogicFrameSpan(ref _framesUntilConstructionBegins);

            reader.PersistAsciiString(ref _workerObjectName);
            reader.PersistAsciiString(ref _structureObjectName);
        }

        internal override void DrawInspector()
        {
            base.DrawInspector();
            ImGui.LabelText("Frames until construction begins", _framesUntilConstructionBegins.ToString());
            ImGui.LabelText("Worker ID", _workerId.ToString());
            ImGui.LabelText("Structure ID", _structureId.ToString());
            ImGui.LabelText("Original Structure ID", _originalStructureId.ToString());
        }

        public void RegisterDamage()
        {
            // no healing delay, nothing to do here
        }
    }

    /// <summary>
    /// Requires the REBUILD_HOLE KindOf on the object that will use this to work properly.
    /// </summary>
    public sealed class RebuildHoleUpdateModuleData : UpdateModuleData
    {
        internal static RebuildHoleUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(BaseFieldParseTable);

        internal static readonly IniParseTable<RebuildHoleUpdateModuleData> BaseFieldParseTable = new IniParseTable<RebuildHoleUpdateModuleData>
        {
            { "WorkerObjectName", (parser, x) => x.WorkerObjectDefinition = parser.ParseObjectReference() },
            { "WorkerRespawnDelay", (parser, x) => x.WorkerRespawnDelay = parser.ParseTimeMillisecondsToLogicFrames() },
            { "HoleHealthRegen%PerSecond", (parser, x) => x.HoleHealthRegenPercentPerSecond = parser.ParsePercentage() }
        };

        public LazyAssetReference<ObjectDefinition> WorkerObjectDefinition { get; private set; }

        public LogicFrameSpan WorkerRespawnDelay { get; private set; }

        public Percentage HoleHealthRegenPercentPerSecond { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new RebuildHoleUpdate(gameObject, context, this);
        }
    }
}
