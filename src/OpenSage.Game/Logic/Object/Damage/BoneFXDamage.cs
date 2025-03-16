using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class BoneFXDamage : DamageModule
{
    public BoneFXDamage(GameObject gameObject, GameEngine gameEngine) : base(gameObject, gameEngine)
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
/// Enables use of BoneFXUpdate module on this object where an additional dynamic FX logic can
/// be used.
/// </summary>
public sealed class BoneFXDamageModuleData : DamageModuleData
{
    internal static BoneFXDamageModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<BoneFXDamageModuleData> FieldParseTable = new IniParseTable<BoneFXDamageModuleData>();

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new BoneFXDamage(gameObject, gameEngine);
    }
}
