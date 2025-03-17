using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class BaseRegenerateUpdate : UpdateModule, IDamageModule
{
    protected override LogicFrameSpan FramesBetweenUpdates { get; }

    internal BaseRegenerateUpdate(GameObject gameObject, GameEngine gameEngine) : base(gameObject, gameEngine)
    {
        SetNextUpdateFrame(new LogicFrame(uint.MaxValue));
        FramesBetweenUpdates = LogicFrameSpan.OneSecond(gameEngine.LogicFramesPerSecond);
    }

    /// <summary>
    /// Increments the frame after which healing is allowed.
    /// </summary>
    public void OnDamage(in DamageData damageData)
    {
        var currentFrame = GameEngine.GameLogic.CurrentFrame;
        SetNextUpdateFrame(currentFrame + GameEngine.AssetLoadContext.AssetStore.GameData.Current.BaseRegenDelay);
    }

    private protected override void RunUpdate(BehaviorUpdateContext context)
    {
        GameObject.HealDirectly(GameEngine.AssetLoadContext.AssetStore.GameData.Current.BaseRegenHealthPercentPerSecond);

        if (GameObject.IsFullHealth)
        {
            SetNextUpdateFrame(new LogicFrame(uint.MaxValue));
        }
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

/// <summary>
/// Forces object to auto-repair itself over time. Parameters are defined in GameData.INI
/// through <see cref="GameData.BaseRegenHealthPercentPerSecond"/> and
/// <see cref="GameData.BaseRegenDelay"/>.
/// </summary>
public sealed class BaseRegenerateUpdateModuleData : UpdateModuleData
{
    internal static BaseRegenerateUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<BaseRegenerateUpdateModuleData> FieldParseTable = new IniParseTable<BaseRegenerateUpdateModuleData>();

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new BaseRegenerateUpdate(gameObject, gameEngine);
    }
}
