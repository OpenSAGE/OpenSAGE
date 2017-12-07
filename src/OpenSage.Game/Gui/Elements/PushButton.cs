namespace OpenSage.Gui.Elements
{
    public sealed class PushButton : UIElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(Text);
        }
    }
}
