namespace OpenSage
{
    public abstract class ModuleBase : DisposableBase
    {
        public string Tag { get; internal set; }

        internal virtual void Load(StatePersister reader)
        {
            reader.ReadVersion(1);
        }
    }
}
