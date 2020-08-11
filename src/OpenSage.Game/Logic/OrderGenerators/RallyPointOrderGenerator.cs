using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Input;
using OpenSage.Logic.Orders;

namespace OpenSage.Logic.OrderGenerators
{
    // TODO: Cancel this when:
    // 1. Structure dies
    // 2. We lose access to the building
    public sealed class RallyPointOrderGenerator : IOrderGenerator
    {
        private Vector3 _position;

        public bool CanDrag { get; } = true;

        public RallyPointOrderGenerator()
        {
        }

        public void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime)
        {
        }

        public OrderGeneratorResult TryActivate(Scene3D scene, KeyModifiers keyModifiers)
        {
            var playerId = scene.GetPlayerIndex(scene.LocalPlayer);
            var objectIds = scene.GameObjects.GetObjectIds(scene.LocalPlayer.SelectedUnits);
            var order = Order.CreateSetRallyPointOrder(playerId, objectIds, _position);

            return OrderGeneratorResult.SuccessAndContinue(new[] { order });
        }

        public void UpdatePosition(Vector2 mousePosition, Vector3 worldPosition)
        {
            _position = worldPosition;
        }

        public void UpdateDrag(Vector3 position)
        {
        }

        public string GetCursor(KeyModifiers keyModifiers) => "SetRallyPoint";
    }
}
