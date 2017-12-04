using LL.Graphics2D;

namespace OpenSage.Gui.Elements
{
    public sealed class Button : UIElement
    {
        public string Text { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(Text);
        }
    }
}
