using System.Collections.Generic;
using ImGuiNET;
using OpenSage.Data.Csf;
using OpenSage.Viewer.Util;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class CsfView : AssetView
    {
        private readonly CsfFile _csfFile;
        private byte[] _searchTextBuffer;
        private string _searchText;
        private List<CsfLabel> _labels;

        public CsfView(AssetViewContext context)
        {
            _csfFile = CsfFile.FromFileSystemEntry(context.Entry);
            _searchTextBuffer = new byte[32];
            _labels = new List<CsfLabel>();
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGuiUtility.InputText("##search", _searchTextBuffer, out var searchText);
            ImGui.Columns(2, "CSF", true);

            ImGui.Separator();
            ImGui.Text("Name"); ImGui.NextColumn();
            ImGui.Text("Value"); ImGui.NextColumn();
            ImGui.Separator();

            UpdateSearch(searchText);

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

            foreach (var label in _csfFile.Labels)
            {
                foreach (var str in label.Strings)
                {
                    if (str.Value.Contains(searchText))
                    {
                        _labels.Add(label);
                    }
                }
            }
        }

        private static string CleanText(string text) => (text ?? string.Empty).Replace("%", "%%");
    }
}
