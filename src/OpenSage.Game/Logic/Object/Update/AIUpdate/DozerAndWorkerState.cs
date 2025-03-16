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
    private readonly GameEngine _gameEngine;
    private readonly AIUpdate _aiUpdate;
    private readonly IBuilderAIUpdateData _moduleData;

    private readonly DozerTarget[] _dozerTargets = new DozerTarget[3];
    private readonly BuilderStateMachine _stateMachine;
    private int _unknown2;
    private readonly DozerSomething2[] _unknownList2 = new DozerSomething2[9]; // these seem to be in groups of 3, one group for each target
    private int _unknown4;

    public DozerAndWorkerState(GameObject gameObject, GameEngine gameEngine, AIUpdate aiUpdate)
    {
        _gameObject = gameObject;
        _gameEngine = gameEngine;
        _aiUpdate = aiUpdate;
        _moduleData = (IBuilderAIUpdateData)aiUpdate.ModuleData; // todo: remove this cast in the future
        _stateMachine = new BuilderStateMachine(gameObject, gameEngine, gameObject.AIUpdate);
    }

    // todo: This is really state _machine_ behavior, and should be moved there when we better understand the fields
    public void Update(BehaviorUpdateContext updateContext)
    {
        if (TryGetBuildTarget(out var buildTarget))
        {
            UpdateBuildTarget(buildTarget);
        }
        else if (TryGetRepairTarget(out var repairTarget))
        {
            UpdateRepairTarget(repairTarget, updateContext);
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

    private void UpdateRepairTarget(GameObject repairTarget, BehaviorUpdateContext updateContext)
    {
        if (repairTarget.IsFullHealth)
        {
            ClearDozerTasks();
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.ActivelyConstructing, false);
        }
        else if (repairTarget.HealedEndFrame <= updateContext.LogicFrame.Value)
        {
            // advance repair progress
            repairTarget.Heal(_moduleData.RepairHealthPercentPerSecond, _gameObject);
            repairTarget.HealedEndFrame = (updateContext.LogicFrame + LogicFrameSpan.OneSecond).Value;
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
        if (id > 0)
        {
            gameObject = _gameEngine.GameLogic.GetObjectById(id);
            return true;
        }

        gameObject = null;
        return false;
    }

    public void SetBuildTarget(GameObject buildTarget, uint currentFrame)
    {
        _dozerTargets[0] = new DozerTarget { ObjectId = buildTarget.ID, OrderFrame = currentFrame };
        // these are both set to the unit currently (or most recently) constructing the object
        buildTarget.CreatedByObjectID = _gameObject.ID;
        buildTarget.BuiltByObjectID = _gameObject.ID;
    }

    private void ClearBuildTarget()
    {
        _gameObject.ModelConditionFlags.Set(ModelConditionFlag.ActivelyConstructing, false);
        _dozerTargets[0] = new DozerTarget();
    }

    private bool TryGetRepairTarget([NotNullWhen(true)] out GameObject? gameObject)
    {
        var id = _dozerTargets[1].ObjectId;
        if (id > 0)
        {
            gameObject = _gameEngine.GameLogic.GetObjectById(id);
            return true;
        }

        gameObject = null;
        return false;
    }

    public void SetRepairTarget(GameObject repairTarget, uint currentFrame)
    {
        repairTarget.HealedByObjectId = _gameObject.ID;
        _dozerTargets[1] = new DozerTarget { ObjectId = repairTarget.ID, OrderFrame = currentFrame };
    }

    private void ClearRepairTarget()
    {
        if (TryGetRepairTarget(out var repairTarget))
        {
            repairTarget.HealedByObjectId = 0;
            repairTarget.HealedEndFrame = 0;
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
        public uint ObjectId;
        public uint OrderFrame;

        public void Persist(StatePersister persister)
        {
            persister.PersistObjectID(ref ObjectId);
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

    internal sealed class BuilderStateMachine : StateMachineBase
    {
        public BuilderStateMachine(GameObject gameObject, GameEngine context, AIUpdate aiUpdate) : base(gameObject, context, aiUpdate)
        {
            AddState(0, new BuilderUnknown0State(this));
            AddState(1, new BuilderUnknown1State(this));
            AddState(2, new BuilderUnknown2State(this));
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
