namespace OpenSage.Logic.AI.AIStates
{
    internal class EnterContainerState : MoveTowardsState
    {
        private uint _containerObjectId;

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.ReadObjectID(ref _containerObjectId);
        }
    }
}
