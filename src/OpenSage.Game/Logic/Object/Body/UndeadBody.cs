﻿#nullable enable

using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

/// <summary>
/// First death is intercepted and sets flags and max health.
/// Second death is handled normally.
/// </summary>
[AddedIn(SageGame.CncGeneralsZeroHour)]
public sealed class UndeadBody : ActiveBody
{
    private readonly UndeadBodyModuleData _moduleData;

    /// <summary>
    /// This is false until I detect death the first time, then I change my
    /// max, initial, and current health, and stop intercepting anything.
    /// </summary>
    private bool _isSecondLife;

    internal UndeadBody(GameObject gameObject, IGameEngine gameEngine, UndeadBodyModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        _moduleData = moduleData;
    }

    public override DamageInfoOutput AttemptDamage(in DamageInfoInput damageInput)
    {
        // If we are on our first life, see if this damage will kill us.
        // If it will, bind it to one hitpoint remaining, then go ahead
        // and take it.
        var shouldStartSecondLife = false;

        var modifiedDamageInput = damageInput;

        if (damageInput.DamageType != DamageType.Unresistable
            && !_isSecondLife
            && damageInput.Amount >= Health
            && damageInput.DamageType.IsHealthDamagingDamage())
        {
            modifiedDamageInput.Amount = Math.Min(damageInput.Amount, Health - 1);
            shouldStartSecondLife = true;
        }

        var damageOutput = AttemptDamage(modifiedDamageInput);

        // After we take it (which allows for damaging special effects),
        // we will do our modifications to the body module.
        if (shouldStartSecondLife)
        {
            StartSecondLife(damageInput, damageOutput);
        }

        return damageOutput;
    }

    private void StartSecondLife(in DamageInfoInput damageInput, in DamageInfoOutput damageOutput)
    {
        // Flag module as no longer intercepting damage.
        _isSecondLife = true;

        // Modify ActiveBody's max health and initial health.
        SetMaxHealth(_moduleData.SecondLifeMaxHealth, MaxHealthChangeType.FullyHeal);

        // Set Armor set flag to use second life armor.
        SetArmorSetFlag(ArmorSetCondition.SecondLife);

        // Fire the Slow Death module. The fact that this is not the result of
        // OnDie will cause the special behavior.
        var total = 0;
        foreach (var module in GameObject.FindBehaviors<SlowDeathBehavior>())
        {
            if (module.IsDieApplicable(damageInput))
            {
                total += module.GetProbabilityModifier(damageOutput);
            }
        }
        DebugUtility.AssertCrash(total > 0, "Hmm, this is wrong");

        // This returns a value from 1...total, inclusive.
        var roll = GameEngine.GameLogic.Random.Next(1, total);

        foreach (var module in GameObject.FindBehaviors<SlowDeathBehavior>())
        {
            if (module.IsDieApplicable(damageInput))
            {
                roll -= module.GetProbabilityModifier(damageOutput);
                if (roll <= 0)
                {
                    module.BeginSlowDeath(damageInput);
                    return;
                }
            }
        }
    }
}

/// <summary>
/// Treats the first death as a state change. Triggers the Use of SECOND_LIFE
/// ModelConditionState/ArmorSet and allows the use of the BattleBusSlowDeathBehavior module.
/// </summary>
[AddedIn(SageGame.CncGeneralsZeroHour)]
public sealed class UndeadBodyModuleData : ActiveBodyModuleData
{
    internal static new UndeadBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<UndeadBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
        .Concat(new IniParseTable<UndeadBodyModuleData>
        {
            { "SecondLifeMaxHealth", (parser, x) => x.SecondLifeMaxHealth = parser.ParseFloat() },
        });

    public float SecondLifeMaxHealth { get; private set; } = 1;
}
