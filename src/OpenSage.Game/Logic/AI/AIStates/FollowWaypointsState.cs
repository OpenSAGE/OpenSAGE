#nullable enable

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

        public FollowWaypointsState(AIUpdateStateMachine stateMachine, bool asTeam) : base(stateMachine)
        {
            _asTeam = asTeam;
        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistBase(base.Persist);

            reader.PersistUInt32(ref _unknownInt1);
            reader.PersistUInt32(ref _unknownInt2);
            reader.PersistUInt32(ref _unknownInt3);
            reader.PersistUInt32(ref _unknownInt4);
            reader.PersistUInt32(ref _waypointIdMaybe1);
            reader.PersistUInt32(ref _waypointIdMaybe2);
            reader.PersistBoolean(ref _unknownBool);
        }
    }
}
