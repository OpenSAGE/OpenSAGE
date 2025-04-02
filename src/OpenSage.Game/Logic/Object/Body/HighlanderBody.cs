#nullable enable

using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

/// <summary>
/// Takes damage according to armor, but can't die from normal damage.
/// Can die from <see cref="DamageType.Unresistable"/> though.
/// </summary>
public sealed class HighlanderBody : ActiveBody
{
    internal HighlanderBody(GameObject gameObject, IGameEngine gameEngine, HighlanderBodyModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
    }

    public override DamageInfoOutput AttemptDamage(in DamageInfoInput damageInput)
    {
        var modifiedDamageInput = damageInput;

        // Bind to one hitpoint remaining afterwards, unless it is Unresistable damage.
        if (damageInput.DamageType != DamageType.Unresistable)
        {
            modifiedDamageInput.Amount = Math.Min(damageInput.Amount, Health - 1);
        }

        return AttemptDamage(modifiedDamageInput);
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

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new HighlanderBody(gameObject, gameEngine, this);
    }
}
