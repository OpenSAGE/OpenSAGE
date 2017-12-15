using System;
using System.Collections.Generic;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Gui.Elements;
using OpenSage.Mathematics;

namespace OpenSage.Gui
{
    public sealed class GuiSystem : GameSystem
    {
        private readonly List<GuiComponent> _guiComponents;
        private readonly SpriteBatch _spriteBatch;
        private readonly EffectPipelineStateHandle _pipelineStateHandle;

        public GuiSystem(Game game) 
            : base(game)
        {
            RegisterComponentList(_guiComponents = new List<GuiComponent>());

            _spriteBatch = AddDisposable(new SpriteBatch(Game.ContentManager));

            // TODO: Duplicated from SpriteComponent.
            var rasterizerState = RasterizerStateDescription.CullBackSolid;
            rasterizerState.IsFrontCounterClockwise = false;

            _pipelineStateHandle = new EffectPipelineState(
                rasterizerState,
                DepthStencilStateDescription.None,
                BlendStateDescription.AlphaBlend)
                .GetHandle();
        }

        internal override void OnEntityComponentAdded(EntityComponent component)
        {
            if (component is GuiComponent c)
            {
                CreateSizeDependentResources(c);
            }

            base.OnEntityComponentAdded(component);
        }

        internal override void OnSwapChainChanged()
        {
            foreach (var guiComponent in _guiComponents)
            {
                CreateSizeDependentResources(guiComponent);
            }
        }

        private void CreateSizeDependentResources(GuiComponent guiComponent)
        {
            var viewport = Game.Scene.Camera.Viewport;
            var size = new Size(viewport.Width, viewport.Height);
            DoActionRecursive(
                guiComponent.RootWindow,
                x => x.CreateSizeDependentResources(Game.ContentManager, size));
        }

        internal void AddRenderItem(RenderList renderList, GuiComponent component)
        {
            renderList.AddGuiRenderItem(new RenderItem(
                component,
                _spriteBatch.Material,
                _pipelineStateHandle,
                (commandEncoder, effect, pipelineStateHandle, instanceData) =>
                {
                    _spriteBatch.Begin(commandEncoder, Game.Scene.Camera.Viewport.Bounds(), BlendStateDescription.AlphaBlend, SamplerStateDescription.LinearClamp);

                    DoActionRecursive(component.RootWindow, x =>
                    {
                        if (!x.Hidden)
                        {
                            _spriteBatch.Draw(
                                x.Texture,
                                new Rectangle(0, 0, x.Texture.Width, x.Texture.Height),
                                x.Frame);
                        }
                    });

                    _spriteBatch.End();
                }));
        }

        private static void DoActionRecursive(UIElement element, Action<UIElement> action)
        {
            action(element);

            foreach (var child in element.Children)
            {
                DoActionRecursive(child, action);
            }
        }

        public override void Update(GameTime gameTime)
        {
            var mousePosition = Game.Input.MousePosition;

            foreach (var guiComponent in _guiComponents)
            {
                DoActionRecursive(
                    guiComponent.RootWindow,
                    x =>
                    {
                        if (x.Frame.Contains(mousePosition))
                        {
                            if (!x.IsMouseOver)
                            {
                                x.IsMouseOver = true;
                                x.OnMouseEnter(EventArgs.Empty);
                            }
                        }
                        else if (x.IsMouseOver)
                        {
                            x.IsMouseOver = false;
                            x.OnMouseExit(EventArgs.Empty);
                        }
                    });
            }

            foreach (var guiComponent in _guiComponents)
            {
                DoActionRecursive(
                    guiComponent.RootWindow, 
                    x => x.Render(Game.GraphicsDevice));
            }

            base.Update(gameTime);
        }
    }
}
