using System;
using ImGuiNET;
using OpenSage.Data.Apt;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class ConstView : AssetView
    {
        private readonly ConstantData _constFile;

        public ConstView(AssetViewContext context)
        {
            _constFile = ConstantData.FromFileSystemEntry(context.Entry);
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.Columns(2, "Const", true);

            ImGui.Separator();
            ImGui.Text("Index"); ImGui.NextColumn();
            ImGui.Text("Value"); ImGui.NextColumn();
            ImGui.Separator();

            for (var i = 0; i < _constFile.Entries.Count; i++)
            {
                var entry = _constFile.Entries[i];

                ImGui.Text(i.ToString()); ImGui.NextColumn();

                var value = (entry.Type == ConstantEntryType.Register)
                    ? Convert.ToUInt32(entry.Value).ToString()
                    : Convert.ToString(entry.Value);

                ImGui.Text(value); ImGui.NextColumn();
            }

            ImGui.Columns(1, null, false);
        }
    }
}
