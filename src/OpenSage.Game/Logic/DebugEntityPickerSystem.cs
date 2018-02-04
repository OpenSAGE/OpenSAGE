using System.Collections.Generic;
using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Logic
{
    // TODO: Only for debugging use.
    public sealed class DebugEntityPickerSystem : GameSystem
    {
        private readonly Game _game;
        private readonly List<ColliderComponent> _colliders;

        public DebugEntityPickerSystem(Game game) : base(game)
        {
            _game = game;
            _colliders = new List<ColliderComponent>();
        }

        public override void Initialize()
        {
            _game.Input.MessageBuffer.Handlers.Insert(0, new DebugEntityPickerMessageHandler(this));
        }

        internal override void OnEntityComponentAdded(EntityComponent component)
        {
            base.OnEntityComponentAdded(component);

            if (component is ColliderComponent collider)
            {
                _colliders.Add(collider);
            }
        }

        public bool OnClickPosition(Vector2 position)
        {
            var cameraRay = Game.Scene.Camera.ScreenPointToRay(position);

            ColliderComponent closestCollider = null;
            var closestDepth = float.MaxValue;

            foreach (var collider in _colliders)
            {
                if (!collider.Intersects(cameraRay, out var depth) || depth > closestDepth)
                {
                    continue;
                }

                closestDepth = depth;
                closestCollider = collider;
            }

            if (closestCollider == null)
            {
                return false;
            }

            closestCollider.Transform.LocalScale = new Vector3(0.5f, 0.5f, 0.5f);
            return true;
        }
    }
}
