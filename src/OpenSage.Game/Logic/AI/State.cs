namespace OpenSage.Logic.AI
{
    internal abstract class State : IPersistableObject
    {
        public abstract void Persist(StatePersister reader);
    }
}
