namespace OpenSage.Logic.Object.Helpers;

internal sealed class ObjectRepulsorHelper : ObjectHelperModule
{
    // TODO
    public ObjectRepulsorHelper(GameObject gameObject, IGameEngine gameEngine) : base(gameObject, gameEngine)
    {
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        base.Load(reader);
    }
}
