using FixedMath.NET;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class HighlanderBody : ActiveBody
{
    internal HighlanderBody(GameObject gameObject, GameContext context, HighlanderBodyModuleData moduleData)
        : base(gameObject, context, moduleData)
    {
    }

    public override void AttemptDamage(ref DamageData damageInfo)
    {
        // TODO: Don't think this is right.
        if (damageInfo.Request.DamageType == DamageType.Unresistable)
        {
            Health -= (Fix64)damageInfo.Request.DamageToDeal;

            if (Health < (Fix64)0)
            {
                Health = (Fix64)0;
            }

            // TODO: DamageFX

            if (Health <= Fix64.Zero)
            {
                GameObject.Die(damageInfo.Request.DeathType);
            }
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
/// Allows the object to take damage but not die. The object will only die from irresistable damage.
/// </summary>
public sealed class HighlanderBodyModuleData : ActiveBodyModuleData
{
    internal static new HighlanderBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<HighlanderBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
        .Concat(new IniParseTable<HighlanderBodyModuleData>());

    internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
    {
        return new HighlanderBody(gameObject, context, this);
    }
}
