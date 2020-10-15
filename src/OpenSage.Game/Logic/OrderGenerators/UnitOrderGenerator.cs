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
            if (_game.Scene3D.LocalPlayer.SelectedUnits.Count == 0)
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

            // TODO: should only work on enemy objects
            return _worldObject != null
                ? "AttackObj"
                : "Move";
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
                    var objectId = scene.GameObjects.GetObjectId(_worldObject);

                    order = Order.CreateAttackObject(scene.GetPlayerIndex(scene.LocalPlayer), (uint) objectId, true);
                }
                else
                {
                    order = Order.CreateAttackGround(scene.GetPlayerIndex(scene.LocalPlayer), _worldPosition);
                }
            }
            else
            {
                // TODO: should only work on enemy objects
                if (_worldObject != null)
                {
                    var objectId = scene.GameObjects.GetObjectId(_worldObject);

                    // TODO: handle hordes properly
                    unit.OnLocalAttack(_game.Audio);
                    order = Order.CreateAttackObject(scene.GetPlayerIndex(scene.LocalPlayer), (uint) objectId, false);
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
