using System.Numerics;
using ImGuiNET;
using OpenSage.Data.StreamFS;
using OpenSage.Mathematics;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class ManifestView : AssetView
    {
        private readonly GameStream _gameStream;
        private Asset _selectedAsset;
        //private ContentView _selectedContentView;

        public ManifestView(AssetViewContext context)
        {
            _gameStream = new GameStream(context.Entry, context.Game);
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.BeginChild("manifest sidebar", new Vector2(300, 0), false, 0);
            {
                var panelSize = ImGui.GetContentRegionAvail();
                panelSize.Y /= 2;
                
                ImGui.BeginChild("manifest contents", panelSize, true, 0);

                foreach (var asset in _gameStream.ManifestFile.Assets)
                {
                    if (ImGui.Selectable(asset.Name, asset == _selectedAsset))
                    {
                        _selectedAsset = asset;
                    }
                }

                ImGui.EndChild();

                ImGui.BeginChild("asset properties", ImGui.GetContentRegionAvail(), true, 0);

                if (_selectedAsset != null)
                {
                    ImGui.Text($"Name: {_selectedAsset.Name}");
                    ImGui.Text($"TypeId: {_selectedAsset.Header.TypeId}");
                    ImGui.Text($"AssetType: {_selectedAsset.AssetType}");
                    ImGui.Text($"SourceFileName: {_selectedAsset.SourceFileName}");
                    ImGui.Text($"TypeHash: {_selectedAsset.Header.TypeHash}");
                    ImGui.Text($"InstanceDataSize: {_selectedAsset.Header.InstanceDataSize}");
                    ImGui.Text($"ImportsDataSize: {_selectedAsset.Header.ImportsDataSize}");

                    if (_selectedAsset.AssetImports.Count > 0)
                    {
                        ImGui.Text("Asset references:");
                        foreach (var assetImport in _selectedAsset.AssetImports)
                        {
                            ImGui.BulletText(assetImport.ImportedAsset?.Name ?? "Import not found");
                        }
                    }
                }
                else
                {
                    ImGui.Text("Select an asset to view its properties.");
                }

                ImGui.EndChild();
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
