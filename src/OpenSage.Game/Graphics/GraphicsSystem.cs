using System.Collections.Generic;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Graphics
{
    public sealed class GraphicsSystem : GameSystem
    {
        private readonly List<CameraComponent> _cameras;
        private readonly List<ModelComponent> _models;

        public IEnumerable<CameraComponent> Cameras => _cameras;

        internal readonly RenderList RenderList = new RenderList();

        public GraphicsSystem(Game game)
            : base(game)
        {
            RegisterComponentList(_cameras = new List<CameraComponent>());
            RegisterComponentList(_models = new List<ModelComponent>());

            RenderList = new RenderList();
        }

        internal override void OnEntityComponentAdded(EntityComponent component)
        {
            base.OnEntityComponentAdded(component);

            if (component is RenderableComponent r)
            {
                r.BuildRenderList(RenderList);
            }
        }

        internal override void OnEntityComponentRemoved(EntityComponent component)
        {
            base.OnEntityComponentRemoved(component);

            if (component is RenderableComponent r)
            {
                RenderList.RemoveRenderable(r);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var camera in _cameras)
            {
                camera.Render(gameTime);
            }

            base.Draw(gameTime);
        }
    }
}
