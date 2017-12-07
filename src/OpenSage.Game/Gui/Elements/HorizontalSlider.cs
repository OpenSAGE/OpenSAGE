namespace OpenSage.Gui.Elements
{
    public sealed class HorizontalSlider : UIElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(Text);
        }
    }
}
