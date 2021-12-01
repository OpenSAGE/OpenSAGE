namespace OpenSage
{
    public abstract class ModuleBase : DisposableBase
    {
        public string Tag { get; internal set; }

        internal virtual void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);
        }
    }
}
