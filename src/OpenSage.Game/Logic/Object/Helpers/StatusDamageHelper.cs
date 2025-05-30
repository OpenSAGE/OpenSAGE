﻿namespace OpenSage.Logic.Object.Helpers;

internal sealed class StatusDamageHelper : ObjectHelperModule
{
    public StatusDamageHelper(GameObject gameObject, IGameEngine gameEngine)
        : base(gameObject, gameEngine)
    {
    }

    public void DoStatusDamage(ObjectStatus status, LogicFrameSpan duration)
    {
        // TODO(Port): Implement this.
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

        reader.SkipUnknownBytes(8);
    }
}
