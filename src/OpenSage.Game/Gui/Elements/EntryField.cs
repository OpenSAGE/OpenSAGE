namespace OpenSage.Gui.Elements
{
    public sealed class EntryField : UIElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(Text);
        }
    }
}
