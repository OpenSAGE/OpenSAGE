using System.Collections.Generic;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;
using OpenSage.Graphics;

namespace OpenSage.Gui.Apt
{
    public sealed class AptSystem : GameSystem
    {
        private readonly List<AptComponent> _guiComponents;
        private readonly EffectPipelineStateHandle _pipelineStateHandle;

        public AptSystem(Game game)
            : base(game)
        {
            RegisterComponentList(_guiComponents = new List<AptComponent>());

            // TODO: Duplicated from SpriteComponent.
            var rasterizerState = RasterizerStateDescription.CullBackSolid;
            rasterizerState.IsFrontCounterClockwise = false;

            _pipelineStateHandle = new EffectPipelineState(
                rasterizerState,
                DepthStencilStateDescription.None,
                BlendStateDescription.AlphaBlend)
                .GetHandle();

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
            var size = new Size(viewport.Width, viewport.Height);

            foreach (var guiComponent in _guiComponents)
            {
                guiComponent.Layout(Game.GraphicsDevice, size);
            }
        }

        internal void Render(SpriteBatch spriteBatch)
        {
            foreach (var component in _guiComponents)
            {
                component.Render(spriteBatch);
            }
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var component in _guiComponents)
            {
                component.Update(gameTime, Game.GraphicsDevice);
            }

            base.Update(gameTime);
        }
    }
}
