namespace OpenSage.Gui.Elements
{
    public sealed class ListBox : UIElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(Text);
        }
    }
}
