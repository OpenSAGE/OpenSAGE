using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class StopHackingInternetState : State
    {
        public const uint StateId = 1002;

        private readonly HackInternetAIUpdate _aiUpdate;

        private LogicFrameSpan _framesUntilFinishedPacking;

        public StopHackingInternetState(GameObject gameObject, GameContext context, HackInternetAIUpdate aiUpdate) : base(gameObject, context)
        {
            _aiUpdate = aiUpdate;
        }

        public override void OnEnter()
        {
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, false);
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, false);
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, true);

            Context.AudioSystem.PlayAudioEvent(GameObject, GameObject.Definition.UnitSpecificSounds.UnitPack?.Value);

            var frames = _aiUpdate.GetVariableFrames(_aiUpdate.ModuleData.PackTime, Context);

            GameObject.Drawable.SetAnimationDuration(frames);

            _framesUntilFinishedPacking = frames;
        }

        public override UpdateStateResult Update()
        {
            if (_framesUntilFinishedPacking-- == LogicFrameSpan.Zero)
            {
                return UpdateStateResult.TransitionToState(IdleState.StateId);
            }

            return UpdateStateResult.Continue();
        }

        public override void OnExit()
        {
            GameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, false);
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistLogicFrameSpan(ref _framesUntilFinishedPacking);
        }
    }
}
