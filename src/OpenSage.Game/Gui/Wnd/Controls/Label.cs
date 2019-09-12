using OpenSage.Data.Wnd;

namespace OpenSage.Gui.Wnd.Controls
{
    public class Label : Control
    {
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Center;

        public Label(WndWindowDefinition wndWindow)
        {
            TextAlignment = wndWindow.StaticTextData.Centered
                ? TextAlignment.Center
                : TextAlignment.Leading;
        }

        protected override void DrawOverride(DrawingContext2D drawingContext)
        {
            DrawText(drawingContext, TextAlignment);
        }
    }
}
