using System;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Mathematics;
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
            set => _listBox.Items = value;
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

        public override Font Font
        {
            set
            {
                base.Font = value;
                _listBox.Font = value;
            }
        }

        public override ColorRgbaF TextColor
        {
            set
            {
                base.TextColor = value;
                _listBox.TextColor = value;
            }
        }

        public ComboBox()
        {
            _editBox = new TextBox();
            Controls.Add(_editBox);

            _dropDownButton = new Button();
            _dropDownButton.Click += OnDropDownButtonClick;
            Controls.Add(_dropDownButton);

            _listBox = new ListBox();
            _listBox.SelectedIndexChanged += OnSelectedIndexChanged;
            _listBox.Visible = false;
            Controls.Add(_listBox);
        }

        private void OnDropDownButtonClick(object sender, EventArgs e)
        {
            IsDropDownOpen = true;
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            _editBox.Text = SelectedIndex != -1
                ? Items[SelectedIndex].ColumnData[0]
                : null;
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

            _listBox.Bounds = new Rectangle(
                0,
                ClientSize.Height,
                ClientSize.Width,
                200); // TODO
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            _listBox.SelectedIndexChanged -= OnSelectedIndexChanged;
            _dropDownButton.Click -= OnDropDownButtonClick;

            base.Dispose(disposeManagedResources);
        }
    }
}
