using System.Collections.Generic;
using OpenSage.Graphics;
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

        internal override void OnSwapChainChanged()
        {
            if (Game.Scene == null)
                return;

            var viewport = Game.Scene.Camera.Viewport;
            var size = new Size((int) viewport.Width, (int) viewport.Height);

            foreach (var shape in _guiComponents)
            {
                shape.Layout(Game.GraphicsDevice, size);
            }
        }

        internal void Render(SpriteBatch spriteBatch)
        {
            foreach (var shape in _guiComponents)
            {
                shape.Render(spriteBatch);
            }
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
