//using System;
//using System.Collections.Generic;
//using OpenSage.Content;
//using OpenSage.Data.Wnd;
//using OpenSage.Mathematics;
//using Veldrid;

//namespace OpenSage.Gui.Wnd
//{
//    public sealed class WndWindowListBox : WndWindow
//    {
//        private readonly ImageButton _upButton;
//        private readonly ImageButton _downButton;
//        private readonly ImageButton _thumb;

//        private readonly ListBoxImages _images;
//        private readonly ListBoxImages _imagesHighlighted;

//        public List<WndListBoxItem> ListBoxItems { get; } = new List<WndListBoxItem>();

//        private int _selectedIndex;
//        public int SelectedIndex
//        {
//            get => _selectedIndex;
//            set
//            {
//                _selectedIndex = value;
//                Invalidate();
//            }
//        }

//        internal WndWindowListBox(WndWindowDefinition wndWindow, ContentManager contentManager, WndCallbackResolver callbackResolver)
//            : base(wndWindow, contentManager, callbackResolver)
//        {
//            _selectedIndex = -1;

//            ImageButton createButton(WndDrawData enabledDrawData, WndDrawData hiliteDrawData, Action onClick)
//            {
//                return new ImageButton(
//                    contentManager.WndImageTextureCache.GetNormalTexture(
//                        enabledDrawData,
//                        0),
//                    contentManager.WndImageTextureCache.GetNormalTexture(
//                        hiliteDrawData,
//                        0),
//                    onClick,
//                    Invalidate);
//            }

//            _upButton = createButton(
//                wndWindow.ListBoxEnabledUpButtonDrawData,
//                wndWindow.ListBoxHiliteUpButtonDrawData,
//                ScrollUp);

//            _downButton = createButton(
//                wndWindow.ListBoxEnabledDownButtonDrawData,
//                wndWindow.ListBoxHiliteDownButtonDrawData,
//                ScrollDown);

//            _thumb = createButton(
//                wndWindow.SliderThumbEnabledDrawData,
//                wndWindow.SliderThumbHiliteDrawData,
//                () => { }); // TODO

//            _images = new ListBoxImages
//            {
//                SelectedItem = contentManager.WndImageTextureCache.GetStretchableTexture(
//                    wndWindow,
//                    wndWindow.EnabledDrawData,
//                    1, 3, 2)
//            };

//            _imagesHighlighted = new ListBoxImages
//            {
//                SelectedItem = contentManager.WndImageTextureCache.GetStretchableTexture(
//                    wndWindow,
//                    wndWindow.HiliteDrawData,
//                    1, 3, 2)
//            };
//        }

//        private void ScrollUp()
//        {
//            // TODO
//        }

//        private void ScrollDown()
//        {
//            // TODO
//        }

//        protected override void CreateSizeDependentResourcesOverride(in Size windowSize)
//        {
//            _upButton.Frame = new RectangleF(
//                Bounds.Width - _upButton.TextureSize.Width * Scale,
//                0,
//                _upButton.TextureSize.Width * Scale,
//                _upButton.TextureSize.Height * Scale);

//            _thumb.Frame = new RectangleF(
//                _upButton.Frame.X,
//                _upButton.Frame.Bottom,
//                _thumb.TextureSize.Width * Scale,
//                _thumb.TextureSize.Height * Scale);

//            _downButton.Frame = new RectangleF(
//                Bounds.Width - _downButton.TextureSize.Width * Scale,
//                Bounds.Height - _downButton.TextureSize.Height * Scale,
//                _downButton.TextureSize.Width * Scale,
//                _downButton.TextureSize.Height * Scale);
//        }

//        protected override void DefaultInputOverride(WndWindowMessage message, UIElementCallbackContext context)
//        {
//            _upButton.HandleInput(message);
//            _downButton.HandleInput(message);
//            _thumb.HandleInput(message);

//            switch (message.MessageType)
//            {
//                case WndWindowMessageType.MouseEnter:
//                    CurrentState = WndWindowState.Highlighted;
//                    break;

//                case WndWindowMessageType.MouseExit:
//                    CurrentState = WndWindowState.Enabled;
//                    break;

//                case WndWindowMessageType.MouseUp:
//                    var itemBounds = GetItemBounds();
//                    for (var i = 0; i < itemBounds.Length; i++)
//                    {
//                        if (itemBounds[i].Bounds.Contains(message.MousePosition))
//                        {
//                            SelectedIndex = i;
//                            break;
//                        }
//                    }
//                    break;
//            }
//        }

//        private ListBoxItemDimension[] GetItemBounds()
//        {
//            var horizontalPadding = 3 * Scale;
//            var availableWidth = Bounds.Width -
//                ((Definition.ListBoxData.ColumnWidths.Length + 1) * horizontalPadding) -
//                _upButton.Frame.Width;

//            float calculateColumnWidth(int column)
//            {
//                return (Definition.ListBoxData.ColumnWidths[column] / 100.0f) * availableWidth;
//            }

//            var font = GetFont();

//            var result = new ListBoxItemDimension[ListBoxItems.Count];

//            var y = 0f;
//            for (var i = 0; i < ListBoxItems.Count; i++)
//            {
//                var item = ListBoxItems[i];

//                var itemHeight = float.MinValue;
//                for (var column = 0; column < Definition.ListBoxData.Columns; column++)
//                {
//                    var textSize = PrimitiveBatch.MeasureText(
//                        item.ColumnData[column],
//                        font,
//                        TextAlignment.Leading,
//                        calculateColumnWidth(column));

//                    if (textSize.Height > itemHeight)
//                    {
//                        itemHeight = textSize.Height;
//                    }
//                }

//                result[i] = new ListBoxItemDimension
//                {
//                    Bounds = new RectangleF(0, y, Bounds.Width, itemHeight),
//                    ColumnBounds = new RectangleF[Definition.ListBoxData.Columns]
//                };

//                var x = horizontalPadding;
//                for (var column = 0; column < Definition.ListBoxData.Columns; column++)
//                {
//                    var columnWidth = calculateColumnWidth(column);

//                    result[i].ColumnBounds[column] = new RectangleF(x, y, columnWidth, itemHeight);

//                    x += columnWidth + horizontalPadding;
//                }

//                y += itemHeight;
//            }

//            return result;
//        }

//        private sealed class ListBoxItemDimension
//        {
//            public RectangleF Bounds;
//            public RectangleF[] ColumnBounds;
//        }

//        protected override void DefaultDrawOverride(Game game)
//        {
//            var activeState = ActiveState;

//            var imageSet = (CurrentState == WndWindowState.Highlighted)
//                ? _imagesHighlighted
//                : _images;

//            var font = GetFont();
//            var textColor = StateConfigurations[WndWindowState.Disabled].TextColor.ToColorRgbaF();

//            // TODO: Scrolling

//            var itemBounds = GetItemBounds();

//            for (var i = 0; i < itemBounds.Length; i++)
//            {
//                var item = ListBoxItems[i];

//                if (i == _selectedIndex)
//                {
//                    PrimitiveBatch.DrawImage(
//                        imageSet.SelectedItem,
//                        null,
//                        itemBounds[i].Bounds);
//                }

//                for (var column = 0; column < Definition.ListBoxData.Columns; column++)
//                {
//                    PrimitiveBatch.DrawText(
//                        item.ColumnData[column],
//                        font,
//                        TextAlignment.Leading,
//                        textColor,
//                        itemBounds[i].ColumnBounds[column]);
//                }
//            }

//            _upButton.Draw(PrimitiveBatch);
//            _downButton.Draw(PrimitiveBatch);
//            _thumb.Draw(PrimitiveBatch);
//        }

//        private sealed class ListBoxImages
//        {
//            public Texture SelectedItem;
//        }
//    }

//    public sealed class WndListBoxItem
//    {
//        public object DataItem { get; set; }
//        public string[] ColumnData { get; set; }
//    }
//}
