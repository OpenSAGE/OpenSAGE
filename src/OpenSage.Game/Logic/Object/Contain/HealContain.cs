﻿#nullable enable

using FixedMath.NET;
using OpenSage.Audio;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class HealContain : OpenContainModule
{
    private readonly HealContainModuleData _moduleData;

    internal HealContain(GameObject gameObject, IGameEngine gameEngine, HealContainModuleData moduleData) : base(gameObject, gameEngine, moduleData)
    {
        _moduleData = moduleData;
    }

    private protected override void UpdateModuleSpecific()
    {
        HealUnits(_moduleData.TimeForFullHeal);
        foreach (var unitId in ContainedObjectIds)
        {
            var unit = GameObjectForId(unitId);
            if (unit.BodyModule.Health == unit.BodyModule.MaxHealth)
            {
                Remove(unit.Id);
            }
        }
    }

    protected override bool CanUnitEnter(GameObject unit)
    {
        return unit.BodyModule.Health < unit.BodyModule.MaxHealth;
    }

    protected override BaseAudioEventInfo? GetEnterVoiceLine(UnitSpecificSounds sounds)
    {
        return sounds.VoiceGetHealed?.Value;
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
/// Automatically heals and restores the health of units that enter or exit the object.
/// </summary>
public sealed class HealContainModuleData : GarrisonContainModuleData
{
    internal static new HealContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<HealContainModuleData> FieldParseTable = GarrisonContainModuleData.FieldParseTable
        .Concat(new IniParseTable<HealContainModuleData>
        {
            { "TimeForFullHeal", (parser, x) => x.TimeForFullHeal = parser.ParseInteger() }
        });

    public int TimeForFullHeal { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new HealContain(gameObject, gameEngine, this);
    }
}
