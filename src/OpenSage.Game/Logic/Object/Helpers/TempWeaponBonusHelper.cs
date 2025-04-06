namespace OpenSage.Logic.Object.Helpers;

internal sealed class TempWeaponBonusHelper : ObjectHelperModule
{
    private int _unknownInt;

    public TempWeaponBonusHelper(GameObject gameObject, IGameEngine gameEngine) : base(gameObject, gameEngine)
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

        reader.PersistInt32(ref _unknownInt);
        if (_unknownInt != -1 && _unknownInt != 0)
        {
            // Normally -1, 0 for a civilian building when an infantry is garrisoned inside it
            throw new InvalidStateException();
        }

        reader.SkipUnknownBytes(4);
    }
}
