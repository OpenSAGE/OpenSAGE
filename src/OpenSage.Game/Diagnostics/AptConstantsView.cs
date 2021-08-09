using System;
using ImGuiNET;
using OpenSage.FileFormats.Apt;

namespace OpenSage.Diagnostics
{
    internal sealed class AptConstantsView : DiagnosticView
    {
        public override string DisplayName { get; } = "APT Constants";

        public AptConstantsView(DiagnosticViewContext context)
            : base(context)
        {

        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            if (Context.SelectedAptWindow == null)
            {
                ImGui.Text("APT constants are only available when an APT window has been selected in the APT Windows window.");
                return;
            }

            ImGui.Columns(2, "Const", true);

            ImGui.Separator();
            ImGui.Text("Index"); ImGui.NextColumn();
            ImGui.Text("Value"); ImGui.NextColumn();
            ImGui.Separator();

            var constEntries = Context.SelectedAptWindow.AptFile.Constants.Entries;
            for (var i = 0; i < constEntries.Count; i++)
            {
                var entry = constEntries[i];

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
