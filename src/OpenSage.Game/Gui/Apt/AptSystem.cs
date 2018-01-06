using System.Collections.Generic;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;

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

        public override void Initialize()
        {
            base.Initialize();
        }

        internal override void OnEntityComponentAdded(EntityComponent component)
        {
            if (component is AptComponent c)
            {
            }

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

            foreach (var guiComponent in _guiComponents)
            {
                guiComponent.Layout(Game.GraphicsDevice, size);
            }
        }

        public override void Update(GameTime gameTime)
        {
            foreach(var component in _guiComponents)
            {
                component.Update(gameTime, Game.GraphicsDevice);
            }

            base.Update(gameTime);
        }      
    }
}
