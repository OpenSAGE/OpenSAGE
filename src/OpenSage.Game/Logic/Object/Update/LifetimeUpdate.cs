﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

internal sealed class LifetimeUpdate : UpdateModule
{
    private readonly LifetimeUpdateModuleData _moduleData;

    private LogicFrame _frameToDie;

    // TODO: Should this be public?
    public LogicFrame FrameToDie
    {
        get => _frameToDie;
        set => _frameToDie = value;
    }

    public LifetimeUpdate(GameObject gameObject, IGameEngine gameEngine, LifetimeUpdateModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;

        var lifetimeFrames = gameEngine.GameLogic.Random.Next(
            (int)moduleData.MinLifetime.Value,
            (int)moduleData.MaxLifetime.Value);

        _frameToDie = gameEngine.GameLogic.CurrentFrame + new LogicFrameSpan((uint)lifetimeFrames);
    }

    public override UpdateSleepTime Update()
    {
        if (GameEngine.GameLogic.CurrentFrame >= _frameToDie)
        {
            GameObject.Kill(deathType: _moduleData.DeathType);
            _frameToDie = LogicFrame.MaxValue;
        }

        // TODO(Port): Use correct value.
        return UpdateSleepTime.None;
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistLogicFrame(ref _frameToDie);
    }
}

public sealed class LifetimeUpdateModuleData : UpdateModuleData
{
    internal static LifetimeUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<LifetimeUpdateModuleData> FieldParseTable = new IniParseTable<LifetimeUpdateModuleData>
    {
        { "MinLifetime", (parser, x) => x.MinLifetime = parser.ParseTimeMillisecondsToLogicFrames() },
        { "MaxLifetime", (parser, x) => x.MaxLifetime = parser.ParseTimeMillisecondsToLogicFrames() },
        { "WaitForWakeUp", (parser, x) => x.WaitForWakeUp = parser.ParseBoolean() },
        { "DeathType", (parser, x) => x.DeathType = parser.ParseEnum<DeathType>() }
    };

    public LogicFrameSpan MinLifetime { get; private set; }
    public LogicFrameSpan MaxLifetime { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public bool WaitForWakeUp { get; private set; }

    [AddedIn(SageGame.Bfme)]
    public DeathType DeathType { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new LifetimeUpdate(gameObject, gameEngine, this);
    }
}
