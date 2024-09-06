using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class DeadState : State
    {
        internal DeadState(GameObject gameObject, GameContext context) : base(gameObject, context)
        {
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);
        }
    }
}
