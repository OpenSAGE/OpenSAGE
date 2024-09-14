#nullable enable

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class WanderState : FollowWaypointsState
    {
        private uint _unknownInt1;
        private uint _unknownInt2;

        public WanderState(AIUpdateStateMachine stateMachine)
            : base(stateMachine, false)
        {
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistBase(base.Persist);

            reader.PersistUInt32(ref _unknownInt1);
            reader.PersistUInt32(ref _unknownInt2);
        }
    }
}
