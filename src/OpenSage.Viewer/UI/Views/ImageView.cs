using System;
using System.Numerics;
using ImGuiNET;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Viewer.UI.Views
{
    internal abstract class ImageView : AssetView
    {
        private readonly Texture _texture;
        private readonly IntPtr _imagePointer;

        protected ImageView(AssetViewContext context)
        {
            _texture = GetTexture(context);

            _imagePointer = context.ImGuiRenderer.GetOrCreateImGuiBinding(
                context.GraphicsDevice.ResourceFactory,
                _texture);

            AddDisposeAction(() => context.ImGuiRenderer.ClearCachedImageResources());
        }

        protected abstract Texture GetTexture(AssetViewContext context);

        public override void Draw()
        {
            var textureBounds = new RectangleF(0, 0, _texture.Width, _texture.Height);

            var availableSize = ImGui.GetContentRegionAvailable();

            var rect = RectangleF.CalculateRectangleFittingAspectRatio(
                textureBounds,
                textureBounds.Size,
                new Size((int) availableSize.X, (int) availableSize.Y));

            ImGui.Image(
                _imagePointer,
                new Vector2(rect.Width, rect.Height),
                Vector2.Zero,
                Vector2.One,
                Vector4.One,
                Vector4.Zero);
        }
    }
}
