#nullable enable

using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class StartHackingInternetState : State
    {
        private readonly HackInternetAIUpdateStateMachine _stateMachine;

        public const uint StateId = 1000;

        private LogicFrameSpan _framesUntilHackingBegins;

        public StartHackingInternetState(HackInternetAIUpdateStateMachine stateMachine) : base(stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public override void OnEnter()
        {
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, true);
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, false);
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, false);

            Context.AudioSystem.PlayAudioEvent(GameObject, GameObject.Definition.UnitSpecificSounds.UnitUnpack?.Value);

            var frames = _stateMachine.GetVariableFrames(_stateMachine.AIUpdate.ModuleData.UnpackTime, Context);

            GameObject.Drawable.SetAnimationDuration(frames);

            _framesUntilHackingBegins = frames;
        }

        public override UpdateStateResult Update()
        {
            if (_framesUntilHackingBegins-- == LogicFrameSpan.Zero)
            {
                return UpdateStateResult.TransitionToState(HackInternetState.StateId);
            }

            return UpdateStateResult.Continue();
        }

        public override void OnExit()
        {
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, false);
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistLogicFrameSpan(ref _framesUntilHackingBegins);
        }
    }
}
