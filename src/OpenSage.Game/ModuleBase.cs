namespace OpenSage
{
    public abstract class ModuleBase : DisposableBase, IPersistableObject
    {
        public string Tag { get; internal set; }

        void IPersistableObject.Persist(StatePersister persister)
        {
            Load(persister);
        }

        internal virtual void Load(StatePersister reader)
        {
            reader.PersistVersion(1);
        }
    }
}
