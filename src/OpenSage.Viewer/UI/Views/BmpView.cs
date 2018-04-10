using Veldrid;
using Veldrid.ImageSharp;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class BmpView : ImageView
    {
        public BmpView(AssetViewContext context)
            : base(context)
        {
        }

        protected override Texture GetTexture(AssetViewContext context)
        {
            return AddDisposable(new ImageSharpTexture(context.Entry.FullFilePath).CreateDeviceTexture(
                context.GraphicsDevice,
                context.GraphicsDevice.ResourceFactory));
        }
    }
}
