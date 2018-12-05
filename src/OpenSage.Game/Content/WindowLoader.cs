using System.Linq;
using OpenSage.Content.Util;
using OpenSage.Data;
using OpenSage.Data.Wnd;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;

namespace OpenSage.Content
{
    internal sealed class WindowLoader : ContentLoader<Window>
    {
        private readonly WndCallbackResolver _wndCallbackResolver;

        public WindowLoader(ContentManager contentManager, WndCallbackResolver wndCallbackResolver, string language)
        {
            _wndCallbackResolver = wndCallbackResolver;

            switch (contentManager.SageGame)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                    contentManager.IniDataContext.LoadIniFile($@"Data\{language}\HeaderTemplate.ini");
                    break;

                case SageGame.Bfme:
                    contentManager.IniDataContext.LoadIniFile($@"Lang\{language}\HeaderTemplate.ini");
                    break;
                case SageGame.Bfme2:
                case SageGame.Bfme2Rotwk:
                    contentManager.IniDataContext.LoadIniFile(@"HeaderTemplate.ini");
                    break;
            }

            contentManager.IniDataContext.LoadIniFiles(@"Data\INI\MappedImages\HandCreated\");
            contentManager.IniDataContext.LoadIniFiles(@"Data\INI\MappedImages\TextureSize_512\");
        }

        protected override Window LoadEntry(FileSystemEntry entry, ContentManager contentManager, Game game, LoadOptions loadOptions)
        {
            var wndFile = WndFile.FromFileSystemEntry(entry);

            var rootControl = CreateElementRecursive(
                wndFile.RootWindow,
                contentManager,
                _wndCallbackResolver,
                wndFile.RootWindow.ScreenRect.UpperLeft);

            return new Window(wndFile.RootWindow.ScreenRect.CreationResolution, rootControl, contentManager)
            {
                Game = game,
                Bounds = wndFile.RootWindow.ScreenRect.ToRectangle(),
                LayoutInit = _wndCallbackResolver.GetWindowCallback(wndFile.LayoutBlock.LayoutInit),
                LayoutUpdate = _wndCallbackResolver.GetWindowCallback(wndFile.LayoutBlock.LayoutUpdate),
                LayoutShutdown = _wndCallbackResolver.GetWindowCallback(wndFile.LayoutBlock.LayoutShutdown)
            };
        }

        private static Control CreateElementRecursive(WndWindowDefinition wndWindow, ContentManager contentManager, WndCallbackResolver wndCallbackResolver, Point2D parentOffset)
        {
            var imageLoader = contentManager.WndImageLoader;

            Control createControl()
            {
                switch (wndWindow.WindowType)
                {
                    case WndWindowType.CheckBox:
                        return new CheckBox
                        {
                            UncheckedImage = imageLoader.CreateNormalImage(wndWindow.EnabledDrawData, 1),
                            CheckedImage = imageLoader.CreateNormalImage(wndWindow.EnabledDrawData, 2),

                            HoverUncheckedImage = imageLoader.CreateNormalImage(wndWindow.HiliteDrawData, 1),
                            HoverCheckedImage = imageLoader.CreateNormalImage(wndWindow.HiliteDrawData, 2),

                            DisabledUncheckedImage = imageLoader.CreateNormalImage(wndWindow.DisabledDrawData, 1),
                            DisabledCheckedImage = imageLoader.CreateNormalImage(wndWindow.DisabledDrawData, 2)
                        };

                    case WndWindowType.ComboBox:
                        var combobox = new ComboBox
                        {
                            IsEditable = wndWindow.ComboBoxData.IsEditable,
                            MaxDisplay = wndWindow.ComboBoxData.MaxDisplay,

                            TextBoxBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.ComboBoxEditBoxEnabledDrawData, 0, 2, 1),
                            TextBoxHoverBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.ComboBoxEditBoxHiliteDrawData, 0, 2, 1),
                            TextBoxDisabledBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.ComboBoxEditBoxDisabledDrawData, 0, 2, 1),

                            DropDownSelectedItemBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.ComboBoxListBoxEnabledDrawData, 1, 3, 2),
                            DropDownSelectedItemHoverBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.ComboBoxListBoxHiliteDrawData, 1, 3, 2),
                            ListBoxDisabledBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.ComboBoxListBoxDisabledDrawData, 1, 3, 2),

                            DropDownButtonImage = imageLoader.CreateNormalImage(wndWindow.ComboBoxDropDownButtonEnabledDrawData, 0),

                            DropDownUpButtonImage = imageLoader.CreateNormalImage(wndWindow.ListBoxEnabledUpButtonDrawData, 0),
                            DropDownUpButtonHoverImage = imageLoader.CreateNormalImage(wndWindow.ListBoxHiliteUpButtonDrawData, 0),

                            DropDownDownButtonImage = imageLoader.CreateNormalImage(wndWindow.ListBoxEnabledDownButtonDrawData, 0),
                            DropDownDownButtonHoverImage = imageLoader.CreateNormalImage(wndWindow.ListBoxHiliteDownButtonDrawData, 0),

                            DropDownThumbImage = imageLoader.CreateNormalImage(wndWindow.SliderThumbEnabledDrawData, 0),
                            DropDownThumbHoverImage = imageLoader.CreateNormalImage(wndWindow.SliderThumbHiliteDrawData, 0),
                        };

                        if (wndWindow.ComboBoxListBoxEnabledDrawData.Items != null && wndWindow.ComboBoxListBoxEnabledDrawData.Items.Length > 0)
                        {
                            combobox.ListBoxBackgroundColor =
                                wndWindow.ComboBoxListBoxEnabledDrawData.Items[0].Color.ToColorRgbaF();
                            combobox.ListBoxBorderColor =
                                wndWindow.ComboBoxListBoxEnabledDrawData.Items[0].BorderColor.ToColorRgbaF();
                        }

                        return combobox;

                    case WndWindowType.HorizontalSlider:
                        return new Slider
                        {
                            HighlightedBoxImage = imageLoader.CreateNormalImage(wndWindow.DisabledDrawData, 0),
                            UnhighlightedBoxImage = imageLoader.CreateNormalImage(wndWindow.DisabledDrawData, 1),

                            MinimumValue = wndWindow.SliderData.MinValue,
                            MaximumValue = wndWindow.SliderData.MaxValue,

                            Value = wndWindow.SliderData.MinValue + (wndWindow.SliderData.MaxValue - wndWindow.SliderData.MinValue) / 2
                        };

                    case WndWindowType.ListBox:
                        var listBox = new ListBox
                        {
                            BorderColor = wndWindow.EnabledDrawData.Items[0].BorderColor.ToColorRgbaF(),
                            BorderWidth = 1,

                            SelectedItemBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.EnabledDrawData, 1, 3, 2),
                            SelectedItemHoverBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.HiliteDrawData, 1, 3, 2),

                            ColumnWidths = wndWindow.ListBoxData.ColumnWidths,

                            IsScrollBarVisible = wndWindow.ListBoxData.ScrollBar
                        };
                        if (wndWindow.ListBoxData.ScrollBar)
                        {
                            listBox.UpButtonImage = imageLoader.CreateNormalImage(wndWindow.ListBoxEnabledUpButtonDrawData, 0);
                            listBox.UpButtonHoverImage = imageLoader.CreateNormalImage(wndWindow.ListBoxHiliteUpButtonDrawData, 0);

                            listBox.DownButtonImage = imageLoader.CreateNormalImage(wndWindow.ListBoxEnabledDownButtonDrawData, 0);
                            listBox.DownButtonHoverImage = imageLoader.CreateNormalImage(wndWindow.ListBoxHiliteDownButtonDrawData, 0);

                            listBox.ThumbImage = imageLoader.CreateNormalImage(wndWindow.SliderThumbEnabledDrawData, 0);
                            listBox.ThumbHoverImage = imageLoader.CreateNormalImage(wndWindow.SliderThumbHiliteDrawData, 0);
                        };
                        if (wndWindow.ListBoxData.ForceSelect)
                        {
                            listBox.SelectedIndex = 0;
                        }
                        return listBox;

                    case WndWindowType.PushButton:
                        return new Button
                        {
                            BackgroundImage = imageLoader.CreateStretchableImage(wndWindow.EnabledDrawData, 0, 5, 6),
                            HoverBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.HiliteDrawData, 0, 5, 6),
                            DisabledBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.DisabledDrawData, 0, 5, 6),
                            PushedBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.HiliteDrawData, 1, 3, 4),

                            HoverTextColor = wndWindow.TextColor.Hilite.ToColorRgbaF(),
                            DisabledTextColor = wndWindow.TextColor.Disabled.ToColorRgbaF()
                        };

                    case WndWindowType.StaticText:
                        return new Label
                        {
                            TextAlignment = wndWindow.StaticTextData.Centered
                                ? TextAlignment.Center
                                : TextAlignment.Leading
                        };

                    case WndWindowType.EntryField:
                        return new TextBox
                        {
                            BackgroundImage = imageLoader.CreateStretchableImage(wndWindow.EnabledDrawData, 0, 2, 1),
                            HoverBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.HiliteDrawData, 0, 2, 1),
                            DisabledBackgroundImage = imageLoader.CreateStretchableImage(wndWindow.DisabledDrawData, 0, 2, 1),

                            HoverTextColor = wndWindow.TextColor.Hilite.ToColorRgbaF(),
                            DisabledTextColor = wndWindow.TextColor.Disabled.ToColorRgbaF()
                        };

                    default: // TODO: Implement other window types.
                        {
                            var control = new Control();
                            if (wndWindow.Status.HasFlag(WndWindowStatusFlags.Image))
                            {
                                control.BackgroundImage = imageLoader.CreateNormalImage(wndWindow.EnabledDrawData, 0);
                            }
                            else
                            {
                                control.BackgroundColor = wndWindow.EnabledDrawData.Items[0].Color.ToColorRgbaF();
                            }
                            if (control.BackgroundImage == null)
                            {
                                control.BorderColor = wndWindow.EnabledDrawData.Items[0].BorderColor.ToColorRgbaF();
                                control.BorderWidth = 1;
                            }
                            return control;
                        }
                }
            }

            var result = createControl();

            result.Name = wndWindow.Name;

            var wndRectangle = wndWindow.ScreenRect.ToRectangle();
            result.Bounds = new Rectangle(
                wndRectangle.X - parentOffset.X,
                wndRectangle.Y - parentOffset.Y,
                wndRectangle.Width,
                wndRectangle.Height);

            var systemCallback = wndCallbackResolver.GetControlCallback(wndWindow.SystemCallback);
            if (systemCallback != null)
            {
                result.SystemCallback = systemCallback;
            }

            var inputCallback = wndCallbackResolver.GetControlCallback(wndWindow.InputCallback);
            if (inputCallback != null)
            {
                result.InputCallback = inputCallback;
            }

            var drawCallback = wndCallbackResolver.GetControlDrawCallback(wndWindow.DrawCallback);
            if (drawCallback != null)
            {
                result.DrawCallback = drawCallback;
            }

            // TODO: TooltipCallback

            result.Visible = !wndWindow.Status.HasFlag(WndWindowStatusFlags.Hidden);

            if (wndWindow.Status.HasFlag(WndWindowStatusFlags.SeeThru))
            {
                result.BackgroundColor = ColorRgbaF.Transparent;
                result.BorderColor = ColorRgbaF.Transparent;
            }

            if (wndWindow.HasHeaderTemplate)
            {
                var headerTemplate = contentManager.IniDataContext.HeaderTemplates.First(x => x.Name == wndWindow.HeaderTemplate);
                result.Font = contentManager.GetOrCreateFont(headerTemplate.Font, headerTemplate.Point, headerTemplate.Bold ? FontWeight.Bold : FontWeight.Normal);
            }
            else
            {
                result.Font = contentManager.GetOrCreateFont(wndWindow.Font.Name, wndWindow.Font.Size, wndWindow.Font.Bold ? FontWeight.Bold : FontWeight.Normal);
            }

            result.TextColor = wndWindow.TextColor.Enabled.ToColorRgbaF();

            result.Text = contentManager.TranslationManager.Lookup(wndWindow.Text);

            // TODO: TextBorderColor

            foreach (var childWindow in wndWindow.ChildWindows)
            {
                var child = CreateElementRecursive(childWindow, contentManager, wndCallbackResolver, wndRectangle.Location);
                result.Controls.Add(child);
            }

            return result;
        }
    }
}
