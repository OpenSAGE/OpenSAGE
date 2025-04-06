namespace OpenSage.Logic.Object.Helpers;

internal sealed class ObjectWeaponStatusHelper : ObjectHelperModule
{
    // TODO
    protected override UpdateOrder UpdateOrder => UpdateOrder.Order3;

    public ObjectWeaponStatusHelper(GameObject gameObject, IGameEngine gameEngine) : base(gameObject, gameEngine)
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

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}
