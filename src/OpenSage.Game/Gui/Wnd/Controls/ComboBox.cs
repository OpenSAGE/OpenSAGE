using System;
using System.Linq;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;
using SixLabors.Fonts;

namespace OpenSage.Gui.Wnd.Controls
{
    public class ComboBox : Control
    {
        private readonly TextBox _editBox;
        private readonly Button _dropDownButton;
        private readonly ListBox _listBox;

        public bool IsEditable
        {
            get => !_editBox.IsReadOnly;
            set => _editBox.IsReadOnly = !value;
        }

        public Image TextBoxBackgroundImage
        {
            get => _editBox.BackgroundImage;
            set => _editBox.BackgroundImage = value;
        }

        public Image TextBoxHoverBackgroundImage
        {
            get => _editBox.HoverBackgroundImage;
            set => _editBox.HoverBackgroundImage = value;
        }

        public Image TextBoxDisabledBackgroundImage
        {
            get => _editBox.DisabledBackgroundImage;
            set => _editBox.DisabledBackgroundImage = value;
        }

        public ColorRgbaF ListBoxBackgroundColor
        {
            get => _listBox.BackgroundColor;
            set => _listBox.BackgroundColor = value;
        }

        public ColorRgbaF ListBoxBorderColor
        {
            get => _listBox.BorderColor;
            set => _listBox.BorderColor = value;
        }

        public Image ListBoxDisabledBackgroundImage
        {
            get => _listBox.DisabledBackgroundImage;
            set => _listBox.DisabledBackgroundImage = value;
        }

        public Image DropDownButtonImage
        {
            get => _dropDownButton.BackgroundImage;
            set => _dropDownButton.BackgroundImage = value;
        }

        public bool IsDropDownOpen
        {
            get => _listBox.Visible;
            set => _listBox.Visible = value;
        }

        public ListBoxDataItem[] Items
        {
            get => _listBox.Items;
            set
            {
                _listBox.Items = value;
                _listBox.IsScrollBarVisible = MaxDisplay < value.Length;
            }
        }

        public int SelectedIndex
        {
            get => _listBox.SelectedIndex;
            set => _listBox.SelectedIndex = value;
        }

        public Image DropDownUpButtonImage
        {
            get => _listBox.UpButtonImage;
            set => _listBox.UpButtonImage = value;
        }

        public Image DropDownUpButtonHoverImage
        {
            get => _listBox.UpButtonHoverImage;
            set => _listBox.UpButtonHoverImage = value;
        }

        public Image DropDownDownButtonImage
        {
            get => _listBox.DownButtonImage;
            set => _listBox.DownButtonImage = value;
        }

        public Image DropDownDownButtonHoverImage
        {
            get => _listBox.DownButtonHoverImage;
            set => _listBox.DownButtonHoverImage = value;
        }

        public Image DropDownThumbImage
        {
            get => _listBox.ThumbImage;
            set => _listBox.ThumbImage = value;
        }

        public Image DropDownThumbHoverImage
        {
            get => _listBox.ThumbHoverImage;
            set => _listBox.ThumbHoverImage = value;
        }

        public Image DropDownSelectedItemBackgroundImage
        {
            get => _listBox.SelectedItemBackgroundImage;
            set => _listBox.SelectedItemBackgroundImage = value;
        }

        public Image DropDownSelectedItemHoverBackgroundImage
        {
            get => _listBox.SelectedItemHoverBackgroundImage;
            set => _listBox.SelectedItemHoverBackgroundImage = value;
        }
        
        public int MaxDisplay
        {
            get => _listBox.MaxDisplay;
            set =>_listBox.MaxDisplay = value;
        }

        public override Font Font
        {
            set
            {
                base.Font = value;
                _listBox.Font = value;
                _editBox.Font = value;
            }
        }

        public override ColorRgbaF TextColor
        {
            set
            {
                base.TextColor = value;
                _listBox.TextColor = value;
                _editBox.TextColor = value;
            }
        }

        public ComboBox(WndWindowDefinition wndWindow, WndImageLoader imageLoader)
            : this()
        {
            IsEditable = wndWindow.ComboBoxData.IsEditable;
            MaxDisplay = wndWindow.ComboBoxData.MaxDisplay;

            TextBoxBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.ComboBoxEditBoxEnabledDrawData, 0, 2, 1);
            TextBoxHoverBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.ComboBoxEditBoxHiliteDrawData, 0, 2, 1);
            TextBoxDisabledBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.ComboBoxEditBoxDisabledDrawData, 0, 2, 1);

            DropDownSelectedItemBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.ComboBoxListBoxEnabledDrawData, 1, 3, 2);
            DropDownSelectedItemHoverBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.ComboBoxListBoxHiliteDrawData, 1, 3, 2);
            ListBoxDisabledBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.ComboBoxListBoxDisabledDrawData, 1, 3, 2);

            DropDownButtonImage = imageLoader.CreateNormalImage(wndWindow.ComboBoxDropDownButtonEnabledDrawData, 0);

            DropDownUpButtonImage = imageLoader.CreateNormalImage(wndWindow.ListBoxEnabledUpButtonDrawData, 0);
            DropDownUpButtonHoverImage = imageLoader.CreateNormalImage(wndWindow.ListBoxHiliteUpButtonDrawData, 0);

            DropDownDownButtonImage = imageLoader.CreateNormalImage(wndWindow.ListBoxEnabledDownButtonDrawData, 0);
            DropDownDownButtonHoverImage = imageLoader.CreateNormalImage(wndWindow.ListBoxHiliteDownButtonDrawData, 0);

            DropDownThumbImage = imageLoader.CreateNormalImage(wndWindow.SliderThumbEnabledDrawData, 0);
            DropDownThumbHoverImage = imageLoader.CreateNormalImage(wndWindow.SliderThumbHiliteDrawData, 0);

            if (wndWindow.ComboBoxListBoxEnabledDrawData.Items != null && wndWindow.ComboBoxListBoxEnabledDrawData.Items.Length > 0)
            {
                ListBoxBackgroundColor = wndWindow.ComboBoxListBoxEnabledDrawData.Items[0].Color.ToColorRgbaF();
                ListBoxBorderColor = wndWindow.ComboBoxListBoxEnabledDrawData.Items[0].BorderColor.ToColorRgbaF();
            }
        }

        public ComboBox()
        {
            _editBox = new TextBox();
            _editBox.Click += OnDropDownButtonClick;
            Controls.Add(_editBox);

            _dropDownButton = new Button();
            _dropDownButton.Click += OnDropDownButtonClick;
            Controls.Add(_dropDownButton);

            _listBox = new ListBox(new[] { 100 });
            _listBox.SelectedIndexChanged += OnSelectedIndexChanged;
            _listBox.Visible = false;
            Controls.Add(_listBox);
        }

        public override bool HitTest(in Point2D windowPoint)
        {
            if (!Enabled || !Visible || Opacity != 1)
            {
                return false;
            }

            var clientPoint = PointToClient(windowPoint);
            if (!IsDropDownOpen)
            {
                return ClientRectangle.Contains(clientPoint);
            }

            return ClientRectangle.Contains(clientPoint) || _listBox.Bounds.Contains(clientPoint);
        }

        private void OnDropDownButtonClick(object sender, EventArgs e)
        {
            bool nextState = !IsDropDownOpen;
            IsDropDownOpen = nextState;
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            _editBox.Text = SelectedIndex != -1
                ? Items[SelectedIndex].ColumnData[0]
                : null;
            _editBox.TextColor = Items[SelectedIndex].TextColor;
            IsDropDownOpen = false;
        }

        protected override void LayoutOverride()
        {
            var expandButtonWidth = _dropDownButton.GetPreferredSize(ClientSize).Width;
            _dropDownButton.Bounds = new Rectangle(
                ClientRectangle.Right - expandButtonWidth,
                0,
                expandButtonWidth,
                ClientSize.Height);

            _editBox.Bounds = new Rectangle(
                0,
                0,
                _dropDownButton.Bounds.X,
                ClientSize.Height);

            var itemsHeight = MaxDisplay >= Items.Length ? Items.Sum(i => i.ListBoxItemHeight) : MaxDisplay * Items.First().ListBoxItemHeight;
            _listBox.Bounds = new Rectangle(
                0,
                ClientSize.Height,
                ClientSize.Width,
                itemsHeight);
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            _listBox.SelectedIndexChanged -= OnSelectedIndexChanged;
            _dropDownButton.Click -= OnDropDownButtonClick;

            base.Dispose(disposeManagedResources);
        }
    }
}
