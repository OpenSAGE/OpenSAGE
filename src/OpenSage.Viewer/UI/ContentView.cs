using System.IO;
using ImGuiNET;
using OpenSage.Data.StreamFS;
using OpenSage.Viewer.UI.Views;

namespace OpenSage.Viewer.UI
{
    internal sealed class ContentView : DisposableBase
    {
        private readonly AssetView _assetView;

        public string DisplayName { get; }

        public ContentView(AssetViewContext context)
        {
            _assetView = AddDisposable(CreateViewForFileSystemEntry(context));
            DisplayName = context.Entry.FilePath;
        }

        private static AssetView CreateViewForFileSystemEntry(AssetViewContext context)
        {
            switch (Path.GetExtension(context.Entry.FilePath).ToLowerInvariant())
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

                case ".ini":
                    return new IniView(context);

                case ".map":
                    return new MapView(context);

                case ".manifest":
                    return new ManifestView(context);

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
