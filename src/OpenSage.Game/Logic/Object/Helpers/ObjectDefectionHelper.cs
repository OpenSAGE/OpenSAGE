﻿namespace OpenSage.Logic.Object.Helpers;

internal sealed class ObjectDefectionHelper : ObjectHelperModule
{
    private uint _frameStart;
    private uint _frameEnd;
    private bool _unknown;

    public ObjectDefectionHelper(GameObject gameObject, IGameEngine gameEngine) : base(gameObject, gameEngine)
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

        reader.PersistUInt32(ref _frameStart);
        reader.PersistUInt32(ref _frameEnd);

        reader.SkipUnknownBytes(4);

        reader.PersistBoolean(ref _unknown);
    }
}
