using System;
using System.Collections.Generic;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Gui.Wnd.Elements;
using OpenSage.Mathematics;
using System.IO;

namespace OpenSage.Gui.Apt
{
    public sealed class ShapeSystem : GameSystem
    {
        private readonly List<ShapeComponent> _guiComponents;
        private List<ShapeComponent> _renderList;
        private readonly EffectPipelineStateHandle _pipelineStateHandle;

        public ShapeSystem(Game game)
            : base(game)
        {
            RegisterComponentList(_guiComponents = new List<ShapeComponent>());

            _renderList = new List<ShapeComponent>();

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


        private static void DoActionRecursive(UIElement element, Func<UIElement, bool> action)
        {
            if (!action(element))
            {
                return;
            }

            foreach (var child in element.Children)
            {
                DoActionRecursive(child, action);
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
