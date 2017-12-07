namespace OpenSage.Gui.Elements
{
    public sealed class VerticalSlider : UIElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(Text);
        }
    }
}
