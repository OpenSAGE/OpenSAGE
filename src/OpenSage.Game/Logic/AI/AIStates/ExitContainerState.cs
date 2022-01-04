namespace OpenSage.Logic.AI.AIStates
{
    internal sealed class ExitContainerState : State
    {
        private uint _containerObjectId;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistObjectID("ContainerObjectId", ref _containerObjectId);
        }
    }
}
