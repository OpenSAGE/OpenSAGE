using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class PanicState : FollowWaypointsState
    {
        private uint _unknownInt1;
        private uint _unknownInt2;

        public PanicState(GameObject gameObject, GameContext context, AIUpdate aiUpdate)
            : base(gameObject, context, aiUpdate, false)
        {

        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Persist(reader);

            reader.PersistUInt32(ref _unknownInt1);
            reader.PersistUInt32(ref _unknownInt2);
        }
    }
}
