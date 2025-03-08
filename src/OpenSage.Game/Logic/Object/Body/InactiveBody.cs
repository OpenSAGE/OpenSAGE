using FixedMath.NET;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class InactiveBody : BodyModule
{
    internal InactiveBody(GameObject gameObject, GameContext context) : base(gameObject, context)
    {
    }

    public override void AttemptDamage(ref DamageData damageInfo)
    {
        // TODO

        GameObject.Die(damageInfo.Request.DeathType);
    }

    public override Fix64 MaxHealth
    {
        get => Fix64.Zero;
        internal set { }
    }

    public override void SetArmorSetFlag(ArmorSetCondition armorSetCondition) { }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

/// <summary>
/// Prevents normal interaction with other objects.
/// </summary>
public sealed class InactiveBodyModuleData : BodyModuleData
{
    internal static InactiveBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<InactiveBodyModuleData> FieldParseTable = new IniParseTable<InactiveBodyModuleData>();

    internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
    {
        return new InactiveBody(gameObject, context);
    }
}
