using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class FollowWaypointsExactState : MoveTowardsState
    {
        private readonly bool _asTeam;

        private uint _waypointId;

        public FollowWaypointsExactState(bool asTeam)
        {
            _asTeam = asTeam;
        }

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.ReadUInt32(ref _waypointId);
        }
    }
}
