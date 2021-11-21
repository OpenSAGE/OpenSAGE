using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class FollowWaypointsExactState : MoveTowardsState
    {
        private readonly bool _asTeam;

        public FollowWaypointsExactState(bool asTeam)
        {
            _asTeam = asTeam;
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var waypointId = reader.ReadUInt32();
        }
    }
}
