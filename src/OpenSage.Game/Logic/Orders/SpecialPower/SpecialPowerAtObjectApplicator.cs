#nullable enable

using System;
using System.Collections.Generic;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.Orders.SpecialPower;

// cash hack                 SpecialPowerAtObject(Integer:14, ObjectId:674, Integer:643, ObjectId:657)
// missile laser lock        SpecialPowerAtObject(Integer:23, ObjectId:700, Integer:771, ObjectId:0)
// tank hunter tnt           SpecialPowerAtObject(Integer:24, ObjectId:6, Integer:259, ObjectId:0)
// burton remote demo charge SpecialPowerAtObject(Integer:25, ObjectId:2, Integer:771, ObjectId:0)
// burton timed demo charge  SpecialPowerAtObject(Integer:26, ObjectId:3, Integer:259, ObjectId:0)
// hacker disable building   SpecialPowerAtObject(Integer:27, ObjectId:8, Integer:259, ObjectId:0)
// lotus capture building    SpecialPowerAtObject(Integer:28, ObjectId:8, Integer:259, ObjectId:0)
// usa ranger capture        SpecialPowerAtObject(Integer:29, ObjectId:8, Integer:323, ObjectId:0)
// china red guard capture   SpecialPowerAtObject(Integer:30, ObjectId:8, Integer:323, ObjectId:0)
// gla rebel capture         SpecialPowerAtObject(Integer:31, ObjectId:8, Integer:323, ObjectId:0)
// lotus disable vehicle     SpecialPowerAtObject(Integer:32, ObjectId:6, Integer:259, ObjectId:0)
// lotus cash hack           SpecialPowerAtObject(Integer:33, ObjectId:674, Integer:259, ObjectId:0)
// bomb truck disguise       SpecialPowerAtObject(Integer:36, ObjectId:1176, Integer:263, ObjectId:0)
internal abstract class SpecialPowerAtObjectApplicator
{
    internal static void Execute(SpecialPowerType specialPower, in ObjectArguments arguments, IReadOnlyCollection<GameObject> selectedObjects)
    {
        Action<ObjectArguments, IReadOnlyCollection<GameObject>> executor =  specialPower switch
        {
            SpecialPowerType.CashHack => ExecuteCashHack,
            SpecialPowerType.MissileDefenderLaserGuidedMissiles => throw new NotImplementedException(),
            SpecialPowerType.TankHunterTntAttack => throw new NotImplementedException(),
            SpecialPowerType.RemoteCharges => throw new NotImplementedException(),
            SpecialPowerType.TimedCharges => throw new NotImplementedException(),
            SpecialPowerType.HackerDisableBuilding => throw new NotImplementedException(),
            SpecialPowerType.BlackLotusCaptureBuilding => throw new NotImplementedException(),
            SpecialPowerType.InfantryCaptureBuilding => throw new NotImplementedException(),
            SpecialPowerType.BlackLotusDisableVehicleHack => throw new NotImplementedException(),
            SpecialPowerType.BlackLotusStealCashHack => throw new NotImplementedException(),
            SpecialPowerType.DisguiseAsVehicle => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(specialPower),
                $"{specialPower} is not a supported object power"),
        };

        executor(arguments, selectedObjects);
    }

    private static void ExecuteCashHack(ObjectArguments arguments, IReadOnlyCollection<GameObject> selectedObjects)
    {
        arguments.CommandCenter?.FindBehavior<CashHackSpecialPower>()?.Activate(arguments.Target);
    }
}

// var specialPowerDefinitionId = (SpecialPowerType) order.Arguments[0].Value.Integer;
// var targetId = order.Arguments[1].Value.ObjectId;
// var commandFlags = (SpecialPowerOrderFlags) order.Arguments[2].Value.Integer;
// var commandCenterSource = order.Arguments[3].Value.ObjectId;
internal readonly record struct ObjectArguments(Player Caller, GameObject Target, SpecialPowerOrderFlags OrderFlags, GameObject? CommandCenter);
