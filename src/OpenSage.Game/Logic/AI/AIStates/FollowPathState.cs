namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class FollowPathState : MoveTowardsState
    {
        private uint _unknownInt1;
        private bool _unknownBool1;
        private bool _unknownBool2;

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.ReadUInt32(ref _unknownInt1);
            reader.ReadBoolean(ref _unknownBool1);
            reader.ReadBoolean(ref _unknownBool2);
        }
    }
}
