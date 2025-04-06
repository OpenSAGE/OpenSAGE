namespace OpenSage.Logic.Object.Helpers;

internal sealed class ObjectFiringTrackerHelper : UpdateModule
{
    private uint _numShotsFiredAtLastTarget;
    private ObjectId _lastTargetObjectId;

    protected override UpdateOrder UpdateOrder => UpdateOrder.Order3;

    public ObjectFiringTrackerHelper(GameObject gameObject, IGameEngine gameEngine) : base(gameObject, gameEngine)
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

        reader.PersistUInt32(ref _numShotsFiredAtLastTarget);
        reader.PersistObjectId(ref _lastTargetObjectId);

        reader.SkipUnknownBytes(4);
    }
}
