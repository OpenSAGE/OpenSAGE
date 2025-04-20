#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using OpenSage.Logic.AI;
using OpenSage.Logic.AI.AIStates;

namespace OpenSage.Logic.Object;

internal sealed class DozerAndWorkerState : IPersistableObject
{
    public GameObject? BuildTarget => TryGetBuildTarget(out var buildTarget) ? buildTarget : null;
    public GameObject? RepairTarget => TryGetRepairTarget(out var repairTarget) ? repairTarget : null;

    private readonly GameObject _gameObject;
    private readonly IGameEngine _gameEngine;
    private readonly AIUpdate _aiUpdate;
    private readonly IBuilderAIUpdateData _moduleData;

    private readonly DozerTarget[] _dozerTargets = new DozerTarget[3];
    private readonly BuilderStateMachine _stateMachine;
    private int _unknown2;
    private readonly DozerSomething2[] _unknownList2 = new DozerSomething2[9]; // these seem to be in groups of 3, one group for each target
    private int _unknown4;

    public DozerAndWorkerState(GameObject gameObject, IGameEngine gameEngine, AIUpdate aiUpdate)
    {
        _gameObject = gameObject;
        _gameEngine = gameEngine;
        _aiUpdate = aiUpdate;
        _moduleData = (IBuilderAIUpdateData)aiUpdate.ModuleData; // todo: remove this cast in the future
        _stateMachine = new BuilderStateMachine(gameObject, gameEngine, gameObject.AIUpdate);
    }

    // todo: This is really state _machine_ behavior, and should be moved there when we better understand the fields
    public void Update()
    {
        if (TryGetBuildTarget(out var buildTarget))
        {
            UpdateBuildTarget(buildTarget);
        }
        else if (TryGetRepairTarget(out var repairTarget))
        {
            UpdateRepairTarget(repairTarget);
        }
    }

    public void ArrivedAtDestination()
    {
        if (BuildTarget != null || RepairTarget != null)
        {
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.ActivelyConstructing, true);
        }
    }

    private void UpdateBuildTarget(GameObject buildTarget)
    {
        if (!buildTarget.IsUnderActiveConstruction() && buildTarget.CollidesWith(_gameObject))
        {
            buildTarget.SetIsBeingConstructed();
        }
        else if (_gameObject.ModelConditionFlags.Get(ModelConditionFlag.ActivelyConstructing))
        {
            // advance construction
            buildTarget.AdvanceConstruction();

            if (buildTarget is { BuildProgress: >= 1 })
            {
                ClearDozerTasks();
                _gameEngine.AudioSystem.PlayAudioEvent(_gameObject, _gameObject.Definition.VoiceTaskComplete.Value);
            }
        }
    }

    private void UpdateRepairTarget(GameObject repairTarget)
    {
        if (repairTarget.BodyModule.Health == repairTarget.BodyModule.MaxHealth)
        {
            ClearDozerTasks();
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.ActivelyConstructing, false);
        }
        else if (repairTarget.HealedEndFrame <= _gameEngine.GameLogic.CurrentFrame)
        {
            // advance repair progress
            repairTarget.AttemptHealing(_moduleData.RepairHealthPercentPerSecond * repairTarget.BodyModule.MaxHealth, _gameObject);
            repairTarget.HealedEndFrame = _gameEngine.GameLogic.CurrentFrame + LogicFrameSpan.OneSecond(_gameEngine.LogicFramesPerSecond);
        }
    }

    public void ClearDozerTasks()
    {
        ClearBuildTarget();
        ClearRepairTarget();
    }

    private bool TryGetBuildTarget([NotNullWhen(true)] out GameObject? gameObject)
    {
        var id = _dozerTargets[0].ObjectId;
        if (id.IsValid)
        {
            gameObject = _gameEngine.GameLogic.GetObjectById(id);
            return true;
        }

        gameObject = null;
        return false;
    }

    public void SetBuildTarget(GameObject buildTarget, uint currentFrame)
    {
        _dozerTargets[0] = new DozerTarget { ObjectId = buildTarget.Id, OrderFrame = currentFrame };
        // these are both set to the unit currently (or most recently) constructing the object
        buildTarget.CreatedByObjectID = _gameObject.Id;
        buildTarget.BuiltByObjectID = _gameObject.Id;
    }

    private void ClearBuildTarget()
    {
        _gameObject.ModelConditionFlags.Set(ModelConditionFlag.ActivelyConstructing, false);
        _dozerTargets[0] = new DozerTarget();
    }

    private bool TryGetRepairTarget([NotNullWhen(true)] out GameObject? gameObject)
    {
        var id = _dozerTargets[1].ObjectId;
        if (id.IsValid)
        {
            gameObject = _gameEngine.GameLogic.GetObjectById(id);
            return true;
        }

        gameObject = null;
        return false;
    }

    public void SetRepairTarget(GameObject repairTarget, uint currentFrame)
    {
        repairTarget.HealedByObjectId = _gameObject.Id;
        _dozerTargets[1] = new DozerTarget { ObjectId = repairTarget.Id, OrderFrame = currentFrame };
    }

    private void ClearRepairTarget()
    {
        if (TryGetRepairTarget(out var repairTarget))
        {
            repairTarget.HealedByObjectId = ObjectId.Invalid;
            repairTarget.HealedEndFrame = LogicFrame.Zero;
        }
        _dozerTargets[1] = new DozerTarget();
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistArrayWithUInt32Length(
            _dozerTargets,
            static (StatePersister persister, ref DozerTarget item) =>
            {
                persister.PersistObjectValue(ref item);
            });

        reader.PersistObject(_stateMachine);
        reader.PersistInt32(ref _unknown2);

        var unknown3 = 3;
        reader.PersistInt32(ref unknown3);
        if (unknown3 != 0 && unknown3 != 3 && unknown3 != 257)
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

    private struct DozerTarget : IPersistableObject
    {
        public ObjectId ObjectId;
        public uint OrderFrame;

        public void Persist(StatePersister persister)
        {
            persister.PersistObjectId(ref ObjectId);
            persister.PersistUInt32(ref OrderFrame);
        }
    }

    private struct DozerSomething2 : IPersistableObject
    {
        public bool UnknownBool; // potentially whether the related dozertarget is active (assuming 3 DozerSomething2 for each DozerTarget)
        public Vector3 UnknownPos;

        public void Persist(StatePersister persister)
        {
            persister.PersistBoolean(ref UnknownBool);
            persister.PersistVector3(ref UnknownPos);
        }
    }

    internal static class BuilderStateIds
    {
        public static readonly StateId Idle = new(0);
        public static readonly StateId Build = new(1);
        public static readonly StateId Repair = new(2);
    }

    internal sealed class BuilderStateMachine : StateMachineBase
    {
        public BuilderStateMachine(GameObject gameObject, IGameEngine gameEngine, AIUpdate aiUpdate) : base(gameObject, gameEngine, aiUpdate)
        {
            DefineState(BuilderStateIds.Idle, new BuilderUnknown0State(this), StateId.Invalid, StateId.Invalid);
            DefineState(BuilderStateIds.Build, new BuilderUnknown1State(this), BuilderStateIds.Idle, BuilderStateIds.Idle);
            DefineState(BuilderStateIds.Repair, new BuilderUnknown2State(this), BuilderStateIds.Idle, BuilderStateIds.Idle);
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();
        }
    }
}
