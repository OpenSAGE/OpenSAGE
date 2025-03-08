namespace OpenSage;

public abstract class ModuleBase : DisposableBase, IPersistableObject
{
    public string Tag { get; internal set; }

    /// <summary>
    /// Called after all modules for a given Thing are created.
    /// It allows modules to resolve any inter-module dependencies.
    /// </summary>
    protected internal virtual void OnObjectCreated() { }

    void IPersistableObject.Persist(StatePersister persister)
    {
        Load(persister);
    }

    internal virtual void Load(StatePersister reader)
    {
        reader.PersistVersion(1);
    }

    internal virtual void DrawInspector() { }
}
