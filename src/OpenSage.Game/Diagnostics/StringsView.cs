using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Csf;
using OpenSage.Diagnostics.Util;

namespace OpenSage.Diagnostics
{
    internal sealed class StringsView : DiagnosticView
    {
        private readonly List<CsfLabel> _labels;
        private readonly byte[] _searchTextBuffer;
        private string _searchText;

        public override string DisplayName { get; } = "Strings";

        public StringsView(DiagnosticViewContext context)
            : base(context)
        {
            _labels = new List<CsfLabel>();
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
                ImGui.Text(label.Name); ImGui.NextColumn();

                string value = null;
                if (label.Strings.Length > 0)
                {
                    var csfString = label.Strings[0];
                    value = csfString.Value;
                }

                ImGui.Text(CleanText(value)); ImGui.NextColumn();
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
                var matchesSearch =
                    label.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                    || label.Strings.Any(x => x.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase));

                if (matchesSearch)
                {
                    _labels.Add(label);
                }
            }
        }

        private static string CleanText(string text) => (text ?? string.Empty).Replace("%", "%%");
    }
}
