using System.Collections.Generic;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt
{
    public sealed class ShapeSystem : GameSystem
    {
        private readonly List<ShapeComponent> _guiComponents;
        private List<ShapeComponent> _renderList;

        public ShapeSystem(Game game)
            : base(game)
        {
            RegisterComponentList(_guiComponents = new List<ShapeComponent>());

            _renderList = new List<ShapeComponent>();

            switch (game.SageGame)
            {
                case SageGame.BattleForMiddleEarth:
                case SageGame.BattleForMiddleEarthII:
                    break;
                default: // TODO: Handle other games.

                    break;
            }
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        internal override void OnEntityComponentAdded(EntityComponent component)
        {
            base.OnEntityComponentAdded(component);
        }

        internal override void OnEntityComponentRemoved(EntityComponent component)
        {
            base.OnEntityComponentRemoved(component);
        }

        internal override void OnSwapChainChanged()
        {
            if (Game.Scene == null)
                return;

            var viewport = Game.Scene.Camera.Viewport;
            var size = new Size(viewport.Width, viewport.Height);

            foreach (var shape in _guiComponents)
            {
                shape.Layout(Game.GraphicsDevice, size);
            }
        }

        internal override void BuildRenderList(RenderList renderList)
        {
            foreach (var shape in _guiComponents)
            {
                renderList.Gui.AddRenderItemDraw(
                    shape.Material,
                    shape.VertexBuffer,
                    null,
                    CullFlags.AlwaysVisible,
                    null,
                    default,
                    0,
                    6);
            }

            base.BuildRenderList(renderList);
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var shape in _guiComponents)
            {
                shape.Draw(Game.GraphicsDevice);
            }

            base.Update(gameTime);
        }


    }
}
