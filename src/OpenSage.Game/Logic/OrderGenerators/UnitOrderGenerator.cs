using System.Linq;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;

namespace OpenSage.Logic.OrderGenerators
{
    internal sealed class UnitOrderGenerator(Game game) : OrderGenerator(game)
    {
        public override bool CanDrag => false;

        public override string GetCursor(KeyModifiers keyModifiers)
        {
            if (LocalPlayer == null || SelectedUnits == null || SelectedUnits.Count == 0)
            {
                return WorldObject != null
                    ? "Select" // TODO: Maybe shouldn't have this here.
                    : "Arrow";
            }

            if (keyModifiers.HasFlag(KeyModifiers.Ctrl))
            {
                return WorldObject != null
                    ? "ForceAttackObj"
                    : "ForceAttackGround";
            }

            if (WorldObject != null)
            {
                // TODO: Should take allies into account.
                if (WorldObject.Owner != LocalPlayer)
                {
                    if (SelectedUnits.Any(u => u.IsKindOf(ObjectKinds.Harvester)) &&
                        WorldObject.IsKindOf(ObjectKinds.SupplySource))
                    {
                        // always take this order, even if the harvester is full
                        return "EnterFriendly";
                    }

                    return "AttackObj";
                }

                if (WorldObject.Definition.KindOf.Get(ObjectKinds.Transport))
                {
                    // TODO: Check if transport is full.
                    return "EnterFriendly";
                }

                if (SelectedUnits.Any(u => u.IsKindOf(ObjectKinds.Harvester)) &&
                         SelectedUnits.Any(u => u.Supply > 0) &&
                         LocalPlayer.SupplyManager.Contains(WorldObject))
                {
                    return "EnterFriendly";
                }

                return "Select";
            }

            return "Move";
        }

        public override OrderGeneratorResult TryActivate(Scene3D scene, KeyModifiers keyModifiers)
        {
            if (scene.LocalPlayer.SelectedUnits.Count == 0)
            {
                return OrderGeneratorResult.Inapplicable();
            }

            Order order;

            var unit = scene.LocalPlayer.SelectedUnits.Last();

            // TODO: Use ini files for this, don't hardcode it.
            if (keyModifiers.HasFlag(KeyModifiers.Ctrl))
            {
                if (WorldObject != null)
                {
                    order = Order.CreateAttackObject(scene.GetPlayerIndex(scene.LocalPlayer), WorldObject.ID, true);
                }
                else
                {
                    order = Order.CreateAttackGround(scene.GetPlayerIndex(scene.LocalPlayer), WorldPosition);
                }
            }
            else
            {
                if (WorldObject != null)
                {
                    // TODO: Should take allies and neutrals (like supply depots) into account.
                    if (WorldObject.Owner != LocalPlayer)
                    {
                        if (unit.IsKindOf(ObjectKinds.Harvester) && WorldObject.IsKindOf(ObjectKinds.SupplySource))
                        {
                            // always take this order, even if the harvester is full
                            order = Order.CreateSupplyGatherDump(scene.GetPlayerIndex(scene.LocalPlayer), WorldObject.ID);
                        }
                        else
                        {
                            order = Order.CreateAttackObject(scene.GetPlayerIndex(scene.LocalPlayer), WorldObject.ID, false);
                        }
                    }
                    else if (WorldObject.Definition.KindOf.Get(ObjectKinds.Transport))
                    {
                        // SoundEnter
                        // VoiceEnter
                        // TODO: Also need to check TransportSlotCount, Slots, etc.
                        order = Order.CreateEnter(scene.GetPlayerIndex(scene.LocalPlayer), WorldObject.ID);
                    }
                    else if (unit.IsKindOf(ObjectKinds.Harvester) && unit.Supply > 0 && scene.LocalPlayer.SupplyManager.Contains(WorldObject))
                    {
                        order = Order.CreateSupplyGatherDump(scene.GetPlayerIndex(scene.LocalPlayer), WorldObject.ID);
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
                    order = Order.CreateMoveOrder(scene.GetPlayerIndex(scene.LocalPlayer), WorldPosition);
                }
            }

            return OrderGeneratorResult.SuccessAndContinue(new[] { order });
        }
    }
}
