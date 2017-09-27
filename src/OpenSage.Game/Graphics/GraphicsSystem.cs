using System.Collections.Generic;
using LLGfx;
using OpenSage.Graphics.Cameras;

namespace OpenSage.Graphics
{
    public sealed class GraphicsSystem : GameSystem
    {
        private readonly List<CameraComponent> _cameras;
        private readonly List<RenderableComponent> _renderables;

        public IEnumerable<CameraComponent> Cameras => _cameras;
        public IReadOnlyList<RenderableComponent> Renderables => _renderables;

        public GraphicsSystem(Game game)
            : base(game)
        {
            RegisterComponentList(_cameras = new List<CameraComponent>());
            RegisterComponentList(_renderables = new List<RenderableComponent>());
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var camera in _cameras)
                camera.Render();

            base.Draw(gameTime);
        }
    }
}
