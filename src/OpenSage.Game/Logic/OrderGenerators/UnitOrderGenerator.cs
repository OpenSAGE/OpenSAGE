using System.Linq;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;

namespace OpenSage.Logic.OrderGenerators
{
    internal sealed class UnitOrderGenerator : IOrderGenerator
    {
        private readonly Game _game;

        private Vector3 _worldPosition;
        private GameObject _worldObject;

        public bool CanDrag { get; } = false;

        public UnitOrderGenerator(Game game)
        {
            _game = game;
        }

        public void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime) { }

        public string GetCursor(KeyModifiers keyModifiers)
        {
            if (_game.Scene3D.LocalPlayer?.SelectedUnits.Count == 0)
            {
                return _worldObject != null
                    ? "Select" // TODO: Maybe shouldn't have this here.
                    : "Arrow";
            }

            if (keyModifiers.HasFlag(KeyModifiers.Ctrl))
            {
                return _worldObject != null
                    ? "ForceAttackObj"
                    : "ForceAttackGround";
            }

            if (_worldObject != null)
            {
                // TODO: Should take allies into account.
                if (_worldObject.Owner != _game.Scene3D.LocalPlayer)
                {
                    if (_game.Scene3D.LocalPlayer.SelectedUnits.Any(u => u.IsKindOf(ObjectKinds.Harvester)) &&
                        _worldObject.IsKindOf(ObjectKinds.SupplySource))
                    {
                        // always take this order, even if the harvester is full
                        return "EnterFriendly";
                    }

                    return "AttackObj";
                }

                if (_worldObject.Definition.KindOf.Get(ObjectKinds.Transport))
                {
                    // TODO: Check if transport is full.
                    return "EnterFriendly";
                }

                if (_game.Scene3D.LocalPlayer.SelectedUnits.Any(u => u.IsKindOf(ObjectKinds.Harvester)) &&
                         _game.Scene3D.LocalPlayer.SelectedUnits.Any(u => u.Supply > 0) &&
                         _game.Scene3D.LocalPlayer.SupplyManager.Contains(_worldObject))
                {
                    return "EnterFriendly";
                }

                return "Select";
            }

            return "Move";
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
                        if (unit.IsKindOf(ObjectKinds.Harvester) && _worldObject.IsKindOf(ObjectKinds.SupplySource))
                        {
                            // always take this order, even if the harvester is full
                            order = Order.CreateSupplyGatherDump(scene.GetPlayerIndex(scene.LocalPlayer), _worldObject.ID);
                        }
                        else
                        {
                            // TODO: handle hordes properly
                            unit.OnLocalAttack(_game.Audio);
                            order = Order.CreateAttackObject(scene.GetPlayerIndex(scene.LocalPlayer), _worldObject.ID, false);
                        }
                    }
                    else if (_worldObject.Definition.KindOf.Get(ObjectKinds.Transport))
                    {
                        // SoundEnter
                        // VoiceEnter
                        // TODO: Also need to check TransportSlotCount, Slots, etc.
                        order = Order.CreateEnter(scene.GetPlayerIndex(scene.LocalPlayer), _worldObject.ID);
                    }
                    else if (unit.IsKindOf(ObjectKinds.Harvester) && unit.Supply > 0 && scene.LocalPlayer.SupplyManager.Contains(_worldObject))
                    {
                        order = Order.CreateSupplyGatherDump(scene.GetPlayerIndex(scene.LocalPlayer), _worldObject.ID);
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
