using OpenSage.Data.Wnd;

namespace OpenSage.Gui.Wnd.Controls
{
    public class Label : Control
    {
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Center;

        public Label(WndWindowDefinition wndWindow)
        {
            TextAlignment = wndWindow.StaticTextData.Centered
                ? TextAlignment.Center
                : TextAlignment.Leading;

            if(wndWindow.EnabledDrawData.Items.Length > 0)
            {
                BorderColor = wndWindow.EnabledDrawData.Items[0].BorderColor.ToColorRgbaF();
            }
            
            BorderWidth = 1;
        }

        protected override void DrawOverride(DrawingContext2D drawingContext)
        {
            DrawText(drawingContext, TextAlignment);
        }
    }
}
