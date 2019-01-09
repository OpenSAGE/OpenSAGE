using ImGuiNET;

namespace OpenSage.Diagnostics
{
    internal sealed class AptView : DiagnosticView
    {
        public override string DisplayName { get; } = "APT Windows";

        public AptView(DiagnosticViewContext context)
            : base(context)
        {
            
        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            foreach (var aptWindow in Game.Scene2D.AptWindowManager.WindowStack)
            {
                ImGui.Bullet();
                ImGui.SameLine();

                if (ImGui.Selectable(aptWindow.Name, aptWindow == Context.SelectedAptWindow))
                {
                    Context.SelectedAptWindow = aptWindow;
                }
            }
        }
    }
}
