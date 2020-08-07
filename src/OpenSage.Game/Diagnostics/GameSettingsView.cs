using ImGuiNET;
using OpenSage.Navigation;

namespace OpenSage.Diagnostics
{
    internal sealed class GameSettingsView : DiagnosticView
    {
        public GameSettingsView(DiagnosticViewContext context) : base(context)
        {
        }

        public override string DisplayName => "Game Settings";

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            var enablePathSmooting = PathOptimizer.EnablePathSmoothing;
            if (ImGui.Checkbox("Enable path smoothing", ref enablePathSmooting))
            {
                PathOptimizer.EnablePathSmoothing = enablePathSmooting;
            }
        }
    }
}
