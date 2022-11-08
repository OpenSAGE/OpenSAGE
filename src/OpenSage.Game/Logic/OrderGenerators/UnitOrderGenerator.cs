using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;
using FixedMath.NET;

namespace OpenSage.Logic.OrderGenerators
{
    internal sealed class UnitOrderGenerator : IOrderGenerator
    {
        private readonly Game _game;

        private Vector3 _worldPosition;
        private GameObject _worldObject;

        private IReadOnlyCollection<GameObject> SelectedUnits => _game.Scene3D.LocalPlayer.SelectedUnits;
        private Player LocalPlayer => _game.Scene3D.LocalPlayer;

        public bool CanDrag { get; } = false;

        public UnitOrderGenerator(Game game)
        {
            _game = game;
        }

        public void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime) { }

        private string GetCursorForForceFire()
        {
            if (SelectedUnitsIsStructure(out var s) && s.CanAttack)
            {
                // TODO: check range of target, or range to ground position if target is null
                const bool targetIsInRange = true;
                return targetIsInRange ? _worldObject is null ? "ForceAttackGround" : "ForceAttackObj" : "GenericInvalid";
            }

            if (_worldObject is null)
            {
                return "ForceAttackGround";
            }

            if (SelectedUnits.Count == 1 && SelectedUnits.Single() == _worldObject)
            {
                return "Select";
            }

            return AnyUnitCanAttackTarget(_worldObject) ? "AttackObj" : "GenericInvalid";
        }

        private static string GetCursorForWaypointModifier()
        {
            // pretty much just always return waypoint as long as we own the unit selected
            return "Waypoint";
        }

        public string GetCursor(KeyModifiers keyModifiers)
        {
            var selectedUnits = _game.Scene3D.LocalPlayer?.SelectedUnits;
            if (selectedUnits is not null && (selectedUnits.Count == 0 ||
               selectedUnits.All(u => u.Owner != LocalPlayer)))
            {
                return _worldObject != null
                    ? "Select" // TODO: Maybe shouldn't have this here.
                    : "Arrow";
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
                return "Move";
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
                return "CaptureBuilding";
            }

            if (TargetIsGarrisonable(target, out var containModuleData))
            {
                return GetCursorForGarrisonableTarget(target, containModuleData);
            }

            if (SelectedUnits.Any(u => u.Definition.KindOf.Get(ObjectKinds.Dozer)))
            {
                if (GameObjectIsStructure(target))
                {
                    // todo: command centers that spawn at the beginning of the game spawn with BuildProgress = 0
                    if (TargetIsPlayerOwned(target) && target.BuildProgress < 1f && !target.IsBeingConstructed())
                    {
                        return "ResumeConstruction";
                    }

                    if (target.HealthPercentage < Fix64.One)
                    {
                        return "GetRepaired";
                    }
                }
            }

            // TODO: if target can heal unit and isn't full (e.g. barracks, airfield, war factory) show **EnterFriendly**

            return "Select";
        }

        private string GetCursorForStructureSelected(GameObject structure)
        {
            // the selected object is a player-owned structure
            if (!TryGetTarget(out var structureTarget))
            {
                if (StructureHasRallyPointAbility(structure))
                {
                    return "SetRallyPoint";
                }

                return "GenericInvalid";
            }

            // target exists
            if (!TargetIsEnemy(structureTarget))
            {
                return "Select";
            }

            // target is enemy
            if (StructureCanAttackTarget(structureTarget))
            {
                return "AttackObj";
            }

            return "GenericInvalid";
        }

        private string GetCursorForTerrainTarget()
        {
            if (!TerrainUnderTargetIsImpassable())
            {
                return "Move";
            }

            // note that in Generals, we always show the Invalid cursor at this point
            // in Zero hour, we show Move if any unit has a cliff locomotor
            // terrain is impassable
            if (AnySelectedUnitCanTraverseCliffs())
            {
                return "Move";
            }

            return "GenericInvalid";
        }

        private string GetCursorForEnemyTarget(GameObject target)
        {
            // target is enemy
            if (AnyUnitCanAttackTarget(target))
            {
                return "AttackObj";
            }

            // TODO: black lotus/hacker abilities?

            return "GenericInvalid";
        }

        private string GetCursorForGarrisonableTarget(GameObject target, OpenContainModuleData containModuleData)
        {
            if (AnySelectedUnitCanGarrisonTarget(target, containModuleData))
            {
                return "EnterFriendly";
            }

            return "Select";
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

        // todo
        private static bool StructureHasRallyPointAbility(GameObject structure)
        {
            return true;
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

        // todo
        private bool TerrainUnderTargetIsImpassable()
        {
            // this throws index out of range exceptions
            // _game.Scene3D.Terrain.Map.BlendTileData.Impassability[(int) _worldPosition.X, (int) _worldPosition.Y];
            return false;
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
        private static bool TargetIsGarrisonable(GameObject target, [NotNullWhen(true)] out OpenContainModuleData containModuleData)
        {
            containModuleData = default;
            if (target.Definition.Behaviors.Values.FirstOrDefault(b =>
                        b.Data.ModuleKinds.HasFlag(ModuleKinds.Contain) &&
                        b.Data is GarrisonContainModuleData || b.Data is TransportContainModuleData)
                    .Data is OpenContainModuleData data)
            {
                containModuleData = data;
                return true;
            }

            return false;
        }

        private bool AnySelectedUnitCanGarrisonTarget(GameObject target, OpenContainModuleData garrisonData)
        {
            return TargetGarrisonIsNotFull(target) &&
                   SelectedUnits.Select(u => u.Definition.KindOf)
                       .Any(k => garrisonData.AllowInsideKindOf?.Intersects(k) ?? true);
        }

        // todo
        private bool TargetGarrisonIsNotFull(GameObject target)
        {
            return true;
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
                    else if (_worldObject.Definition.KindOf.Get(ObjectKinds.Transport))
                    {
                        // SoundEnter
                        // VoiceEnter
                        // TODO: Also need to check TransportSlotCount, Slots, etc.
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
}
