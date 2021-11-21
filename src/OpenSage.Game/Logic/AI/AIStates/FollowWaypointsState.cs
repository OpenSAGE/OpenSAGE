using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal class FollowWaypointsState : MoveTowardsState
    {
        private readonly bool _asTeam;

        public FollowWaypointsState(bool asTeam)
        {
            _asTeam = asTeam;
        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknownInt0 = reader.ReadUInt32();
            var unknownInt1 = reader.ReadUInt32();
            var unknownInt2 = reader.ReadUInt32();
            var unknownInt3 = reader.ReadUInt32();
            var waypointIdMaybe = reader.ReadUInt32();
            var waypointId2Maybe = reader.ReadUInt32();
            var unknownBool1 = reader.ReadBoolean();
        }
    }
}
