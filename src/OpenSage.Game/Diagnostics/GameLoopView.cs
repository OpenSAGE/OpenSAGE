using ImGuiNET;

namespace OpenSage.Diagnostics
{
    internal class GameLoopView : DiagnosticView
    {
        public GameLoopView(DiagnosticViewContext context) : base(context)
        {

        }

        public override string DisplayName => "Game loop";

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            ImGui.Text($"Logic frame: {Game.CurrentFrame}");
            ImGui.Text($"Total time : {Game.UpdateTime.TotalGameTime}");
            ImGui.Text($"Frame time : {Game.UpdateTime.ElapsedGameTime}");
            ImGui.Text($"Cumulative update time error: {Game.CumulativeLogicUpdateError}");
        }
    }
}
