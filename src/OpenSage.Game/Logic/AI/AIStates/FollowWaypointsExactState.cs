using OpenSage.Logic.Object;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class FollowWaypointsExactState : MoveTowardsState
    {
        private readonly bool _asTeam;

        private uint _waypointId;

        public FollowWaypointsExactState(GameObject gameObject, GameContext context, AIUpdate aiUpdate, bool asTeam) : base(gameObject, context, aiUpdate)
        {
            _asTeam = asTeam;
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Persist(reader);

            reader.PersistUInt32(ref _waypointId);
        }
    }
}
