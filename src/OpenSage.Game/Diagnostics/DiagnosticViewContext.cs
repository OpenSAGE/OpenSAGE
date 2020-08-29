using OpenSage.Gui.Apt;
using Veldrid;

namespace OpenSage.Diagnostics
{
    internal sealed class DiagnosticViewContext
    {
        public Game Game { get; }
        public ImGuiRenderer ImGuiRenderer { get; }

        public AptWindow SelectedAptWindow { get; set; }

        public IInspectable SelectedObject { get; set; }

        public DiagnosticViewContext(Game game, ImGuiRenderer imGuiRenderer)
        {
            Game = game;
            ImGuiRenderer = imGuiRenderer;
        }
    }

    public interface IInspectable
    {
        string Name { get; }

        void DrawInspector();
    }
}
