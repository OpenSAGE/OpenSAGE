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

        private readonly WindowTransitionManager _transitionManager;

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

            game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\WindowTransitions.ini");
            _transitionManager = new WindowTransitionManager(game.ContentManager.IniDataContext.WindowTransitions);
        }

        internal override void OnEntityComponentAdded(EntityComponent component)
        {
            if (component is GuiComponent c)
            {
                CreateSizeDependentResources(c);

                c.Window.LayoutInit?.Invoke(c.Window);
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
                guiComponent.Window.Root,
                x =>
                {
                    x.CreateSizeDependentResources(Game.ContentManager, size);
                    return true;
                });
        }

        internal void AddRenderItem(RenderList renderList, GuiComponent component)
        {
            renderList.AddGuiRenderItem(new RenderItem(
                component,
                _spriteBatch.Material,
                _pipelineStateHandle,
                (commandEncoder, effect, pipelineStateHandle, instanceData) =>
                {
                    _spriteBatch.Begin(commandEncoder, Game.Scene.Camera.Viewport.Bounds(), BlendStateDescription.AlphaBlend, SamplerStateDescription.PointClamp);

                    DoActionRecursive(component.Window.Root, x =>
                    {
                        if (x.Visible)
                        {
                            _spriteBatch.Draw(
                                x.Texture,
                                new Rectangle(0, 0, x.Texture.Width, x.Texture.Height),
                                x.Frame,
                                x.Opacity);
                            return true;
                        }
                        return false;
                    });

                    _spriteBatch.End();
                }));
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
            var mousePosition = Game.Input.MousePosition;

            foreach (var guiComponent in _guiComponents)
            {
                guiComponent.Window.LayoutUpdate?.Invoke(guiComponent.Window);

                var elementCallbackContext = new UIElementCallbackContext(
                    guiComponent.Window,
                    _transitionManager,
                    mousePosition,
                    gameTime);

                DoActionRecursive(
                    guiComponent.Window.Root,
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

                        x.SystemCallback?.Invoke(x, elementCallbackContext);

                        return true;
                    });
            }

            _transitionManager.Update(gameTime);

            foreach (var guiComponent in _guiComponents)
            {
                DoActionRecursive(
                    guiComponent.Window.Root, 
                    x =>
                    {
                        x.Render(Game.GraphicsDevice);
                        return true;
                    });
            }

            base.Update(gameTime);
        }
    }
}
