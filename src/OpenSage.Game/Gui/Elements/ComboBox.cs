namespace OpenSage.Gui.Elements
{
    public sealed class ComboBox : UIElement
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawText(Text);
        }
    }
}
