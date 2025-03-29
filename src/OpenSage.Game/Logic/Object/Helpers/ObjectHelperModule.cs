namespace OpenSage.Logic.Object.Helpers;

internal abstract class ObjectHelperModule : UpdateModule
{
    protected ObjectHelperModule(GameObject gameObject, IGameEngine gameEngine) : base(gameObject, gameEngine)
    {
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}
