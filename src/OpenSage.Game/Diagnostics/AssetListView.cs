using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Diagnostics.Util;

namespace OpenSage.Diagnostics
{
    internal sealed class AssetListView : DiagnosticView
    {
        private readonly List<AssetListItem> _items;

        private readonly byte[] _searchTextBuffer;
        private string _searchText;

        public override string DisplayName { get; } = "Asset List";

        public override Vector2 DefaultSize { get; } = new Vector2(350, 400);

        public AssetListView(DiagnosticViewContext context)
            : base(context)
        {
            _items = new List<AssetListItem>();

            _searchTextBuffer = new byte[32];

            UpdateSearch(null);
        }

        private sealed class AssetListItem
        {
            public readonly string Name;
            public readonly BaseAsset Asset;

            public AssetListItem(string name, BaseAsset asset)
            {
                Name = name;
                Asset = asset;
            }
        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            ImGui.PushItemWidth(-1);
            ImGuiUtility.InputText("##search", _searchTextBuffer, out var searchText);
            UpdateSearch(searchText);
            ImGui.PopItemWidth();

            ImGui.BeginChild("files list", Vector2.Zero, true);

            foreach (var item in _items)
            {
                if (ImGui.Selectable(item.Name, item.Asset == Context.SelectedObject))
                {
                    Context.SelectedObject = item.Asset;
                }
                ImGuiUtility.DisplayTooltipOnHover(item.Name);
            }

            ImGui.EndChild();
        }

        private void UpdateSearch(string searchText)
        {
            searchText = ImGuiUtility.TrimToNullByte(searchText);

            if (searchText == _searchText)
            {
                return;
            }

            _searchText = searchText;

            _items.Clear();

            var isEmptySearch = string.IsNullOrWhiteSpace(_searchText);

            var assetStore = Context.Game.AssetStore;

            foreach (var asset in assetStore.GetAllAssets())
            {
                if (isEmptySearch || asset.FullName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _items.Add(new AssetListItem(asset.FullName, asset));
                }
            }
        }
    }
}
