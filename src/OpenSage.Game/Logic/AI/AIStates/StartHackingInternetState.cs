using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class StartHackingInternetState : State
    {
        public const uint StateId = 1000;

        private readonly GameObject _gameObject;

        private LogicFrameSpan _framesUntilHackingBegins;

        public StartHackingInternetState(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public override void OnEnter()
        {
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, true);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, false);
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, false);

            _gameObject.GameContext.AudioSystem.PlayAudioEvent(_gameObject, _gameObject.Definition.UnitSpecificSounds.UnitUnpack?.Value);

            var aiUpdate = (HackInternetAIUpdate)_gameObject.AIUpdate;

            var frames = aiUpdate.GetVariableFrames(aiUpdate.ModuleData.UnpackTime);

            _gameObject.Drawable.SetAnimationDuration(frames);

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
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, false);
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistLogicFrameSpan(ref _framesUntilHackingBegins);
        }
    }
}
