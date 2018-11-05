using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Viewer.UI.Views
{
    internal abstract class ImageView : AssetView
    {
        private readonly AssetViewContext _context;
        private readonly Texture _texture;
        private readonly Dictionary<TextureViewDescription, TextureView> _textureViews;

        protected Texture Texture => _texture;

        protected ImageView(AssetViewContext context)
        {
            _context = context;
            _textureViews = new Dictionary<TextureViewDescription, TextureView>();

            _texture = GetTexture(context);
        }

        protected abstract Texture GetTexture(AssetViewContext context);

        protected virtual TextureViewDescription GetTextureViewDescription(Texture texture)
        {
            return new TextureViewDescription(texture);
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            var textureViewDescription = GetTextureViewDescription(_texture);

            if (!_textureViews.TryGetValue(textureViewDescription, out var textureView))
            {
                _textureViews.Add(textureViewDescription, textureView = AddDisposable(_context.GraphicsDevice.ResourceFactory.CreateTextureView(ref textureViewDescription)));
            }

            var imagePointer = _context.ImGuiRenderer.GetOrCreateImGuiBinding(
                _context.GraphicsDevice.ResourceFactory,
                textureView);

            var availableSize = ImGui.GetContentRegionAvail();

            var size = SizeF.CalculateSizeFittingAspectRatio(
                new SizeF(_texture.Width, _texture.Height),
                new Size((int) availableSize.X, (int) availableSize.Y));

            ImGui.Image(
                imagePointer,
                new Vector2(size.Width, size.Height),
                Vector2.Zero,
                Vector2.One,
                Vector4.One,
                Vector4.Zero);
        }
    }
}
