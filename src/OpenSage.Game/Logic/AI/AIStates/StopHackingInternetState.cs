using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class StopHackingInternetState : State
    {
        public const uint StateId = 1002;

        private readonly GameObject _gameObject;

        private LogicFrameSpan _framesUntilFinishedPacking;

        public StopHackingInternetState(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public override void OnEnter()
        {
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, false);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, false);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, true);

            _gameObject.GameContext.AudioSystem.PlayAudioEvent(_gameObject, _gameObject.Definition.UnitSpecificSounds.UnitPack?.Value);

            var aiUpdate = (HackInternetAIUpdate)_gameObject.AIUpdate;

            var frames = aiUpdate.GetVariableFrames(aiUpdate.ModuleData.PackTime);

            _gameObject.Drawable.SetAnimationDuration(frames);

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
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, false);
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistLogicFrameSpan(ref _framesUntilFinishedPacking);
        }
    }
}
