using OpenSage.Graphics;

namespace OpenSage.Diagnostics.AssetViews
{
    // This is just a simple wrapper around TextureView, so that we can inspect both TextureAssets and in-memory Textures.
    // TextureAsset has an implicit cast to Texture, but since AssetViews are discovered via reflection, we need to have
    // a separate class for this.
    [AssetView(typeof(TextureAsset))]
    internal sealed class TextureAssetView : AssetView
    {
        private readonly TextureView _textureView;

        public TextureAssetView(DiagnosticViewContext context, TextureAsset textureAsset) : base(context)
        {
            _textureView = new TextureView(context, textureAsset);
        }

        public override void Draw()
        {
            _textureView.Draw();
        }
    }
}
