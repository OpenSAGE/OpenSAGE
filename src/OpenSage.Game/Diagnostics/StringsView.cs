using ImGuiNET;
using OpenSage.Content;

namespace OpenSage.Diagnostics
{
    internal sealed class StringsView : DiagnosticView
    {
        public override string DisplayName { get; } = "Strings";

        public StringsView(DiagnosticViewContext context)
            : base(context)
        {
            
        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            ImGui.Columns(2, "CSF", true);

            ImGui.Separator();
            ImGui.Text("Name"); ImGui.NextColumn();
            ImGui.Text("Value"); ImGui.NextColumn();
            ImGui.Separator();

            foreach (var label in Game.ContentManager.TranslationManager.Labels)
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
