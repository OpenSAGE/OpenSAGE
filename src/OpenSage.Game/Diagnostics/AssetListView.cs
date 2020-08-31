using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using ImGuiNET;
using OpenSage.Diagnostics.AssetViews;
using OpenSage.Diagnostics.Util;

namespace OpenSage.Diagnostics
{
    internal sealed class AssetListView : DiagnosticView
    {
        private static readonly Dictionary<Type, ConstructorInfo> AssetViewConstructors;
        private static readonly ConstructorInfo DefaultAssetViewConstructor;

        static AssetListView()
        {
            AssetViewConstructors = new Dictionary<Type, ConstructorInfo>();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var assetViewAttribute = type.GetCustomAttribute<AssetViewAttribute>();
                if (assetViewAttribute != null)
                {
                    var constructorParameterTypes = new[]
                    {
                        typeof(DiagnosticViewContext),
                        assetViewAttribute.ForType
                    };
                    AssetViewConstructors.Add(assetViewAttribute.ForType, type.GetConstructor(constructorParameterTypes));
                }
            }

            DefaultAssetViewConstructor = typeof(DefaultAssetView).GetConstructors()[0];
        }

        private readonly List<AssetListItem> _items;

        private readonly byte[] _searchTextBuffer;
        private string _searchText;

        private AssetListItem _currentItem;
        private AssetView _currentAssetView;

        public override string DisplayName { get; } = "Asset List";

        public override Vector2 DefaultSize { get; } = new Vector2(700, 400);

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
            public readonly Func<AssetView> CreateAssetView;

            public AssetListItem(string name, BaseAsset asset, Func<AssetView> createAssetView)
            {
                Name = name;
                Asset = asset;
                CreateAssetView = createAssetView;
            }
        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            ImGui.BeginChild("asset list sidebar", new Vector2(350, 0), true, 0);

            ImGui.PushItemWidth(-1);
            ImGuiUtility.InputText("##search", _searchTextBuffer, out var searchText);
            UpdateSearch(searchText);
            ImGui.PopItemWidth();

            ImGui.BeginChild("files list", Vector2.Zero, true);

            foreach (var item in _items)
            {
                if (ImGui.Selectable(item.Name, item == _currentItem))
                {
                    _currentItem = item;

                    RemoveAndDispose(ref _currentAssetView);

                    _currentAssetView = AddDisposable(item.CreateAssetView());

                    Context.SelectedObject = new DefaultInspectable(item.Asset, Context);
                }
                ImGuiUtility.DisplayTooltipOnHover(item.Name);
            }

            ImGui.EndChild();
            ImGui.EndChild();

            ImGui.SameLine();

            if (_currentItem != null)
            {
                ImGui.BeginChild("asset view");
                _currentAssetView.Draw();
                ImGui.EndChild();
            }
            else
            {
                ImGui.Text("Select a previewable asset.");
            }
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
                    if (!AssetViewConstructors.TryGetValue(asset.GetType(), out var assetViewConstructor))
                    {
                        assetViewConstructor = DefaultAssetViewConstructor;
                    }

                    AssetView createAssetView() => (AssetView) assetViewConstructor.Invoke(new object[] { Context, asset });
                    _items.Add(new AssetListItem(asset.FullName, asset, createAssetView));
                }
            }
        }
    }
}
