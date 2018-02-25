namespace OpenSage.Gui.Wnd.Controls
{
    public class Label : Control
    {
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Center;

        protected override void DrawOverride(DrawingContext2D drawingContext)
        {
            DrawText(drawingContext, TextAlignment);
        }
    }
}
