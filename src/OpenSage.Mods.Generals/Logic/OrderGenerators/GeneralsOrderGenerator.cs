using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using FixedMath.NET;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Input;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Logic.OrderGenerators;
using OpenSage.Logic.Orders;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.Mods.Generals.Logic.OrderGenerators;

internal sealed class GeneralsOrderGenerator : IOrderGenerator
{
    private readonly Game _game;

    private Vector3 _worldPosition;
    private GameObject _worldObject;

    private IReadOnlyCollection<GameObject> SelectedUnits => LocalPlayer.SelectedUnits;
    private Player LocalPlayer => _game.PlayerManager.LocalPlayer;

    public bool CanDrag { get; } = false;

    public GeneralsOrderGenerator(Game game)
    {
        _game = game;
    }

    public void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime) { }

    private string GetCursorForForceFire()
    {
        if (SelectedUnitsIsStructure(out var s) && s.CanAttack)
        {
            return GetCursorForAttackingStructure(s);
        }

        if (_worldObject is null)
        {
            return Cursors.ForceAttackGround;
        }

        if (SelectedUnits.Count == 1 && SelectedUnits.Single() == _worldObject)
        {
            return Cursors.Select;
        }

        return AnyUnitCanAttackTarget(_worldObject) ? Cursors.AttackObj : Cursors.GenericInvalid;
    }

    private static string GetCursorForWaypointModifier()
    {
        // pretty much just always return waypoint as long as we own the unit selected
        return Cursors.Waypoint;
    }

    public string GetCursor(KeyModifiers keyModifiers)
    {
        var selectedUnits = _game.Scene3D.LocalPlayer?.SelectedUnits;
        if (selectedUnits is not null && (selectedUnits.Count == 0 ||
                                          selectedUnits.All(u => u.Owner != LocalPlayer)))
        {
            return _worldObject != null
                ? Cursors.Select // TODO: Maybe shouldn't have this here.
                : Cursors.Arrow;
        }

        // the local player has selected unit(s), and they are the owner of those unit(s)

        if (keyModifiers.HasFlag(KeyModifiers.Alt))
        {
            return GetCursorForWaypointModifier();
        }

        if (keyModifiers.HasFlag(KeyModifiers.Ctrl))
        {
            return GetCursorForForceFire();
        }

        // no modifier key has been applied
        // SelectedUnits is player-owned

        // TODO: verify a garrisoned civilian structure is in fact player-owned
        if (SelectedUnitsIsStructure(out var s))
        {
            return GetCursorForStructureSelected(s);
        }

        // the selected object is a player-owned unit (not structure)
        if (!TerrainUnderTargetIsRevealed())
        {
            // In Zero Hour, if terrain is under fog of war we always return the Move cursor, no matter what
            // arguably we could improve upon this by looking at the terrain under the cursor, and ignoring any world objects
            return Cursors.Move;
        }

        // area is not under fog of war
        if (!TryGetTarget(out var target))
        {
            return GetCursorForTerrainTarget();
        }

        // target is some game object
        if (TargetIsEnemy(target))
        {
            return GetCursorForEnemyTarget(target);
        }

        // target is not enemy
        if (AnySelectedUnitCanCaptureTarget(target))
        {
            return Cursors.CaptureBuilding;
        }

        if (TargetIsGarrisonable(target))
        {
            return GetCursorForGarrisonableTarget(target);
        }

        if (GameObjectIsStructure(target))
        {
            if (SelectedUnits.Any(u => u.Definition.KindOf.Get(ObjectKinds.Dozer)))
            {
                // todo: command centers that spawn at the beginning of the game spawn with BuildProgress = 0
                if (TargetIsPlayerOwned(target) && target.BuildProgress < 1f && !target.IsBeingConstructed())
                {
                    return Cursors.ResumeConstruction;
                }

                if (target.HealthPercentage < Fix64.One)
                {
                    return Cursors.GetRepaired; // has priority over units garrisoning structure
                }
            }

            if (StructureCanHealAnySelectedUnit(target))
            {
                return Cursors.EnterFriendly;
            }
        }

        return Cursors.Select;
    }

    private bool StructureCanHealAnySelectedUnit(GameObject structure)
    {
        var allKinds = SelectedUnits
            .Where(u => u.Health < Fix64.One)
            .Select(u => u.Definition.KindOf)
            .Aggregate(new BitArray<ObjectKinds>(), (s, t) => s | t);

        return StructureCanHealVehicles(structure) && allKinds.Get(ObjectKinds.Vehicle) ||
               StructureCanHealInfantry(structure) && allKinds.Get(ObjectKinds.Infantry) ||
               StructureCanHealAircraft(structure) && allKinds.Get(ObjectKinds.Aircraft);
    }

    private bool StructureCanHealVehicles(GameObject structure)
    {
        return TargetIsFriendly(structure) && structure.FindBehavior<RepairDockUpdate>() is not null;
    }

    private bool StructureCanHealInfantry(GameObject structure)
    {
        return TargetIsPlayerOwned(structure) && structure.FindBehavior<HealContain>() is not null &&
               AnySelectedUnitCanGarrisonTarget(structure);
    }

    private bool StructureCanHealAircraft(GameObject structure)
    {
        // todo: doesn't account for parking slots for fixed-wings and I'm not sure this is even correct for rotary-wings either
        return TargetIsFriendly(structure) && structure.Definition.KindOf.Get(ObjectKinds.FSAirfield);
    }

    private string GetCursorForAttackingStructure(GameObject structure)
    {
        return TargetIsInRangeOfStructure(structure)
            ? _worldObject is null ? Cursors.ForceAttackGround : Cursors.ForceAttackObj
            : Cursors.GenericInvalid;
    }

    private bool TargetIsInRangeOfStructure(GameObject structure)
    {
        // TODO: check range of target, or range to ground position if target is null
        return true;
    }

    private string GetCursorForStructureSelected(GameObject structure)
    {
        // the selected object is a player-owned structure
        if (!TryGetTarget(out var structureTarget))
        {
            if (StructureHasRallyPointAbility(structure))
            {
                return Cursors.SetRallyPoint;
            }

            return Cursors.GenericInvalid;
        }

        // target exists
        if (!TargetIsEnemy(structureTarget))
        {
            return Cursors.Select;
        }

        // target is enemy
        if (StructureCanAttackTarget(structureTarget))
        {
            return Cursors.AttackObj;
        }

        return Cursors.GenericInvalid;
    }

    private string GetCursorForTerrainTarget()
    {
        if (!TerrainUnderTargetIsImpassable())
        {
            return Cursors.Move;
        }

        // note that in Generals, we always show the Invalid cursor at this point
        // in Zero hour, we show Move if any unit has a cliff locomotor
        // terrain is impassable
        if (AnySelectedUnitCanTraverseCliffs())
        {
            return Cursors.Move;
        }

        return Cursors.GenericInvalid;
    }

    private string GetCursorForEnemyTarget(GameObject target)
    {
        // target is enemy
        if (AnyUnitCanAttackTarget(target))
        {
            return Cursors.AttackObj;
        }

        // TODO: black lotus/hacker abilities?

        return Cursors.GenericInvalid;
    }

    private string GetCursorForGarrisonableTarget(GameObject target)
    {
        return AnySelectedUnitCanGarrisonTarget(target) ? Cursors.EnterFriendly : Cursors.Select;
    }

    private bool SelectedUnitsIsStructure([NotNullWhen(true)] out GameObject structure)
    {
        structure = default;

        if (SelectedUnits.Count > 1)
        {
            return false;
        }

        structure = SelectedUnits.SingleOrDefault(GameObjectIsStructure);

        return structure is not null;
    }

    private static bool GameObjectIsStructure(GameObject obj)
    {
        return obj.Definition.KindOf.Get(ObjectKinds.Structure);
    }

    private static bool StructureHasRallyPointAbility(GameObject structure)
    {
        return structure.Definition.KindOf.Get(ObjectKinds.AutoRallyPoint);
    }

    private bool TryGetTarget(out GameObject target)
    {
        target = _worldObject;
        return target is not null;
    }

    private bool StructureCanAttackTarget(GameObject target)
    {
        // TODO: determine if target is in range
        const bool targetIsInRange = true; // only matters for structures

        // todo: consider the abilities of the units inside the structure
        return targetIsInRange && AnyUnitCanAttackTarget(target);
    }

    private bool AnyUnitCanAttackTarget(GameObject target)
    {
        return SelectedUnits.Any(unit => unit != target && unit.CanAttackObject(target, out _));
    }

    private bool TargetIsEnemy(GameObject target)
    {
        return LocalPlayer.Enemies.Contains(target.Owner);
    }

    private bool TargetIsFriendly(GameObject target)
    {
        return LocalPlayer.Allies.Contains(target.Owner);
    }

    private bool TargetIsPlayerOwned(GameObject target)
    {
        return target.Owner == LocalPlayer;
    }

    private bool TargetIsNeutral(GameObject target)
    {
        return !TargetIsPlayerOwned(target) &&
               !TargetIsFriendly(target) &&
               !TargetIsEnemy(target);
    }

    // todo: update when fog of war is added
    private bool TerrainUnderTargetIsRevealed()
    {
        return true;
    }

    private bool TerrainUnderTargetIsImpassable()
    {
        var borderWidth = _game.Scene3D.Terrain.Map.HeightMapData.BorderWidth;
        var xSize = _game.Scene3D.Terrain.Map.BlendTileData.Impassability.GetLength(0) - 1;
        var ySize = _game.Scene3D.Terrain.Map.BlendTileData.Impassability.GetLength(1) - 1;
        var xCoord = (int)Math.Max(_worldPosition.X / HeightMap.HorizontalScale + borderWidth, 0);
        var yCoord = (int)Math.Max(_worldPosition.Y / HeightMap.HorizontalScale + borderWidth, 0);
        return _game.Scene3D.Terrain.Map.BlendTileData.Impassability[Math.Min(xCoord, xSize), Math.Min(yCoord, ySize)];
    }

    private bool AnySelectedUnitCanTraverseCliffs()
    {
        return SelectedUnits.Any(u =>
            u.Definition.LocomotorSets.Values
                .SelectMany(t => t.Locomotors)
                .Select(l => l.Value)
                .Any(l => l.Surfaces.HasFlag(Surfaces.Cliff)));
    }

    private bool AnySelectedUnitCanCapture()
    {
        // todo: check if unit capture ability is ready
        return SelectedUnits.Any(u =>
            u.Definition.Behaviors.Values.Any(b =>
                b.Data.ModuleKinds.HasFlag(ModuleKinds.SpecialPower) &&
                b.Data is SpecialAbilityUpdateModuleData s &&
                s.SpecialPowerTemplate.EndsWith("CaptureBuilding")));
    }

    private bool TargetIsCapturable(GameObject target)
    {
        return target.Definition.KindOf.Get(ObjectKinds.Capturable) && (TargetIsEnemy(target) || TargetIsNeutral(target));
    }

    private bool AnySelectedUnitCanCaptureTarget(GameObject target)
    {
        return TargetIsCapturable(target) && AnySelectedUnitCanCapture();
    }

    // todo: what about upgrades like helix or overlord bunker?
    private static bool TargetIsGarrisonable(GameObject target)
    {
        return target.FindBehavior<OpenContainModule>() is not null;
    }

    private bool AnySelectedUnitCanGarrisonTarget(GameObject target)
    {
        var behavior = target.FindBehavior<OpenContainModule>();

        return behavior is not null && SelectedUnits.Any(behavior.CanAddContained);
    }

    public OrderGeneratorResult TryActivate(Scene3D scene, KeyModifiers keyModifiers)
    {
        if (scene.LocalPlayer.SelectedUnits.Count == 0)
        {
            return OrderGeneratorResult.Inapplicable();
        }

        Order order;

        // We choose the sound based on the most-recently-selected unit.
        var unit = scene.LocalPlayer.SelectedUnits.Last();

        // TODO: Use ini files for this, don't hardcode it.
        if (keyModifiers.HasFlag(KeyModifiers.Ctrl))
        {
            // TODO: Check whether clicked point is an object, or empty ground.
            // TODO: handle hordes properly
            unit.OnLocalAttack(_game.Audio);
            if (_worldObject != null)
            {
                order = Order.CreateAttackObject(scene.GetPlayerIndex(scene.LocalPlayer), _worldObject.ID, true);
            }
            else
            {
                order = Order.CreateAttackGround(scene.GetPlayerIndex(scene.LocalPlayer), _worldPosition);
            }
        }
        else
        {
            if (_worldObject != null)
            {
                // TODO: Should take allies into account.
                if (_worldObject.Owner != _game.Scene3D.LocalPlayer)
                {
                    // TODO: handle hordes properly
                    unit.OnLocalAttack(_game.Audio);
                    order = Order.CreateAttackObject(scene.GetPlayerIndex(scene.LocalPlayer), _worldObject.ID, false);
                }
                else if (AnySelectedUnitCanGarrisonTarget(_worldObject))
                {
                    // SoundEnter
                    // VoiceEnter
                    // todo: limit order to only those which can enter
                    order = Order.CreateEnter(scene.GetPlayerIndex(scene.LocalPlayer), _worldObject.ID);
                }
                else
                {
                    return OrderGeneratorResult.Inapplicable();
                }
            }
            else
            {
                // TODO: Check whether at least one of the selected units can actually be moved.
                // TODO: handle hordes properly
                unit.OnLocalMove(_game.Audio);
                order = Order.CreateMoveOrder(scene.GetPlayerIndex(scene.LocalPlayer), _worldPosition);
            }
        }

        return OrderGeneratorResult.SuccessAndContinue(new[] { order });
    }

    public void UpdateDrag(Vector3 position)
    {

    }

    public void UpdatePosition(Vector2 mousePosition, Vector3 worldPosition)
    {
        _worldPosition = worldPosition;
        _worldObject = _game.Selection.FindClosestObject(mousePosition);
    }
}

// these are all of the cursors that were loaded into MouseCursors when launching Generals
internal static class Cursors
{
    public const string Normal = "Normal";
    public const string Arrow = "Arrow";
    public const string Scroll = "Scroll";
    public const string Target = "Target";
    public const string Move = "Move";
    public const string AttackMove = "AttackMove";
    public const string AttackObj = "AttackObj";
    public const string ForceAttackObj = "ForceAttackObj";
    public const string ForceAttackGround = "ForceAttackGround";
    public const string Select = "Select";
    public const string GenericInvalid = "GenericInvalid";
    public const string EnterFriendly = "EnterFriendly";
    public const string EnterAggressive = "EnterAggressive";
    public const string CaptureBuilding = "CaptureBuilding";
    public const string SnipeVehicle = "SnipeVehicle";
    public const string FireBomb = "FireBomb";
    public const string Defector = "Defector";
    public const string LaserGuidedMissiles = "LaserGuidedMissiles";
    public const string TankHunterTntAttack = "TankHunterTNTAttack";
    public const string StabAttack = "StabAttack";
    public const string Hack = "Hack";
    public const string StabAttackInvalid = "StabAttackInvalid";
    public const string PlaceRemoteCharge = "PlaceRemoteCharge";
    public const string PlaceTimedCharge = "PlaceTimedCharge";
    public const string PlaceChargeInvalid = "PlaceChargeInvalid";
    public const string SetRallyPoint = "SetRallyPoint";
    public const string Dock = "Dock";
    public const string GetRepaired = "GetRepaired";
    public const string GetHealed = "GetHealed";
    public const string DoRepair = "DoRepair";
    public const string ResumeConstruction = "ResumeConstruction";
    public const string FireFlame = "FireFlame";
    public const string PlaceBeacon = "PlaceBeacon";
    public const string DisguiseAsVehicle = "DisguiseAsVehicle";
    public const string Waypoint = "Waypoint";
    public const string OutRange = "OutRange";
    public const string ParticleUplinkCannon = "ParticleUplinkCannon";
}
