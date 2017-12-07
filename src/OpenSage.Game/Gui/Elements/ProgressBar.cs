namespace OpenSage.Gui.Elements
{
    public sealed class ProgressBar : UIElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(Text);
        }
    }
}
