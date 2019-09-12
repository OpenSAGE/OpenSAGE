using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Controls
{
    public class CheckBox : Control
    {
        private static readonly Size CheckBoxSize = new Size(22, 22);

        public Image UncheckedImage { get; set; }
        public Image CheckedImage { get; set; }

        public Image HoverUncheckedImage { get; set; }
        public Image HoverCheckedImage { get; set; }

        public Image DisabledUncheckedImage { get; set; }
        public Image DisabledCheckedImage { get; set; }

        public bool Checked { get; set; }

        public CheckBox(WndWindowDefinition wndWindow, WndImageLoader imageLoader)
        {
            UncheckedImage = imageLoader.CreateNormalImage(wndWindow.EnabledDrawData, 1);
            CheckedImage = imageLoader.CreateNormalImage(wndWindow.EnabledDrawData, 2);

            HoverUncheckedImage = imageLoader.CreateNormalImage(wndWindow.HiliteDrawData, 1);
            HoverCheckedImage = imageLoader.CreateNormalImage(wndWindow.HiliteDrawData, 2);

            DisabledUncheckedImage = imageLoader.CreateNormalImage(wndWindow.DisabledDrawData, 1);
            DisabledCheckedImage = imageLoader.CreateNormalImage(wndWindow.DisabledDrawData, 2);
        }

        protected override void LayoutOverride()
        {
            UncheckedImage?.SetSize(CheckBoxSize);
            CheckedImage?.SetSize(CheckBoxSize);

            HoverUncheckedImage?.SetSize(CheckBoxSize);
            HoverCheckedImage?.SetSize(CheckBoxSize);

            DisabledUncheckedImage?.SetSize(CheckBoxSize);
            DisabledCheckedImage?.SetSize(CheckBoxSize);
        }

        protected override void DefaultInputOverride(WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.MouseUp:
                    Checked = !Checked;
                    break;
            }
        }

        protected override void DrawOverride(DrawingContext2D drawingContext)
        {
            var checkboxRect = new Rectangle(Point2D.Zero, CheckBoxSize);
            GetImage().Draw(drawingContext, checkboxRect);

            var textLeft = checkboxRect.Right + 3;
            var textRect = new Rectangle(textLeft, 0, ClientSize.Width - textLeft, CheckBoxSize.Height);
            DrawText(drawingContext, TextAlignment.Leading, textRect);
        }

        private Image GetImage()
        {
            if (Checked)
            {
                if (!Enabled)
                {
                    return DisabledCheckedImage;
                }
                else if (IsMouseOver)
                {
                    return HoverCheckedImage;
                }
                else
                {
                    return CheckedImage;
                }
            }
            else
            {
                if (!Enabled)
                {
                    return DisabledUncheckedImage;
                }
                else if (IsMouseOver)
                {
                    return HoverUncheckedImage;
                }
                else
                {
                    return UncheckedImage;
                }
            }
        }
    }
}
