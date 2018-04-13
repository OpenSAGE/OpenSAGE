using System.IO;
using ImGuiNET;
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

            _assetView = AddDisposable(CreateViewForFileSystemEntry(context));
        }

        private static AssetView CreateViewForFileSystemEntry(AssetViewContext context)
        {
            switch (Path.GetExtension(context.Entry.FilePath).ToLower())
            {
                case ".ani":
                    return new AniView(context);

                case ".apt":
                    return new AptView(context);

                case ".bmp":
                    return new BmpView(context);

                case ".const":
                    return new ConstView(context);

                case ".csf":
                    return new CsfView(context);

                case ".dds":
                    return new DdsView(context);

                case ".map":
                    return new MapView(context);

                case ".ru":
                    return new RuView(context);

                case ".tga":
                    return new TgaView(context);

                case ".txt":
                    return new TxtView(context);

                case ".w3d":
                    return new W3dView(context);

                case ".wav":
                    return new WavView(context);

                case ".wnd":
                    return new WndView(context);

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
                ImGui.TextDisabled("No preview has been implemented for this content type.");
            }
        }
    }
}
