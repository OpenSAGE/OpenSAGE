using System;
using System.Globalization;
using ImGuiNET;

namespace OpenSage.Diagnostics
{
    internal sealed class GameLoopView : DiagnosticView
    {
        public GameLoopView(DiagnosticViewContext context) : base(context)
        {

        }

        public override string DisplayName => "Game loop";

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            ImGui.Text($"Logic frame: {Game.CurrentFrame}");
            ImGui.Separator();
            ImGui.Text($"Map time:    {FormatTime(Game.MapTime.TotalGameTime)}");
            ImGui.Text($"Render time: {FormatTime(Game.RenderTime.TotalGameTime)}");
            ImGui.Text($"Frame time:  {Game.RenderTime.ElapsedGameTime.TotalMilliseconds.ToString("F2", CultureInfo.InvariantCulture)} ms");
            ImGui.Text($"Cumulative update time error: {FormatTime(Game.CumulativeLogicUpdateError)}");
        }

        private static string FormatTime(TimeSpan timeSpan) => $"{timeSpan.TotalMinutes:00}:{timeSpan.TotalSeconds:00}:{timeSpan.Milliseconds:000}";
    }
}
