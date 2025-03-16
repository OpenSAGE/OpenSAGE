using FixedMath.NET;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

internal sealed class MaxHealthUpgrade : UpgradeModule
{
    private readonly MaxHealthUpgradeModuleData _moduleData;

    internal MaxHealthUpgrade(GameObject gameObject, GameEngine context, MaxHealthUpgradeModuleData moduleData)
        : base(gameObject, context, moduleData)
    {
        _moduleData = moduleData;
    }

    protected override void OnUpgrade()
    {
        switch (_moduleData.ChangeType)
        {
            case MaxHealthChangeType.PreserveRatio:
                GameObject.Health += GameObject.HealthPercentage * (Fix64)_moduleData.AddMaxHealth;
                break;
            case MaxHealthChangeType.AddCurrentHealthToo:
                GameObject.Health += (Fix64)_moduleData.AddMaxHealth;
                break;
            case MaxHealthChangeType.SameCurrentHealth:
                // Don't add any new health
                break;
        }

        GameObject.MaxHealth += (Fix64)_moduleData.AddMaxHealth;
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

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine context)
    {
        return new MaxHealthUpgrade(gameObject, context, this);
    }
}

public enum MaxHealthChangeType
{
    [IniEnum("PRESERVE_RATIO")]
    PreserveRatio,

    [IniEnum("ADD_CURRENT_HEALTH_TOO")]
    AddCurrentHealthToo,

    [IniEnum("SAME_CURRENTHEALTH")]
    SameCurrentHealth
}
