using System;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Controls
{
    public class Slider : Control
    {
        private static readonly Size BoxSize = new Size(13, 11);

        public Image HighlightedBoxImage { get; set; }
        public Image UnhighlightedBoxImage { get; set; }

        public int MinimumValue { get; set; }
        public int MaximumValue { get; set; }

        public int Value { get; set; }

        public Slider(WndWindowDefinition wndWindow, WndImageLoader imageLoader)
        {
            HighlightedBoxImage = imageLoader.CreateNormalImage(wndWindow.DisabledDrawData, 0);
            UnhighlightedBoxImage = imageLoader.CreateNormalImage(wndWindow.DisabledDrawData, 1);

            MinimumValue = wndWindow.SliderData.MinValue;
            MaximumValue = wndWindow.SliderData.MaxValue;

            Value = wndWindow.SliderData.MinValue + (wndWindow.SliderData.MaxValue - wndWindow.SliderData.MinValue) / 2;
        }

        protected override void LayoutOverride()
        {
            HighlightedBoxImage.SetSize(BoxSize);
            UnhighlightedBoxImage.SetSize(BoxSize);
        }

        protected override void DrawOverride(DrawingContext2D drawingContext)
        {
            var valueWidth = (Value - MinimumValue) / (float) (MaximumValue - MinimumValue) * ClientSize.Width;

            var x = 0;
            while (x < ClientRectangle.Right)
            {
                var image = x < valueWidth
                    ? HighlightedBoxImage
                    : UnhighlightedBoxImage;

                image.Draw(
                    drawingContext,
                    new Rectangle(new Point2D(x, 0), BoxSize));

                x += BoxSize.Width + 1;
            }
        }

        protected override void DefaultInputOverride(WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.MouseUp:
                    var value = message.MousePosition.X / (float) ClientSize.Width;
                    Value = (int) Math.Round(MinimumValue + (MaximumValue - MinimumValue) * value);
                    break;
            }
        }
    }
}
