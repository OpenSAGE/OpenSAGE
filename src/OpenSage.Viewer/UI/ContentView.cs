using System.IO;
using OpenSage.Data;
using OpenSage.Viewer.UI.Views;

namespace OpenSage.Viewer.UI
{
    internal sealed class ContentView : DisposableBase
    {
        private readonly FileSystemEntry _entry;

        private readonly AssetView _assetView;

        public FileSystemEntry Entry => _entry;

        public ContentView(AssetViewContext context)
        {
            _entry = context.Entry;

            _assetView = CreateViewForFileSystemEntry(context);
        }

        private AssetView CreateViewForFileSystemEntry(AssetViewContext context)
        {
            switch (Path.GetExtension(context.Entry.FilePath).ToLower())
            {
                case ".bmp":
                    return new BmpView(context);

                case ".map":
                    return new MapView(context);

                case ".tga":
                    return new TgaView(context);

                case ".w3d":
                    return new W3dView(context);

                default:
                    return null;
            }
        }

        public void Draw(ref bool isGameViewFocused)
        {
            if (_assetView != null)
            {
                _assetView.Draw(ref isGameViewFocused);
            }
            else
            {
                // TODO
            }
        }
    }
}
