namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class WanderState : FollowWaypointsState
    {
        private uint _unknownInt1;
        private uint _unknownInt2;

        public WanderState()
            : base(false)
        {

        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);

            reader.PersistUInt32(ref _unknownInt1);
            reader.PersistUInt32(ref _unknownInt2);
        }
    }
}
