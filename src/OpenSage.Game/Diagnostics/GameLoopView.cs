using ImGuiNET;

namespace OpenSage.Diagnostics
{
    internal class GameLoopView : DiagnosticView
    {
        private ulong? breakOnFrame;

        public GameLoopView(DiagnosticViewContext context) : base(context)
        {

        }

        public override string DisplayName => "Game loop";

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            if (breakOnFrame == Game.CurrentFrame)
            {
                Game.IsLogicRunning = false;
            }

            var pauseText = Game.IsLogicRunning ? "Pause" : "Play";

            if (ImGui.Button(pauseText))
            {
                Game.IsLogicRunning = !Game.IsLogicRunning;
            }


            if (!Game.IsLogicRunning)
            {
                ImGui.SameLine();

                if (ImGui.Button("Step >"))
                {
                    breakOnFrame = Game.CurrentFrame + 1;
                    Game.IsLogicRunning = true;
                }
            }

            ImGui.Text($"Logic frame: {Game.CurrentFrame}");
            ImGui.Text($"Total time : {Game.UpdateTime.TotalGameTime}");
            ImGui.Text($"Frame time : {Game.UpdateTime.ElapsedGameTime}");
            ImGui.Text($"Cumulative update time error: {Game.CumulativeLogicUpdateError}");
        }
    }
}
