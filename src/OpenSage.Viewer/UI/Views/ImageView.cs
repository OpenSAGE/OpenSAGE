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

        public override void Draw(ref bool isGameViewFocused)
        {
            var availableSize = ImGui.GetContentRegionAvailable();

            var size = SizeF.CalculateSizeFittingAspectRatio(
                new SizeF(_texture.Width, _texture.Height),
                new Size((int) availableSize.X, (int) availableSize.Y));

            ImGui.Image(
                _imagePointer,
                new Vector2(size.Width, size.Height),
                Vector2.Zero,
                Vector2.One,
                Vector4.One,
                Vector4.Zero);
        }
    }
}
