#nullable enable

using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates;

internal sealed class StartHackingInternetState : State
{
    private readonly HackInternetAIUpdateStateMachine _stateMachine;

    private LogicFrameSpan _framesUntilHackingBegins;

    public StartHackingInternetState(HackInternetAIUpdateStateMachine stateMachine) : base(stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public override StateReturnType OnEnter()
    {
        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, true);
        GameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, false);
        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, false);

        GameEngine.AudioSystem.PlayAudioEvent(GameObject, GameObject.Definition.UnitSpecificSounds.UnitUnpack?.Value);

        var frames = _stateMachine.GetVariableFrames(_stateMachine.AIUpdate.ModuleData.UnpackTime, GameEngine);

        GameObject.Drawable.SetAnimationDuration(frames);

        _framesUntilHackingBegins = frames;

        return StateReturnType.Continue;
    }

    public override StateReturnType Update()
    {
        if (_framesUntilHackingBegins-- == LogicFrameSpan.Zero)
        {
            return StateReturnType.Success;
        }

        return StateReturnType.Continue;
    }

    public override void OnExit(StateExitType status)
    {
        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, false);
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistLogicFrameSpan(ref _framesUntilHackingBegins);
    }
}
