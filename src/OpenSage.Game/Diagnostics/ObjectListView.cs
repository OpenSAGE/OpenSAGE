using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Diagnostics.Util;
using OpenSage.Logic.Object;

namespace OpenSage.Diagnostics
{
    internal sealed class ObjectListView : DiagnosticView
    {
        private readonly List<GameObject> _items;

        private readonly byte[] _searchTextBuffer;
        private string _searchText;

        private GameObject _currentItem;

        public override string DisplayName { get; } = "Object List";

        public override Vector2 DefaultSize { get; } = new Vector2(200, 400);

        public ObjectListView(DiagnosticViewContext context)
            : base(context)
        {
            _items = new List<GameObject>();

            _searchTextBuffer = new byte[32];

            UpdateSearch(null);
        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            ImGui.PushItemWidth(-1);
            ImGuiUtility.InputText("##search", _searchTextBuffer, out var searchText);
            UpdateSearch(searchText);
            ImGui.PopItemWidth();

            ImGui.BeginChild("files list", Vector2.Zero, true);

            foreach (var item in Context.Game.Scene3D.GameObjects.Items)
            {
                var name = GetObjectName(item);
                if (ImGui.Selectable(name, item == _currentItem))
                {
                    _currentItem = item;
                    Context.SelectedObject = item;
                }
                ImGuiUtility.DisplayTooltipOnHover(name);
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

            foreach (var asset in Context.Game.Scene3D.GameObjects.Items)
            {
                var name = GetObjectName(asset);
                if (isEmptySearch || name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _items.Add(asset);
                }
            }
        }

        private string GetObjectName(GameObject gameObject)
        {
            return Context.Game.Scene3D.GameObjects.GetObjectId(gameObject) + " - " + (gameObject.Name ?? gameObject.Definition.Name);
        }
    }
}
