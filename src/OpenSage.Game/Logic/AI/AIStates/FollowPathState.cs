namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class FollowPathState : MoveTowardsState
    {
        private uint _unknownInt1;
        private bool _unknownBool1;
        private bool _unknownBool2;

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Persist(reader);

            reader.PersistUInt32(ref _unknownInt1);
            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);
        }
    }
}
