using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic
{
    // TODO: Only for debugging use.
    public sealed class DebugEntityPickerSystem : GameSystem
    {
        private readonly Game _game;

        public DebugEntityPickerSystem(Game game)
            : base(game)
        {
            _game = game;
        }

        public override void Initialize()
        {
            _game.MessageBuffer.Handlers.Insert(0, new DebugEntityPickerMessageHandler(this));
        }

        public bool OnClickPosition(Vector2 position)
        {
            if (Game.Scene3D == null)
            {
                return false;
            }

            var cameraRay = Game.Scene3D.Camera.ScreenPointToRay(position);

            GameObject closestGameObject = null;
            var closestDepth = float.MaxValue;

            foreach (var gameObject in Game.Scene3D.GameObjects.Items)
            {
                var collider = gameObject.Collider;
                if (collider == null || !collider.Intersects(cameraRay, out var depth) || depth > closestDepth)
                {
                    continue;
                }

                closestDepth = depth;
                closestGameObject = gameObject;
            }

            if (closestGameObject == null)
            {
                return false;
            }

            closestGameObject.Transform.Scale = 0.5f;
            return true;
        }
    }
}
