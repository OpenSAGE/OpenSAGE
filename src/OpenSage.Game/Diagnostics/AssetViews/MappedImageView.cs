using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Graphics;
using OpenSage.Gui;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Diagnostics.AssetViews
{
    [AssetView(typeof(MappedImage))]
    internal sealed class MappedImageView : AssetView
    {
        private readonly Texture _texture;
        private readonly Dictionary<TextureViewDescription, Veldrid.TextureView> _textureViews;

        public MappedImageView(DiagnosticViewContext context, MappedImage mappedImageAsset)
            : base(context)
        {
            _texture = MappedImageUtility.CreateTexture(context.Game.GraphicsLoadContext, mappedImageAsset);
            _textureViews = new Dictionary<TextureViewDescription, Veldrid.TextureView>();
        }

        public override void Draw()
        {
            var textureViewDescription = new TextureViewDescription(_texture, 0, 1, 0, 1);

            if (!_textureViews.TryGetValue(textureViewDescription, out var textureView))
            {
                _textureViews.Add(textureViewDescription, textureView = AddDisposable(Context.Game.GraphicsDevice.ResourceFactory.CreateTextureView(ref textureViewDescription)));
            }

            var imagePointer = Context.ImGuiRenderer.GetOrCreateImGuiBinding(
                Context.Game.GraphicsDevice.ResourceFactory,
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
