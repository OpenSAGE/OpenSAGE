using ImGuiNET;

namespace OpenSage.Diagnostics
{
    internal sealed class InspectorView : DiagnosticView
    {
        public InspectorView(DiagnosticViewContext context)
            : base(context)
        {
            
        }

        public override string DisplayName { get; } = "Inspector";

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            if (Context.SelectedObject != null)
            {
                ImGui.Text(Context.SelectedObject.Name);

                ImGui.Separator();

                ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.55f);

                Context.SelectedObject.DrawInspector();

                ImGui.PopItemWidth();
            }
            else
            {
                ImGui.Text("Nothing selected");
            }
        }
    }
}
