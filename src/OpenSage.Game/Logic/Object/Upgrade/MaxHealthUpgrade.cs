using FixedMath.NET;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

internal sealed class MaxHealthUpgrade : UpgradeModule
{
    private readonly MaxHealthUpgradeModuleData _moduleData;

    internal MaxHealthUpgrade(GameObject gameObject, IGameEngine gameEngine, MaxHealthUpgradeModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        _moduleData = moduleData;
    }

    protected override void OnUpgrade()
    {
        GameObject.BodyModule.SetMaxHealth(_moduleData.AddMaxHealth, _moduleData.ChangeType);
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

public sealed class MaxHealthUpgradeModuleData : UpgradeModuleData
{
    internal static MaxHealthUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<MaxHealthUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
        .Concat(new IniParseTable<MaxHealthUpgradeModuleData>
        {
            { "AddMaxHealth", (parser, x) => x.AddMaxHealth = parser.ParseFloat() },
            { "ChangeType", (parser, x) => x.ChangeType = parser.ParseEnum<MaxHealthChangeType>() },
        });

    public float AddMaxHealth { get; private set; }
    public MaxHealthChangeType ChangeType { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new MaxHealthUpgrade(gameObject, gameEngine, this);
    }
}

public enum MaxHealthChangeType
{
    [IniEnum("SAME_CURRENTHEALTH")]
    SameCurrentHealth,

    [IniEnum("PRESERVE_RATIO")]
    PreserveRatio,

    [IniEnum("ADD_CURRENT_HEALTH_TOO")]
    AddCurrentHealthToo,

    [IniEnum("FULLY_HEAL"), AddedIn(SageGame.CncGeneralsZeroHour)]
    FullyHeal,
}
