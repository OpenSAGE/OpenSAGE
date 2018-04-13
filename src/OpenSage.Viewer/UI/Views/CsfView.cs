using ImGuiNET;
using OpenSage.Data.Csf;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class CsfView : AssetView
    {
        private readonly CsfFile _csfFile;

        public CsfView(AssetViewContext context)
        {
            _csfFile = CsfFile.FromFileSystemEntry(context.Entry);
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.Columns(2, "CSF", true);

            ImGui.Separator();
            ImGui.Text("Name"); ImGui.NextColumn();
            ImGui.Text("Value"); ImGui.NextColumn();
            ImGui.Separator();

            foreach (var label in _csfFile.Labels)
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

        private static string CleanText(string text) => (text ?? string.Empty).Replace("%", "%%");
    }
}
