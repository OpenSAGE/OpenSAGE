using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class PanicState : FollowWaypointsState
    {
        private uint _unknownInt1;
        private uint _unknownInt2;

        public PanicState()
            : base(false)
        {

        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);

            reader.PersistUInt32("UnknownInt1", ref _unknownInt1);
            reader.PersistUInt32("UnknownInt2", ref _unknownInt2);
        }
    }
}
