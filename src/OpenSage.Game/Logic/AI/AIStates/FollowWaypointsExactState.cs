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

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Persist(reader);

            reader.PersistUInt32("WaypointId", ref _waypointId);
        }
    }
}
