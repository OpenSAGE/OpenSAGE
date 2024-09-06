using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI
{
    internal abstract class State : IPersistableObject
    {
        public uint Id { get; internal set; }

        private protected GameObject GameObject { get; }
        private protected GameContext Context { get; }

        private protected State(GameObject gameObject, GameContext context)
        {
            GameObject = gameObject;
            Context = context;
        }

        public virtual void OnEnter() { }

        public virtual UpdateStateResult Update() => UpdateStateResult.Continue();

        public virtual void OnExit() { }

        public abstract void Persist(StatePersister reader);
    }

    public readonly struct UpdateStateResult
    {
        public static UpdateStateResult Continue() => new(UpdateStateResultType.Continue, null);

        public static UpdateStateResult TransitionToState(uint stateId) => new(UpdateStateResultType.TransitionToState, stateId);

        public readonly UpdateStateResultType Type;
        public readonly uint? TransitionToStateId;

        private UpdateStateResult(UpdateStateResultType type, uint? transitionToStateId)
        {
            Type = type;
            TransitionToStateId = transitionToStateId;
        }
    }

    public enum UpdateStateResultType
    {
        Continue,
        TransitionToState,
    }
}
