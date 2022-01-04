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
            reader.PersistVersion(1);

            base.Load(reader);

            reader.PersistUInt32("WaypointId", ref _waypointId);
        }
    }
}
