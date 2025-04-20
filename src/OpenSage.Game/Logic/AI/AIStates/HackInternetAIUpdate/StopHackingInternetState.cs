#nullable enable

using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class StopHackingInternetState : State
{
    private readonly HackInternetAIUpdateStateMachine _stateMachine;

    private LogicFrameSpan _framesUntilFinishedPacking;

    public StopHackingInternetState(HackInternetAIUpdateStateMachine stateMachine) : base(stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public override StateReturnType OnEnter()
    {
        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, false);
        GameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, false);
        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, true);

        GameEngine.AudioSystem.PlayAudioEvent(GameObject, GameObject.Definition.UnitSpecificSounds.UnitPack?.Value);

        var frames = _stateMachine.GetVariableFrames(_stateMachine.AIUpdate.ModuleData.PackTime, GameEngine);

        GameObject.Drawable.SetAnimationDuration(frames);

        _framesUntilFinishedPacking = frames;

        return StateReturnType.Continue;
    }

    public override StateReturnType Update()
    {
        if (_framesUntilFinishedPacking-- == LogicFrameSpan.Zero)
        {
            return StateReturnType.Success;
        }

        return StateReturnType.Continue;
    }

    public override void OnExit(StateExitType status)
    {
        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, false);
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistLogicFrameSpan(ref _framesUntilFinishedPacking);
    }
}
