using FixedMath.NET;
using ImGuiNET;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

/// <summary>
/// This is RebuildHoleBehavior in the inis, but appears to be an update module
/// </summary>
public sealed class RebuildHoleUpdate : UpdateModule, IDieModule
{
    private readonly RebuildHoleUpdateModuleData _moduleData;

    private readonly Percentage _healPercentagePerFrame;

    private ObjectId _workerId; // the worker that is building the structure
    private ObjectId _structureId; // the structure that is being built
    private ObjectId _originalStructureId; // the structure that was destroyed to create this hole

    private LogicFrameSpan _framesUntilConstructionBegins;

    private string _workerObjectName; // the worker to spawn to build the structure
    private ObjectDefinition WorkerObjectDefinition => GameEngine.Game.AssetStore.ObjectDefinitions.GetByName(_workerObjectName);
    private string _structureObjectName; // the structure we're rebuilding
    private ObjectDefinition StructureObjectDefinition => GameEngine.Game.AssetStore.ObjectDefinitions.GetByName(_structureObjectName);

    internal RebuildHoleUpdate(GameObject gameObject, IGameEngine gameEngine, RebuildHoleUpdateModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;

        _healPercentagePerFrame = new Percentage((float)moduleData.HoleHealthRegenPercentPerSecond / GameEngine.LogicFramesPerSecond);
        _workerObjectName = moduleData.WorkerObjectDefinition.Value.Name;

        ResetConstructionCounter();
    }

    // set on creation in RebuildHoleExposeDie
    public void SetOriginalStructure(GameObject originalStructure)
    {
        _originalStructureId = originalStructure.Id;
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
        if (GameObject.BodyModule.Health < GameObject.BodyModule.MaxHealth)
        {
            GameObject.AttemptHealing(_healPercentagePerFrame * GameObject.BodyModule.MaxHealth, GameObject);
        }

        if (_framesUntilConstructionBegins != LogicFrameSpan.Zero)
        {
            _framesUntilConstructionBegins--;
            return;
        }

        GameObject worker;
        if (_workerId.IsInvalid)
        {
            // spawn worker first
            worker = GameEngine.GameLogic.CreateObject(WorkerObjectDefinition, GameObject.Owner);
            worker.SetTransformMatrix(GameObject.TransformMatrix);
            worker.SetSelectable(false); // we have no control over this worker
            _workerId = worker.Id;
        }
        else
        {
            worker = GameEngine.GameLogic.GetObjectById(_workerId);
        }

        GameObject structure;
        if (_structureId.IsInvalid)
        {
            // spawn structure after spawning worker
            structure = GameEngine.GameLogic.CreateObject(StructureObjectDefinition, GameObject.Owner);
            structure.SetTransformMatrix(GameObject.TransformMatrix);
            // todo: some special property that disables the cancel construction button?
            _structureId = structure.Id;
            structure.SetIsBeingConstructed();
            structure.SetObjectStatus(ObjectStatus.UnderConstruction, true);
            structure.SetUnknownStatus(UnknownScaffoldStatus, true);
            GameObject.SetObjectStatus(ObjectStatus.InsideGarrison, true); // yup, seriously
        }
        else
        {
            structure = GameEngine.GameLogic.GetObjectById(_structureId);
        }

        if (worker == null || worker.IsEffectivelyDead)
        {
            // he's dead, jim
            ResetConstructionCounter();
            _workerId = ObjectId.Invalid;
        }
        else if (structure == null || structure.IsEffectivelyDead)
        {
            // if the structure dies, we reset the worker as well
            ResetConstructionCounter();
            _structureId = ObjectId.Invalid;
            _workerId = ObjectId.Invalid;
            worker.Destroy();
            GameObject.SetObjectStatus(ObjectStatus.InsideGarrison, false); // the hole is no longer protected
        }
        else if (!structure.IsBeingConstructed())
        {
            // construction complete - we're done here
            _workerId = ObjectId.Invalid;
            _structureId = ObjectId.Invalid;
            structure.SetUnknownStatus(UnknownScaffoldStatus, false);
            worker.Destroy();
            GameObject.Destroy();
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

    void IDieModule.OnDie(in DamageInfoInput damageInput)
    {
        var worker = GameEngine.GameLogic.GetObjectById(_workerId);
        var structure = GameEngine.GameLogic.GetObjectById(_structureId);
        worker?.Destroy();
        structure?.Destroy();
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(2);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistObjectId(ref _workerId);
        reader.PersistObjectId(ref _structureId);
        reader.PersistObjectId(ref _originalStructureId);

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

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new RebuildHoleUpdate(gameObject, gameEngine, this);
    }
}
