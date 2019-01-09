using System.IO;
using ImGuiNET;
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
                case ".ini":
                    return new IniView(context);

                case ".manifest":
                    return new ManifestView(context);

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
