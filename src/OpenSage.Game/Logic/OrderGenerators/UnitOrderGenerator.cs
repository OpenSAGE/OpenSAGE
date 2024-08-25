#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FixedMath.NET;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;
using OpenSage.Mathematics;

namespace OpenSage.Logic.OrderGenerators
{
    internal sealed class UnitOrderGenerator(Game game) : OrderGenerator(game)
    {
        public override bool CanDrag => false;

        private OrderType? _currentOrder = null;

        public override string? GetCursor(KeyModifiers keyModifiers)
        {
            _currentOrder = GetOrderTypeForState(keyModifiers);
            return Cursors.CursorForOrder(_currentOrder);
        }

        public override OrderGeneratorResult TryActivate(Scene3D scene, KeyModifiers keyModifiers)
        {
            var playerId = scene.GetPlayerIndex(scene.LocalPlayer);
            var targetId = WorldObject?.ID ?? 0;

            // TODO: handle hordes properly
            var order = _currentOrder switch
            {
                OrderType.SetSelection => Order.CreateSetSelection(playerId, targetId),
                OrderType.SetRallyPoint => Order.CreateSetRallyPointOrder(playerId, SelectedUnits!.Select(u => u.ID).ToList(), WorldPosition),
                OrderType.AttackObject => Order.CreateAttackObject(playerId, targetId, false),
                OrderType.ForceAttackObject => Order.CreateAttackObject(playerId, targetId, true),
                OrderType.ForceAttackGround => Order.CreateAttackGround(playerId, WorldPosition),
                OrderType.ResumeBuild => Order.CreateResumeBuild(playerId, targetId),
                OrderType.MoveTo => Order.CreateMoveOrder(playerId, WorldPosition),
                OrderType.RepairVehicle => Order.CreateRepairVehicle(playerId, targetId),
                OrderType.RepairStructure => Order.CreateRepairStructure(playerId, targetId),
                OrderType.Enter => Order.CreateEnter(playerId, targetId),
                OrderType.GatherDumpSupplies => Order.CreateSupplyGatherDump(playerId, targetId),
                OrderType.AttackMove => throw new NotImplementedException(),
                OrderType.AddWaypoint => throw new NotImplementedException(),
                _ => null,
            };

            return order != null ? OrderGeneratorResult.SuccessAndContinue([ order ]) : OrderGeneratorResult.Inapplicable();
        }

        private OrderType? GetOrderTypeForState(KeyModifiers keyModifiers)
        {
            if (SelectedUnits is not null && (SelectedUnits.Count == 0 ||
                                              SelectedUnits.All(u => u.Owner != LocalPlayer)))
            {
                return WorldObject != null ? OrderType.SetSelection : OrderType.Zero;
            }

            // the local player has selected unit(s), and they are the owner of those unit(s)

            if (keyModifiers.HasFlag(KeyModifiers.Alt))
            {
                return OrderType.AddWaypoint;
            }

            if (keyModifiers.HasFlag(KeyModifiers.Ctrl))
            {
                return GetOrderForForceFire();
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
                return OrderType.MoveTo;
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

            if (GameObjectIsStructure(target))
            {
                if (SelectedUnits?.Any(u => u.Definition.KindOf.Get(ObjectKinds.Dozer)) == true)
                {
                    if (target.IsBeingConstructed())
                    {
                        if (TargetIsPlayerOwned(target) && ObjectDoesNotHaveOriginalDozerAssigned(target))
                        {
                            return OrderType.ResumeBuild;
                        }
                    }
                    else if (target.HealthPercentage < Fix64.One)
                    {
                        return OrderType.RepairStructure; // has priority over units garrisoning structure
                    }
                }

                if (StructureCanHealAnySelectedUnit(target, out var orderType))
                {
                    return orderType;
                }

                if (SelectedUnits?.Any(u => u.Definition.KindOf.Get(ObjectKinds.Harvester)) == true)
                {
                    if (StructureHasSupplies(target) || // we can give an order to go pick up supplies even if our unit is full
                        (StructureAcceptsSupplies(target) && SelectedUnits?.Any(u => u.FindBehavior<SupplyAIUpdate>()?.CarryingSupplies == true) == true)) // but we can't drop off supplies unless we already have some
                    {
                        return OrderType.GatherDumpSupplies;
                    }
                }
            }

            if (TargetIsEnterable(target))
            {
                return GetCursorForEnterableTarget(target);
            }

            return OrderType.SetSelection;
        }


        private OrderType? GetOrderForForceFire()
        {
            if (SelectedUnitsIsStructure(out var s) && s.CanAttack)
            {
                return GetCursorForForceAttackingStructure(s);
            }

            if (SelectedUnits?.All(u => u.CurrentWeapon == null) != false)
            {
                return null;  // if we have no units or if all our units have no weapons
            }

            if (WorldObject == null)
            {
                return OrderType.ForceAttackGround;
            }

            if (SelectedUnits is {Count: 1} && SelectedUnits.Single() == WorldObject)
            {
                // units can't force-attack themselves
                return OrderType.SetSelection;
            }

            if (AnyUnitCanAttackTarget(WorldObject))
            {
                return OrderType.AttackObject;
            }

            return null;
        }


        /// <summary>
        /// Checks if the most recent builder of the object still has said object as its build target.
        /// </summary>
        private bool ObjectDoesNotHaveOriginalDozerAssigned(GameObject gameObject)
        {
            var builderId = gameObject.BuiltByObjectID;
            var builder = Game.Scene3D.GameObjects.GetObjectById(builderId);
            return builder == null || builder.AIUpdate is IBuilderAIUpdate builderAiUpdate && builderAiUpdate.BuildTarget != gameObject;
        }

        private bool StructureCanHealAnySelectedUnit(GameObject structure, [NotNullWhen(true)] out OrderType? orderType)
        {
            orderType = null;
            var allKinds = SelectedUnits?
                .Where(u => u.Health < Fix64.One)
                .Select(u => u.Definition.KindOf)
                .Aggregate(new BitArray<ObjectKinds>(), (s, t) => s | t);

            if (allKinds == null)
            {
                return false;
            }

            if (StructureCanHealVehicles(structure) && allKinds.Get(ObjectKinds.Vehicle) ||
                StructureCanHealAircraft(structure) && allKinds.Get(ObjectKinds.Aircraft))
            {
                orderType = OrderType.RepairVehicle;
                return true;
            }

            return false;
        }

        private bool StructureCanHealVehicles(GameObject structure)
        {
            return TargetIsFriendly(structure) && structure.HasBehavior<RepairDockUpdate>();
        }

        private bool StructureCanHealAircraft(GameObject structure)
        {
            // todo: doesn't account for parking slots for fixed-wings and I'm not sure this is even correct for rotary-wings either
            return TargetIsFriendly(structure) && structure.Definition.KindOf.Get(ObjectKinds.FSAirfield);
        }

        private OrderType? GetCursorForForceAttackingStructure(GameObject structure)
        {
            if (TargetIsInRangeOfStructure(structure))
            {
                return WorldObject is null ? OrderType.ForceAttackGround : OrderType.ForceAttackObject;
            }

            return null;
        }

        private bool TargetIsInRangeOfStructure(GameObject structure)
        {
            // TODO: check range of target, or range to ground position if target is null
            return true;
        }

        private OrderType? GetCursorForStructureSelected(GameObject structure)
        {
            // the selected object is a player-owned structure
            if (!TryGetTarget(out var structureTarget))
            {
                if (StructureHasRallyPointAbility(structure))
                {
                    return OrderType.SetRallyPoint;
                }

                return null;
            }

            // target exists
            if (!TargetIsEnemy(structureTarget))
            {
                return OrderType.SetSelection;
            }

            // target is enemy
            if (StructureCanAttackTarget(structureTarget))
            {
                return OrderType.AttackObject;
            }

            return null;
        }

        private OrderType? GetCursorForTerrainTarget()
        {
            return !TerrainUnderTargetIsImpassable() || AnySelectedUnitCanTraverseCliffs() ? OrderType.MoveTo : null;
        }

        private OrderType? GetCursorForEnemyTarget(GameObject target)
        {
            return AnyUnitCanAttackTarget(target) ? OrderType.AttackObject : null;
        }

        private OrderType GetCursorForEnterableTarget(GameObject target)
        {
            return AnySelectedUnitCanEnterTarget(target) ? OrderType.Enter : OrderType.SetSelection;
        }

        private bool SelectedUnitsIsStructure([NotNullWhen(true)] out GameObject? structure)
        {
            structure = default;

            if (SelectedUnits is not {Count: 1})
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

        private bool TryGetTarget([NotNullWhen(true)] out GameObject? target)
        {
            target = WorldObject;
            return target is not null;
        }

        private bool StructureCanAttackTarget(GameObject target)
        {
            return TargetIsInRangeOfStructure(target) && AnyUnitCanAttackTarget(target);
        }

        private bool AnyUnitCanAttackTarget(GameObject target)
        {
            return SelectedUnits?.Any(unit => unit != target && unit.CanAttackObject(target)) == true;
        }

        private bool TargetIsEnemy(GameObject target)
        {
            return LocalPlayer?.Enemies.Contains(target.Owner) == true;
        }

        private bool TargetIsFriendly(GameObject target)
        {
            return LocalPlayer?.Allies.Contains(target.Owner) == true;
        }

        private bool TargetIsPlayerOwned(GameObject target)
        {
            return target.Owner == LocalPlayer;
        }

        // todo: update when fog of war is added
        private bool TerrainUnderTargetIsRevealed()
        {
            return true;
        }

        private bool TerrainUnderTargetIsImpassable()
        {
            return Game.Scene3D.Terrain.ImpassableAt(WorldPosition);
        }

        private bool AnySelectedUnitCanTraverseCliffs()
        {
            return SelectedUnits?.Any(u =>
                u.Definition.LocomotorSets.Values
                    .SelectMany(t => t.Locomotors)
                    .Select(l => l.Value)
                    .Any(l => l.Surfaces.HasFlag(Surfaces.Cliff))) == true;
        }

        private static bool StructureHasSupplies(GameObject structure)
        {
            return structure.FindBehavior<SupplyWarehouseDockUpdate>()?.HasBoxes() == true;
        }

        private static bool StructureAcceptsSupplies(GameObject structure)
        {
            return structure.HasBehavior<SupplyCenterDockUpdate>();
        }

        // todo: what about upgrades like helix or overlord bunker? - check is triggered
        private static bool TargetIsEnterable(GameObject target)
        {
            return target.FindBehavior<OpenContainModule>()?.Full == false;
        }

        private bool AnySelectedUnitCanEnterTarget(GameObject target)
        {
            var behavior = target.FindBehavior<OpenContainModule>();

            return behavior != null && SelectedUnits?.Any(behavior.CanAddUnit) == true;
        }
    }
}
