using System;
using System.Collections.Generic;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Gui.Wnd.Elements;
using OpenSage.Mathematics;
using System.IO;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndSystem : GameSystem
    {
        private readonly List<WndComponent> _guiComponents;
        private readonly EffectPipelineStateHandle _pipelineStateHandle;

        internal Stack<GuiWindow> WindowStack { get; }
        internal WindowTransitionManager TransitionManager { get; }

        public WndSystem(Game game) 
            : base(game)
        {
            RegisterComponentList(_guiComponents = new List<WndComponent>());

            WindowStack = new Stack<GuiWindow>();

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
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                    game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\WindowTransitions.ini");
                    TransitionManager = new WindowTransitionManager(game.ContentManager.IniDataContext.WindowTransitions);
                    break;

                default: // TODO: Handle other games.
                    TransitionManager = new WindowTransitionManager(new List<Data.Ini.WindowTransition>());
                    break;
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Game.Input.MessageBuffer.Handlers.Add(new WndInputMessageHandler(this));
        }

        internal override void OnEntityComponentAdded(EntityComponent component)
        {
            if (component is WndComponent c)
            {
                CreateSizeDependentResources(c);

                WindowStack.Push(c.Window);

                c.Window.LayoutInit?.Invoke(c.Window);
            }

            base.OnEntityComponentAdded(component);
        }

        internal override void OnEntityComponentRemoved(EntityComponent component)
        {
            if (component is WndComponent c)
            {
                WindowStack.Pop();
            }

            base.OnEntityComponentRemoved(component);
        }

        internal override void OnSwapChainChanged()
        {
            foreach (var guiComponent in _guiComponents)
            {
                CreateSizeDependentResources(guiComponent);
            }
        }

        private void CreateSizeDependentResources(WndComponent guiComponent)
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

        internal override void BuildRenderList(RenderList renderList)
        {
            foreach (var guiComponent in _guiComponents)
            {
                DoActionRecursive(
                    guiComponent.Window.Root,
                    x =>
                    {
                        if (!x.Visible)
                        {
                            return false;
                        }

                        renderList.Gui.AddRenderItemDraw(
                            x.Material,
                            x.VertexBuffer,
                            null,
                            CullFlags.AlwaysVisible,
                            null,
                            default,
                            0,
                            6);

                        return true;
                    });
            }
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
            foreach (var guiComponent in _guiComponents)
            {
                guiComponent.Window.LayoutUpdate?.Invoke(guiComponent.Window);
            }

            TransitionManager.Update(gameTime);

            foreach (var guiComponent in _guiComponents)
            {
                DoActionRecursive(
                    guiComponent.Window.Root, 
                    x =>
                    {
                        x.DrawCallback.Invoke(x, Game.GraphicsDevice);
                        return true;
                    });
            }

            base.Update(gameTime);
        }

        public GuiWindow OpenWindow(string wndFileName)
        {
            var wndFilePath = Path.Combine("Window", wndFileName);
            var window = Game.ContentManager.Load<GuiWindow>(wndFilePath);

            if (window == null)
            {
                throw new Exception($"Window file {wndFilePath} was not found.");
            }

            var guiComponent = new WndComponent
            {
                Window = window
            };

            var entity = new Entity();

            Game.Scene.Entities.Add(entity);

            entity.Components.Add(guiComponent);

            return guiComponent.Window;
        }
    }
}
