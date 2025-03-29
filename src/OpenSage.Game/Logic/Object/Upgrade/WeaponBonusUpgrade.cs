using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

internal sealed class WeaponBonusUpgrade : UpgradeModule
{
    internal WeaponBonusUpgrade(GameObject gameObject, IGameEngine gameEngine, WeaponBonusUpgradeModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
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

/// <summary>
/// Triggers use of WeaponBonus = parameter on this object's weapons.
/// </summary>
public sealed class WeaponBonusUpgradeModuleData : UpgradeModuleData
{
    internal static WeaponBonusUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<WeaponBonusUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
        .Concat(new IniParseTable<WeaponBonusUpgradeModuleData>());

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new WeaponBonusUpgrade(gameObject, gameEngine, this);
    }
}
