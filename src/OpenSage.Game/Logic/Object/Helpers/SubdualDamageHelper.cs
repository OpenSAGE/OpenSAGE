namespace OpenSage.Logic.Object.Helpers;

internal sealed class SubdualDamageHelper : ObjectHelperModule
{
    public SubdualDamageHelper(GameObject gameObject, GameEngine gameEngine) : base(gameObject, gameEngine)
    {
    }

    public void NotifySubdualDamage(float amount)
    {
        // TODO(Port): Implement this.
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.SkipUnknownBytes(4);
    }
}
