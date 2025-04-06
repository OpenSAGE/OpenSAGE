namespace OpenSage.Logic.Object.Helpers;

internal sealed class ObjectRepulsorHelper : ObjectHelperModule
{
    // TODO
    public ObjectRepulsorHelper(GameObject gameObject, IGameEngine gameEngine) : base(gameObject, gameEngine)
    {
    }

    public override UpdateSleepTime Update()
    {
        // TODO(Port): Use correct value.
        return UpdateSleepTime.None;
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        base.Load(reader);
    }
}
