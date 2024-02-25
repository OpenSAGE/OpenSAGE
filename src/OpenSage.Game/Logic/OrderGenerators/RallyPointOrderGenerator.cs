using System.Collections.Generic;
using OpenSage.Input;
using OpenSage.Logic.Orders;

namespace OpenSage.Logic.OrderGenerators
{
    // TODO: Cancel this when:
    // 1. Structure dies
    // 2. We lose access to the building
    public sealed class RallyPointOrderGenerator(Game game) : OrderGenerator(game)
    {
        public override bool CanDrag => true;

        private OrderType _currentOrder = OrderType.Zero;

        public override OrderGeneratorResult TryActivate(Scene3D scene, KeyModifiers keyModifiers)
        {
            var playerId = scene.GetPlayerIndex(LocalPlayer);

            if (_currentOrder is OrderType.SetSelection)
            {
                if (WorldObject == null)
                {
                    throw new InvalidStateException("World object null for set selection order");
                }

                var setSelectionOrder = Order.CreateSetSelection(playerId, WorldObject.ID);

                return OrderGeneratorResult.SuccessAndContinue(new[] { setSelectionOrder });
            }

            if (SelectedUnits == null)
            {
                throw new InvalidStateException("Local player not present when setting rally point");
            }

            var objectIds = new List<uint>();
            foreach (var gameObject in SelectedUnits)
            {
                objectIds.Add(gameObject.ID);
            }

            var order = Order.CreateSetRallyPointOrder(playerId, objectIds, WorldPosition);

            return OrderGeneratorResult.SuccessAndContinue(new[] { order });
        }

        public override string GetCursor(KeyModifiers keyModifiers)
        {
            _currentOrder = GetCurrentOrder();
            return Cursors.CursorForOrder(_currentOrder);
        }

        private OrderType GetCurrentOrder()
        {
            return WorldObject != null ? OrderType.SetSelection : OrderType.SetRallyPoint;
        }
    }
}
