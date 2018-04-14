using Veldrid;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class TgaView : ImageView
    {
        public TgaView(AssetViewContext context)
            : base(context)
        {
            
        }

        protected override Texture GetTexture(AssetViewContext context)
        {
            return context.Game.ContentManager.Load<Texture>(context.Entry.FilePath);
        }
    }
}
