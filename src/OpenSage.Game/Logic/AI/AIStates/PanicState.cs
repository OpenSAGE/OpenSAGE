using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class PanicState : FollowWaypointsState
    {
        public PanicState()
            : base(false)
        {

        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknownInt0 = reader.ReadUInt32();
            var unknownInt1 = reader.ReadUInt32();
        }
    }
}
