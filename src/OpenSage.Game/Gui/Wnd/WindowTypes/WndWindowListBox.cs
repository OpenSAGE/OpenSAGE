using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Data.Wnd;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndWindowListBox : WndWindow
    {
        private readonly ListBoxImages _images;

        public List<WndListBoxItem> ListBoxItems { get; } = new List<WndListBoxItem>();

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                Invalidate();
            }
        }

        internal WndWindowListBox(WndWindowDefinition wndWindow, ContentManager contentManager, WndCallbackResolver callbackResolver)
            : base(wndWindow, contentManager, callbackResolver)
        {
            _selectedIndex = -1;

            _images = new ListBoxImages
            {
                SelectedItem = contentManager.WndImageTextureCache.GetStretchableTexture(
                    wndWindow,
                    wndWindow.EnabledDrawData,
                    1, 3, 2)
            };
        }

        protected override void DefaultInputOverride(WndWindowMessage message, UIElementCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.MouseEnter:
                    CurrentState = WndWindowState.Highlighted;
                    break;

                case WndWindowMessageType.MouseExit:
                    CurrentState = WndWindowState.Enabled;
                    break;

                case WndWindowMessageType.MouseDown:
                    var itemBounds = GetItemBounds();
                    for (var i = 0; i < itemBounds.Length; i++)
                    {
                        if (itemBounds[i].Bounds.Contains(message.MousePosition))
                        {
                            SelectedIndex = i;
                            break;
                        }
                    }
                    break;

                case WndWindowMessageType.MouseUp:
                    break;
            }
        }

        private ListBoxItemDimension[] GetItemBounds()
        {
            var horizontalPadding = 3 * Scale;
            var availableWidth = Bounds.Width - (Definition.ListBoxData.ColumnWidths.Length + 1) * horizontalPadding;

            float calculateColumnWidth(int column)
            {
                return (Definition.ListBoxData.ColumnWidths[column] / 100.0f) * availableWidth;
            }

            var font = GetFont();

            var result = new ListBoxItemDimension[ListBoxItems.Count];

            var y = 0f;
            for (var i = 0; i < ListBoxItems.Count; i++)
            {
                var item = ListBoxItems[i];

                var itemHeight = float.MinValue;
                for (var column = 0; column < Definition.ListBoxData.Columns; column++)
                {
                    var textSize = PrimitiveBatch.MeasureText(
                        item.ColumnData[column],
                        font,
                        TextAlignment.Leading,
                        calculateColumnWidth(column));

                    if (textSize.Height > itemHeight)
                    {
                        itemHeight = textSize.Height;
                    }
                }

                result[i] = new ListBoxItemDimension
                {
                    Bounds = new RectangleF(0, y, Bounds.Width, itemHeight),
                    ColumnBounds = new RectangleF[Definition.ListBoxData.Columns]
                };

                var x = horizontalPadding;
                for (var column = 0; column < Definition.ListBoxData.Columns; column++)
                {
                    var columnWidth = calculateColumnWidth(column);

                    result[i].ColumnBounds[column] = new RectangleF(x, y, columnWidth, itemHeight);

                    x += columnWidth + horizontalPadding;
                }

                y += itemHeight;
            }

            return result;
        }

        private sealed class ListBoxItemDimension
        {
            public RectangleF Bounds;
            public RectangleF[] ColumnBounds;
        }

        protected override void DefaultDrawOverride(Game game)
        {
            var activeState = ActiveState;

            var font = GetFont();
            var textColor = GetTextColor();

            // TODO: Scrolling

            var itemBounds = GetItemBounds();

            for (var i = 0; i < itemBounds.Length; i++)
            {
                var item = ListBoxItems[i];

                if (i == _selectedIndex)
                {
                    var bounds = itemBounds[i].Bounds;
                    PrimitiveBatch.DrawImage(
                        _images.SelectedItem,
                        null,
                        new Mathematics.Rectangle((int) bounds.X, (int) bounds.Y, (int) bounds.Width, (int) bounds.Height));
                }

                for (var column = 0; column < Definition.ListBoxData.Columns; column++)
                {
                    PrimitiveBatch.DrawText(
                        item.ColumnData[column],
                        font,
                        TextAlignment.Leading,
                        textColor,
                        itemBounds[i].ColumnBounds[column]);
                }
            }
        }

        private sealed class ListBoxImages
        {
            public Texture SelectedItem;
            public Texture UpButton;
            public Texture DownButton;
            public Texture Slider;
            public Texture Thumb;
        }
    }

    public sealed class WndListBoxItem
    {
        public object DataItem { get; set; }
        public string[] ColumnData { get; set; }
    }
}
