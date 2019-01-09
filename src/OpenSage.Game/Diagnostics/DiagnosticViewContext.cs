using Veldrid;

namespace OpenSage.Diagnostics
{
    internal sealed class DiagnosticViewContext
    {
        public Game Game { get; }
        public ImGuiRenderer ImGuiRenderer { get; }

        public DiagnosticViewContext(Game game, ImGuiRenderer imGuiRenderer)
        {
            Game = game;
            ImGuiRenderer = imGuiRenderer;
        }
    }
}
