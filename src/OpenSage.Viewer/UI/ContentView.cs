using System.IO;
using ImGuiNET;
using OpenSage.Data;
using OpenSage.Viewer.UI.Views;
using Veldrid;

namespace OpenSage.Viewer.UI
{
    internal sealed class ContentView : DisposableBase
    {
        private readonly FileSystemEntry _entry;

        private readonly AssetView _assetView;

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

                case ".tga":
                    return new TgaView(context);

                default:
                    return null;
            }
        }

        public void Draw()
        {
            ImGui.Text(_entry.FilePath);

            if (_assetView != null)
            {
                _assetView.Draw();
            }
            else
            {
                // TODO
            }
        }
    }
}
