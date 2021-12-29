using OpenSage.Data.Sav;

namespace OpenSage.Logic.AI.AIStates
{
    internal class FollowWaypointsState : MoveTowardsState
    {
        private readonly bool _asTeam;

        private uint _unknownInt1;
        private uint _unknownInt2;
        private uint _unknownInt3;
        private uint _unknownInt4;
        private uint _waypointIdMaybe1;
        private uint _waypointIdMaybe2;
        private bool _unknownBool;

        public FollowWaypointsState(bool asTeam)
        {
            _asTeam = asTeam;
        }

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.ReadUInt32(ref _unknownInt1);
            reader.ReadUInt32(ref _unknownInt2);
            reader.ReadUInt32(ref _unknownInt3);
            reader.ReadUInt32(ref _unknownInt4);
            reader.ReadUInt32(ref _waypointIdMaybe1);
            reader.ReadUInt32(ref _waypointIdMaybe2);
            reader.ReadBoolean(ref _unknownBool);
        }
    }
}
