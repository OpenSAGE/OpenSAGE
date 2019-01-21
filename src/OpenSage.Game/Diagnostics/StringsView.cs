using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Content.Translation;
using OpenSage.Diagnostics.Util;

namespace OpenSage.Diagnostics
{
    internal sealed class StringsView : DiagnosticView
    {
        private readonly List<string> _labels;
        private readonly byte[] _searchTextBuffer;
        private string _searchText;

        public override string DisplayName { get; } = "Strings";

        public StringsView(DiagnosticViewContext context)
            : base(context)
        {
            _labels = new List<string>();
            _searchTextBuffer = new byte[32];
        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            ImGui.PushItemWidth(-1);
            ImGuiUtility.InputText("##search", _searchTextBuffer, out var searchText);
            UpdateSearch(searchText);
            ImGui.PopItemWidth();

            ImGui.BeginChild("strings", Vector2.Zero, false);

            ImGui.Columns(2, "CSF", true);

            ImGui.Separator();
            ImGui.Text("Name"); ImGui.NextColumn();
            ImGui.Text("Value"); ImGui.NextColumn();
            ImGui.Separator();

            foreach (var label in _labels)
            {
                ImGui.Text(label); ImGui.NextColumn();
                ImGui.Text(CleanText(label.Translate())); ImGui.NextColumn();
            }

            ImGui.Columns(1, null, false);

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

            _labels.Clear();

            foreach (var label in Context.Game.ContentManager.TranslationManager.Labels)
            {
                var matchesSearch = label.Contains(searchText, StringComparison.OrdinalIgnoreCase);

                if (matchesSearch)
                {
                    _labels.Add(label);
                }
            }
        }

        private static string CleanText(string text) => (text ?? string.Empty).Replace("%", "%%");
    }
}
