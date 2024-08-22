using OpenSage.Logic.Orders;

namespace OpenSage.Logic.OrderGenerators;

// these are all of the cursors listed in the Mouse.ini files
internal static class Cursors
{
    // generals
    private const string Normal = "Normal"; // standard cursor // bfme ini comment: ; "Normal" is not quite correct, as it shouldn't ever be used.  Just seems to be a spare entry. :P - MDC
    private const string Arrow = "Arrow"; // also standard cursor
    private const string Scroll = "Scroll"; // prefix for sccscroll0-7 - used when right-click scrolling
    private const string Target = "Target"; // todo: what is this?
    private const string Move = "Move"; // unit move to location
    private const string AttackMove = "AttackMove"; // attack move ability
    private const string AttackObj = "AttackObj"; // attack an object
    private const string ForceAttackObj = "ForceAttackObj"; // force attack an object
    private const string ForceAttackGround = "ForceAttackGround"; // force attack the ground
    private const string Select = "Select"; // select a unit
    private const string GenericInvalid = "GenericInvalid"; // invalid cursor when no other cursor applies?
    private const string EnterFriendly = "EnterFriendly"; // garrison building, enter transport, etc
    private const string EnterAggressive = "EnterAggressive"; // combat drop
    private const string CaptureBuilding = "CaptureBuilding"; // ranger/rebel/red guard capture
    private const string SnipeVehicle = "SnipeVehicle"; // kell snipe vehicle
    private const string FireBomb = "FireBomb"; // "hidden" special power in generals
    private const string Defector = "Defector"; // "hidden" special power in generals
    private const string LaserGuidedMissiles = "LaserGuidedMissiles"; // usa missile defender ability
    private const string TankHunterTntAttack = "TankHunterTNTAttack"; // china tank hunter ability
    private const string StabAttack = "StabAttack"; // colonel burton ability
    private const string Hack = "Hack"; // black lotus/hacker ability
    private const string StabAttackInvalid = "StabAttackInvalid"; // colonel burton ability (invalid)
    private const string PlaceRemoteCharge = "PlaceRemoteCharge"; // colonel burton ability
    private const string PlaceTimedCharge = "PlaceTimedCharge"; // colonel burton ability
    private const string PlaceChargeInvalid = "PlaceChargeInvalid"; // colonel burton ability (invalid)
    private const string SetRallyPoint = "SetRallyPoint"; // set a rally point for a building
    private const string Dock = "Dock"; // todo: what is this? uses Enter cursor icon
    private const string GetRepaired = "GetRepaired"; // send vehicle back to war factory to repair
    private const string GetHealed = "GetHealed"; // todo: what is this?
    private const string DoRepair = "DoRepair"; // dozer repair structure
    private const string ResumeConstruction = "ResumeConstruction"; // dozer resume construction of incomplete structure
    private const string FireFlame = "FireFlame"; // dragon tank ability
    private const string PlaceBeacon = "PlaceBeacon"; // place beacon in multiplayer game
    private const string DisguiseAsVehicle = "DisguiseAsVehicle"; // bomb truck ability
    private const string Waypoint = "Waypoint"; // add new pathing waypoint
    private const string OutRange = "OutRange"; // todo: what is this?
    private const string ParticleUplinkCannon = "ParticleUplinkCannon"; // particle cannon ability

    // bfme
    private const string Axe = "Axe";
    private const string EvilAbilityObj = "EvilAbilityObj";
    private const string LightPointParticle = "LightPointParticle";
    private const string LivingWorldZoom = "LivingWorldZoom";
    private const string WeaponUpgrade = "WeaponUpgrade";
    private const string ArmorUpgrade = "ArmorUpgrade";
    private const string JoinHorde = "JoinHorde";
    private const string Beam = "Beam";
    private const string Bombard = "Bombard";
    private const string PickUp = "PickUp";

    // bfme2
    private const string SendToDeath = "SendToDeath";
    private const string DeliverRing = "DeliverRing";
    private const string Patrol = "Patrol";

    /// <summary>
    /// Gets the cursor that should be used for a given order. This does not work for special powers, which have their own cursors.
    /// </summary>
    public static string CursorForOrder(OrderType? orderType)
    {
        return orderType switch
        {
            OrderType.Zero => Arrow,
            OrderType.SetSelection => Select,
            OrderType.SetRallyPoint => SetRallyPoint,
            OrderType.AttackObject => AttackObj,
            OrderType.ForceAttackObject => ForceAttackObj,
            OrderType.ForceAttackGround => ForceAttackGround,
            OrderType.RepairVehicle => GetRepaired,
            OrderType.ResumeBuild => ResumeConstruction,
            OrderType.RepairStructure => DoRepair,
            OrderType.Enter => EnterFriendly,
            OrderType.GatherDumpSupplies => EnterFriendly,
            OrderType.MoveTo => Move,
            OrderType.AttackMove => AttackMove,
            OrderType.AddWaypoint => Waypoint,
            OrderType.DirectParticleCannon => ParticleUplinkCannon,
            _ => GenericInvalid,
        };
    }
}
