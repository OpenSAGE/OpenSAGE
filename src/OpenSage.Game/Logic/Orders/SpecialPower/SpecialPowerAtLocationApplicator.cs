#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.Orders.SpecialPower;

// fuel air bomb             SpecialPowerAtLocation(Integer:2, Position:<1217.1951, 1874.8988, 18.75>, ObjectId:0, Integer:672, ObjectId:657)
// paradrop                  SpecialPowerAtLocation(Integer:3, Position:<1128.1514, 705.649, 18.74997>, ObjectId:0, Integer:672, ObjectId:657)
// cluster mines             SpecialPowerAtLocation(Integer:5, Position:<1346.773, 2096.985, 18.75>, ObjectId:1243, Integer:672, ObjectId:657)
// emp bomb                  SpecialPowerAtLocation(Integer:6, Position:<1603.1487, 1894.5223, 18.75>, ObjectId:3661, Integer:672, ObjectId:657)
// a10                       SpecialPowerAtLocation(Integer:8, Position:<1795.539, 1420.2986, 110>, ObjectId:0, Integer:672, ObjectId:657)
// nuclear missile           SpecialPowerAtLocation(Integer:10, Position:<668.3643, 241.7229, 10>, ObjectId:5, Integer:672, ObjectId:0)
// scud storm                SpecialPowerAtLocation(Integer:12, Position:<1432.6141, 1948.8225, 18.75>, ObjectId:0, Integer:544, ObjectId:0)
// artillery barrage         SpecialPowerAtLocation(Integer:13, Position:<1490.2301, 2075.6533, 18.75>, ObjectId:2794, Integer:672, ObjectId:657)
// spy satellite             SpecialPowerAtLocation(Integer:15, Position:<304.26398, 409.33313, 10>, ObjectId:0, Integer:544, ObjectId:0)
// spy drone                 SpecialPowerAtLocation(Integer:16, Position:<825.8123, 606.12665, 10>, ObjectId:0, Integer:928, ObjectId:4)
// radar van scan            SpecialPowerAtLocation(Integer:17, Position:<1508.3314, 1920.3726, 18.75>, ObjectId:0, Integer:864, ObjectId:0)
// rebel ambush              SpecialPowerAtLocation(Integer:20, Position:<1734.9331, 1322.1748, 110>, ObjectId:0, Integer:672, ObjectId:657)
// repair                    SpecialPowerAtLocation(Integer:35, Position:<1105.9589, 728.7699, 18.75>, ObjectId:2816, Integer:672, ObjectId:657)
// particle cannon           SpecialPowerAtLocation(Integer:37, Position:<442.46735, 318.8226, 10>, ObjectId:0, Integer:672, ObjectId:0)
// ambulance cleanup area    SpecialPowerAtLocation(Integer:42, Position:<443.1347, 219.59857, 10>, ObjectId:0, Integer:288, ObjectId:0)
internal class SpecialPowerAtLocationApplicator
{
    internal static void Execute(SpecialPowerType specialPower, in LocationArguments arguments, IReadOnlyCollection<GameObject> selectedObjects)
    {
        Action<LocationArguments, IReadOnlyCollection<GameObject>> executor = specialPower switch
        {
            SpecialPowerType.FuelAirBomb => throw new NotImplementedException(),
            SpecialPowerType.ParadropAmerica => throw new NotImplementedException(),
            SpecialPowerType.ClusterMines => throw new NotImplementedException(),
            SpecialPowerType.EmpPulse => throw new NotImplementedException(),
            SpecialPowerType.A10ThunderboltStrike => throw new NotImplementedException(),
            SpecialPowerType.NuclearMissile => throw new NotImplementedException(),
            SpecialPowerType.ScudStorm => throw new NotImplementedException(),
            SpecialPowerType.ArtilleryBarrage => throw new NotImplementedException(),
            SpecialPowerType.SpySatellite => ExecuteSpySatellite,
            SpecialPowerType.SpyDrone => ExecuteSpyDrone,
            SpecialPowerType.RadarVanScan => ExecuteRadarVanScan,
            SpecialPowerType.Ambush => ExecuteAmbush,
            SpecialPowerType.RepairVehicles => throw new NotImplementedException(),
            SpecialPowerType.ParticleUplinkCannon => throw new NotImplementedException(),
            SpecialPowerType.CleanupArea => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(specialPower),
                $"{specialPower} is not a supported location power"),
        };

        executor(arguments, selectedObjects);
    }

    private static void ExecuteSpySatellite(LocationArguments arguments, IReadOnlyCollection<GameObject> selectedObjects)
    {
        ExecuteOclSpecialPowerFromCommandCenter(SpecialPowerType.SpySatellite, arguments);
    }

    private static void ExecuteSpyDrone(LocationArguments arguments, IReadOnlyCollection<GameObject> selectedObjects)
    {
        ExecuteOclSpecialPowerFromCommandCenter(SpecialPowerType.SpyDrone, arguments);
    }

    private static void ExecuteRadarVanScan(LocationArguments arguments, IReadOnlyCollection<GameObject> selectedObjects)
    {
        ExecuteOclSpecialPowerFromSelectedObject(SpecialPowerType.RadarVanScan, arguments, selectedObjects);
    }

    private static void ExecuteAmbush(LocationArguments arguments, IReadOnlyCollection<GameObject> selectedObjects)
    {
        ExecuteOclSpecialPowerFromCommandCenter(SpecialPowerType.Ambush, arguments);
    }

    private static void ExecuteOclSpecialPowerFromCommandCenter(SpecialPowerType specialPowerType, LocationArguments arguments)
    {
        ExecuteOclSpecialPower(specialPowerType, arguments.CommandCenter, arguments.Location);
    }

    private static void ExecuteOclSpecialPowerFromSelectedObject(SpecialPowerType specialPowerType, LocationArguments arguments, IEnumerable<GameObject> selectedObjects)
    {
        // todo: first object where special power is ready, since we could have multiple selected
        ExecuteOclSpecialPower(specialPowerType, selectedObjects.FirstOrDefault(), arguments.Location);
    }

    private static void ExecuteOclSpecialPower(SpecialPowerType specialPowerType, GameObject? source, Vector3 location)
    {
        source?.FindSpecialPowerBehavior<OCLSpecialPowerModule>(specialPowerType)?.Activate(location);
    }
}

// var specialPowerDefinition = (SpecialPowerType) order.Arguments[0].Value.Integer;
// var specialPowerLocation = order.Arguments[1].Value.Position;
// var unknownObjectId = order.Arguments[2].Value.ObjectId;
// var commandFlags = (SpecialPowerOrderFlags) order.Arguments[3].Value.Integer;
// var commandCenterSource = order.Arguments[4].Value.ObjectId;
internal readonly record struct LocationArguments(Player Caller, Vector3 Location, SpecialPowerOrderFlags OrderFlags, GameObject? CommandCenter);
