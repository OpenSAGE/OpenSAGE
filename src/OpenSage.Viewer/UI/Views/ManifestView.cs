using System.Numerics;
using ImGuiNET;
using OpenSage.Data.StreamFS;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class ManifestView : AssetView
    {
        private readonly GameStream _gameStream;
        private Asset _selectedAsset;
        //private ContentView _selectedContentView;

        public ManifestView(AssetViewContext context)
        {
            _gameStream = new GameStream(context.Entry);
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.BeginChild("manifest sidebar", new Vector2(300, 0), true, 0);

            foreach (var asset in _gameStream.ManifestFile.Assets)
            {
                if (ImGui.Selectable(asset.Name, asset == _selectedAsset))
                {
                    _selectedAsset = asset;
                }
            }

            ImGui.EndChild();

            //ImGui.SameLine();

            //if (_selectedContentView != null)
            //{
            //    _selectedContentView.Draw(ref isGameViewFocused);
            //}
        }
    }
}
