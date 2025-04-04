#nullable enable

using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

/// <summary>
/// Just like Active Body, but won't let health drop below 1.
/// </summary>
public sealed class ImmortalBody : ActiveBody
{
    internal ImmortalBody(GameObject gameObject, IGameEngine gameEngine, ImmortalBodyModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
    }

    public override void InternalChangeHealth(float delta)
    {
        // Don't let anything change us to below one hit point.
        delta = Math.Max(delta, -Health + 1);

        // Extend functionality, but I go first because I can't let you die and
        // then fix it, I must prevent.
        base.InternalChangeHealth(delta);

        DebugUtility.AssertCrash(
            Health > 0 && !GameObject.IsEffectivelyDead,
            "Immortal objects should never get marked as dead!");
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
/// Prevents the object from dying or taking damage.
/// </summary>
public sealed class ImmortalBodyModuleData : ActiveBodyModuleData
{
    internal static new ImmortalBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<ImmortalBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
        .Concat(new IniParseTable<ImmortalBodyModuleData>());

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new ImmortalBody(gameObject, gameEngine, this);
    }
}
