using ImGuiNET;

namespace OpenSage.Diagnostics
{
    internal sealed class StatisticsView : DiagnosticView
    {
        public override string DisplayName { get; } = "Statistics";

        public StatisticsView(DiagnosticViewContext context)
            : base(context)
        {

        }

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            ImGui.Text($"Rendered objects (opaque): {Context.Game.Graphics.RenderPipeline.RenderedObjectsOpaque}");
            ImGui.Text($"Rendered objects (transparent): {Context.Game.Graphics.RenderPipeline.RenderedObjectsTransparent}");
        }
    }
}
