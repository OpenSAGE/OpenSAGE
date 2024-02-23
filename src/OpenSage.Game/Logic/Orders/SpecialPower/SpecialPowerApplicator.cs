using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.Orders.SpecialPower;

// burton detonate demo charge      SpecialPower(Integer:25, Integer:256, ObjectId:0)
// detention center intelligence    SpecialPower(Integer:34, Integer:0, ObjectId:0)
// strategy center bombardment      SpecialPower(Integer:41, Integer:9216, ObjectId:0)
// strategy center hold the line    SpecialPower(Integer:41, Integer:17408, ObjectId:0)
// strategy center search & destroy SpecialPower(Integer:41, Integer:33792, ObjectId:0)
internal static class SpecialPowerApplicator
{
    internal static void Execute(SpecialPowerType specialPower, in SpecialPowerArguments arguments, IReadOnlyCollection<GameObject> selectedObjects)
    {
        Action<SpecialPowerArguments, IReadOnlyCollection<GameObject>> executor = specialPower switch
        {
            SpecialPowerType.RemoteCharges => throw new NotImplementedException(),
            SpecialPowerType.CiaIntelligence => throw new NotImplementedException(),
            SpecialPowerType.ChangeBattlePlans => ChangeBattlePlans,
            _ => throw new ArgumentOutOfRangeException(nameof(specialPower),
                $"{specialPower} is not a supported power"),
        };

        executor(arguments, selectedObjects);
    }

    private static void ChangeBattlePlans(SpecialPowerArguments arguments, IReadOnlyCollection<GameObject> selectedObjects)
    {
        var battlePlan = arguments.OrderFlags.HasFlag(SpecialPowerOrderFlags.OptionOne) ? BattlePlanType.Bombardment :
            arguments.OrderFlags.HasFlag(SpecialPowerOrderFlags.OptionTwo) ? BattlePlanType.HoldTheLine :
            arguments.OrderFlags.HasFlag(SpecialPowerOrderFlags.OptionThree) ? BattlePlanType.SearchAndDestroy :
            BattlePlanType.None;
        selectedObjects.FirstOrDefault()?.FindBehavior<BattlePlanUpdate>()?.ChangeBattlePlan(battlePlan);
    }
}

// var specialPowerDefinitionId = (SpecialPowerType) order.Arguments[0].Value.Integer;
// var commandFlags = (SpecialPowerOrderFlags) order.Arguments[1].Value.Integer;
// var commandCenterSource = order.Arguments[2].Value.ObjectId;
internal readonly record struct SpecialPowerArguments(Player Caller, SpecialPowerOrderFlags OrderFlags);
