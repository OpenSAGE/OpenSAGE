namespace OpenSage.Logic.AI.AIStates
{
    internal class EnterContainerState : MoveTowardsState
    {
        private uint _containerObjectId;

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Persist(reader);

            reader.PersistObjectID(ref _containerObjectId);
        }
    }
}
