namespace OpenSage.Gui.Elements
{
    public sealed class CheckBox : UIElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(Text);
        }
    }
}
