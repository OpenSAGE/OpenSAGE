using System;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Mathematics;
using SixLabors.Fonts;

namespace OpenSage.Gui.Wnd.Controls
{
    public class ListBox : Control
    {
        public event EventHandler SelectedIndexChanged
        {
            add { _itemsArea.SelectedIndexChanged += value; }
            remove { _itemsArea.SelectedIndexChanged -= value; }
        }

        private readonly Button _upButton;
        private readonly Button _downButton;
        private readonly Button _thumb;

        private readonly ListBoxItemsArea _itemsArea;

        public bool IsScrollBarVisible
        {
            get => _upButton.Visible;
            set
            {
                _upButton.Visible = value;
                _downButton.Visible = value;
                _thumb.Visible = value;
            }
        }

        public Image UpButtonImage
        {
            get => _upButton.BackgroundImage;
            set => _upButton.BackgroundImage = value;
        }

        public Image UpButtonHoverImage
        {
            get => _upButton.HoverBackgroundImage;
            set => _upButton.HoverBackgroundImage = value;
        }

        public Image DownButtonImage
        {
            get => _downButton.BackgroundImage;
            set => _downButton.BackgroundImage = value;
        }

        public Image DownButtonHoverImage
        {
            get => _downButton.HoverBackgroundImage;
            set => _downButton.HoverBackgroundImage = value;
        }

        public Image ThumbImage
        {
            get => _thumb.BackgroundImage;
            set => _thumb.BackgroundImage = value;
        }

        public Image ThumbHoverImage
        {
            get => _thumb.HoverBackgroundImage;
            set => _thumb.HoverBackgroundImage = value;
        }

        public Image SelectedItemBackgroundImage
        {
            get => _itemsArea.SelectedItemBackgroundImage;
            set => _itemsArea.SelectedItemBackgroundImage = value;
        }

        public Image SelectedItemHoverBackgroundImage
        {
            get => _itemsArea.SelectedItemHoverBackgroundImage;
            set => _itemsArea.SelectedItemHoverBackgroundImage = value;
        }

        public int[] ColumnWidths
        {
            get => _itemsArea.ColumnWidths;
            set => _itemsArea.ColumnWidths = value;
        }

        public ListBoxDataItem[] Items
        {
            get => _itemsArea.Items;
            set => _itemsArea.Items = value;
        }

        public int SelectedIndex
        {
            get => _itemsArea.SelectedIndex;
            set => _itemsArea.SelectedIndex = value;
        }

        public override Font Font
        {
            set
            {
                base.Font = value;
                _itemsArea.Font = value;
            }
        }

        public override ColorRgbaF TextColor
        {
            set
            {
                base.TextColor = value;
                _itemsArea.TextColor = value;
            }
        }

        public ListBox()
        {
            _upButton = new Button();
            _upButton.Click += OnUpButtonClick;
            Controls.Add(_upButton);

            _downButton = new Button();
            _downButton.Click += OnDownButtonClick;
            Controls.Add(_downButton);

            _thumb = new Button();
            _thumb.Click += OnThumbClick;
            Controls.Add(_thumb);

            _itemsArea = new ListBoxItemsArea();
            Controls.Add(_itemsArea);
        }

        private void OnUpButtonClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnDownButtonClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnThumbClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void LayoutOverride()
        {
            var infiniteSize = new Size(int.MaxValue, int.MaxValue);

            var upButtonSize = _upButton.GetPreferredSize(infiniteSize);
            _upButton.Bounds = new Rectangle(
                ClientRectangle.Right - upButtonSize.Width,
                0,
                upButtonSize.Width,
                upButtonSize.Height);

            var downButtonSize = _downButton.GetPreferredSize(infiniteSize);
            _downButton.Bounds = new Rectangle(
                ClientRectangle.Right - downButtonSize.Width,
                ClientRectangle.Bottom - downButtonSize.Height,
                downButtonSize.Width,
                downButtonSize.Height);

            var thumbSize = _thumb.GetPreferredSize(infiniteSize);
            _thumb.Bounds = new Rectangle(
                ClientRectangle.Right - thumbSize.Width,
                _upButton.Bounds.Bottom,
                thumbSize.Width,
                thumbSize.Height);

            var itemsWidth = IsScrollBarVisible
                ? _upButton.Left
                : ClientSize.Width;
            
            _itemsArea.Bounds = new Rectangle(
                0,
                0,
                itemsWidth,
                ClientRectangle.Height);
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            _thumb.Click -= OnThumbClick;
            _downButton.Click -= OnDownButtonClick;
            _upButton.Click -= OnUpButtonClick;

            base.Dispose(disposeManagedResources);
        }
    }

    public sealed class ListBoxDataItem
    {
        public object DataItem { get; }
        public string[] ColumnData { get; }

        public ListBoxDataItem(object dataItem, string[] columnData)
        {
            DataItem = dataItem;
            ColumnData = columnData;
        }
    }

    internal sealed class ListBoxItemsArea : Control
    {
        public event EventHandler SelectedIndexChanged;

        private ListBoxDataItem[] _items = new ListBoxDataItem[0];
        public ListBoxDataItem[] Items
        {
            get => _items;
            set
            {
                Controls.Clear();

                _items = value;

                foreach (var item in value)
                {
                    Controls.Add(new ListBoxItem(this, item));
                }

                UpdateSelectedItem();
            }
        }

        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                UpdateSelectedItem();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateSelectedItem()
        {
            for (var i = 0; i < _items.Length; i++)
            {
                var item = (ListBoxItem) Controls[i];

                if (i == _selectedIndex)
                {
                    item.BackgroundImage = IsMouseOver
                        ? SelectedItemHoverBackgroundImage
                        : SelectedItemBackgroundImage;
                }
                else
                {
                    item.BackgroundImage = null;
                }
            }
        }

        private int[] _columnWidths;
        public int[] ColumnWidths
        {
            get => _columnWidths;
            set
            {
                _columnWidths = value;
                InvalidateLayout();
            }
        }

        public Image SelectedItemBackgroundImage { get; set; }

        public Image SelectedItemHoverBackgroundImage { get; set; }

        public override Size GetPreferredSize(Size proposedSize)
        {
            var height = 0;

            foreach (var child in Controls)
            {
                height += child.GetPreferredSize(proposedSize).Height;
            }

            return new Size(proposedSize.Width, height);
        }

        protected override void LayoutOverride()
        {
            var y = 0;

            foreach (var child in Controls)
            {
                var childHeight = child.GetPreferredSize(ClientSize).Height;

                child.Bounds = new Rectangle(0, y, ClientSize.Width, childHeight);

                y += childHeight;
            }
        }

        protected override void DefaultInputOverride(WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.MouseEnter:
                case WndWindowMessageType.MouseExit:
                    UpdateSelectedItem();
                    break;
            }
        }
    }

    internal sealed class ListBoxItem : Control
    {
        private readonly ListBoxItemsArea _parent;
        private readonly ListBoxDataItem _item;

        private sealed class ListBoxItemDimension
        {
            public Size Size;
            public Rectangle[] ColumnBounds;
        }

        private ListBoxItemDimension _cachedDimension;
        private Size _lastProposedSize;

        public ListBoxItem(ListBoxItemsArea parent, ListBoxDataItem item)
        {
            _parent = parent;
            _item = item;
        }

        private ListBoxItemDimension GetItemBounds(in Size proposedSize)
        {
            if (proposedSize == _lastProposedSize)
            {
                return _cachedDimension;
            }

            const int horizontalPadding = 3;
            var availableWidth = proposedSize.Width - ((_parent.ColumnWidths.Length + 1) * horizontalPadding);

            int calculateColumnWidth(int column)
            {
                return (int) ((_parent.ColumnWidths[column] / 100.0f) * availableWidth);
            }

            var font = _parent.Font;

            var itemHeight = int.MinValue;
            for (var column = 0; column < _parent.ColumnWidths.Length; column++)
            {
                var textSize = DrawingContext2D.MeasureText(
                    _item.ColumnData[column],
                    font,
                    TextAlignment.Leading,
                    calculateColumnWidth(column));

                if (textSize.Height > itemHeight)
                {
                    itemHeight = (int) Math.Ceiling(textSize.Height);
                }
            }

            var result = new ListBoxItemDimension
            {
                Size = new Size(proposedSize.Width, itemHeight),
                ColumnBounds = new Rectangle[_parent.ColumnWidths.Length]
            };

            var x = horizontalPadding;
            for (var column = 0; column < _parent.ColumnWidths.Length; column++)
            {
                var columnWidth = calculateColumnWidth(column);

                result.ColumnBounds[column] = new Rectangle(x, 0, columnWidth, itemHeight);

                x += columnWidth + horizontalPadding;
            }

            _lastProposedSize = proposedSize;
            _cachedDimension = result;

            return result;
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            return GetItemBounds(proposedSize).Size;
        }

        protected override void DefaultInputOverride(WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.MouseUp:
                    _parent.SelectedIndex = _parent.Controls.IndexOf(this);
                    break;
            }
        }

        protected override void DrawOverride(DrawingContext2D drawingContext)
        {
            var itemBounds = GetItemBounds(ClientSize);

            for (var column = 0; column < _parent.ColumnWidths.Length; column++)
            {
                drawingContext.DrawText(
                    _item.ColumnData[column],
                    _parent.Font,
                    TextAlignment.Leading,
                    _parent.TextColor,
                    itemBounds.ColumnBounds[column]);
            }
        }
    }
}
