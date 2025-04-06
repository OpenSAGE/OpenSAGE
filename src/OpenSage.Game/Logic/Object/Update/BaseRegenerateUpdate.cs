using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public sealed class BaseRegenerateUpdate : UpdateModule, IDamageModule
{
    internal BaseRegenerateUpdate(GameObject gameObject, IGameEngine gameEngine)
        : base(gameObject, gameEngine)
    {
        SetWakeFrame(
            gameEngine.AssetStore.GameData.Current.BaseRegenHealthPercentPerSecond.IsZero
                ? UpdateSleepTime.Forever
                : UpdateSleepTime.None);
    }

    /// <summary>
    /// Increments the frame after which healing is allowed.
    /// </summary>
    public void OnDamage(in DamageInfo damageData)
    {
        var sleepTime =
            GameEngine.AssetStore.GameData.Current.BaseRegenHealthPercentPerSecond > new Percentage(0.0f) && damageData.Request.DamageType != DamageType.Healing
            ? UpdateSleepTime.Frames(GameEngine.AssetLoadContext.AssetStore.GameData.Current.BaseRegenDelay)
            : UpdateSleepTime.Forever;

        SetWakeFrame(sleepTime);
    }

    public override UpdateSleepTime Update()
    {
        GameObject.AttemptHealing(
            GameObject.BodyModule.MaxHealth * GameEngine.AssetLoadContext.AssetStore.GameData.Current.BaseRegenHealthPercentPerSecond / GameEngine.LogicFramesPerSecond,
            GameObject);

        if (GameObject.BodyModule.Health == GameObject.BodyModule.MaxHealth)
        {
            return UpdateSleepTime.Forever;
        }

        // We don't really need to heal every frame. Do it every 3 frames or so.
        const int healRate = 3;
        return UpdateSleepTime.Frames(new LogicFrameSpan(healRate));
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

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new BaseRegenerateUpdate(gameObject, gameEngine);
    }
}
