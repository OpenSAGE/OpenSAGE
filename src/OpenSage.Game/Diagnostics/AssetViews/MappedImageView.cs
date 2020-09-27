using System.Numerics;
using ImGuiNET;
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
        private readonly Veldrid.TextureView _textureView;

        public MappedImageView(DiagnosticViewContext context, MappedImage mappedImageAsset)
            : base(context)
        {
            _texture = MappedImageUtility.CreateTexture(context.Game.GraphicsLoadContext, mappedImageAsset);
            var textureViewDescription = new TextureViewDescription(_texture, 0, 1, 0, 1);
            _textureView = AddDisposable(Context.Game.GraphicsDevice.ResourceFactory.CreateTextureView(ref textureViewDescription));
        }

        public override void Draw()
        {
            var imagePointer = Context.ImGuiRenderer.GetOrCreateImGuiBinding(
                Context.Game.GraphicsDevice.ResourceFactory,
                _textureView);

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
