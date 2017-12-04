using System;
using System.Collections.Generic;
using LL.Graphics2D;
using LL.Graphics3D;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;
using OpenSage.Gui.Elements;

namespace OpenSage.Gui
{
    public sealed class GuiSystem : GameSystem
    {
        private readonly List<GuiComponent> _guiComponents;
        private readonly SpriteMaterial _spriteMaterial;
        private readonly EffectPipelineStateHandle _pipelineStateHandle;
        private readonly GraphicsDevice2D _graphicsDevice2D;

        public GuiSystem(Game game) 
            : base(game)
        {
            RegisterComponentList(_guiComponents = new List<GuiComponent>());

            _spriteMaterial = new SpriteMaterial(Game.ContentManager.EffectLibrary.Sprite);

            var materialConstantsBuffer = AddDisposable(new ConstantBuffer<SpriteMaterial.MaterialConstants>(Game.GraphicsDevice));
            materialConstantsBuffer.Value.MipMapLevel = 0;
            materialConstantsBuffer.Update();
            _spriteMaterial.SetMaterialConstants(materialConstantsBuffer.Buffer);

            // TODO: Duplicated from SpriteComponent.
            var rasterizerState = RasterizerStateDescription.CullBackSolid;
            rasterizerState.IsFrontCounterClockwise = false;

            _pipelineStateHandle = new EffectPipelineState(
                rasterizerState,
                DepthStencilStateDescription.None,
                BlendStateDescription.Opaque)
                .GetHandle();

            _graphicsDevice2D = AddDisposable(new GraphicsDevice2D());
        }

        internal void AddRenderItem(RenderList renderList, GuiComponent component)
        {
            // TODO: Duplicated from SpriteComponent.
            renderList.AddGuiRenderItem(new RenderItem(
                component,
                _spriteMaterial,
                _pipelineStateHandle,
                (commandEncoder, effect, pipelineStateHandle, instanceData) =>
                {
                    DoActionRecursive(component.RootWindow, x =>
                    {
                        _spriteMaterial.SetTexture(x.Texture);
                        _spriteMaterial.Apply();

                        effect.Apply(commandEncoder);

                        commandEncoder.Draw(PrimitiveType.TriangleList, 0, 3);
                    });
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
            var renderContext = new RenderContext(
                Game.GraphicsDevice,
                _graphicsDevice2D);

            foreach (var guiComponent in _guiComponents)
            {
                DoActionRecursive(
                    guiComponent.RootWindow, 
                    x => x.Render(renderContext));
            }

            base.Update(gameTime);
        }
    }
}
