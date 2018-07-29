namespace OpenSage.Gui.Wnd.Controls
{
    public class TextBox : Control
    {
        public bool IsReadOnly { get; set; }

        protected override void DrawOverride(DrawingContext2D drawingContext)
        {
            DrawText(drawingContext, TextAlignment.Leading);
        }
    }
}
