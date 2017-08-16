using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using OpenSage.Data;
using OpenSage.Data.Wnd;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class WndFileContentViewModel : FileContentViewModel
    {
        public Canvas ContainerView { get; }

        public WndFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            WndFile wndFile;
            using (var fileStream = file.Open())
                wndFile = WndFile.FromStream(fileStream);

            ContainerView = new Canvas
            {
                Width = wndFile.RootWindow.ScreenRect.CreationResolution.Width,
                Height = wndFile.RootWindow.ScreenRect.CreationResolution.Height
            };
            AddWindow(wndFile.RootWindow);
        }

        private void AddWindow(WndWindow window)
        {
            FrameworkElement result;
            switch (window.WindowType)
            {
                case WndWindowType.PushButton:
                    result = new Button { Content = window.Text };
                    break;

                case WndWindowType.StaticText:
                    result = new TextBlock { Text = window.Text };
                    break;

                case WndWindowType.CheckBox:
                    result = new CheckBox();
                    break;

                case WndWindowType.RadioButton:
                    result = new RadioButton { GroupName = "Group" + window.RadioButtonData.Group };
                    break;

                case WndWindowType.TextEntry:
                    result = new TextBox { Text = window.Text };
                    break;

                case WndWindowType.ProgressBar:
                    result = new ProgressBar { Value = 50 };
                    break;

                case WndWindowType.ComboBox:
                    result = new ComboBox { Items = { new ComboBoxItem { Content = "Test" } } };
                    break;

                case WndWindowType.HorizontalSlider:
                    result = new Slider { Orientation = Orientation.Horizontal, Minimum = window.SliderData.MinValue, Maximum = window.SliderData.MaxValue };
                    break;

                case WndWindowType.VerticalSlider:
                    result = new Slider { Orientation = Orientation.Vertical, Minimum = window.SliderData.MinValue, Maximum = window.SliderData.MaxValue };
                    break;

                case WndWindowType.ListBox:
                    result = new ListBox { Items = { new ListBoxItem { Content = "Item 1" }, new ListBoxItem { Content = "Item 2" } } };
                    break;

                case WndWindowType.GenericWindow:
                    result = new Canvas();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var screenRect = window.ScreenRect;

            var border = new WindowBorder(window);
            border.Child = result;

            Canvas.SetLeft(border, screenRect.UpperLeft.X);
            Canvas.SetTop(border, screenRect.UpperLeft.Y);

            border.Width = screenRect.BottomRight.X - screenRect.UpperLeft.X;
            border.Height = screenRect.BottomRight.Y - screenRect.UpperLeft.Y;

            TextBlock.SetFontFamily(border, new FontFamily(window.Font.Name));
            TextBlock.SetFontWeight(border, window.Font.Bold ? FontWeights.Bold : FontWeights.Normal);
            TextBlock.SetFontSize(border, window.Font.Size);

            ContainerView.Children.Add(border);

            foreach (var childWindow in window.ChildWindows)
            {
                AddWindow(childWindow);
            }
        }

        private sealed class WindowBorder : Border
        {
            public WindowBorder(WndWindow window)
            {
                DataContext = window;

                Background = Brushes.Transparent; // So that hit testing works for IsMouseOver.

                var style = new Style(typeof(Border));

                style.Setters.Add(new Setter(BorderBrushProperty, new Binding("EnabledDrawData.Items[0].BorderColor") { Converter = WndColorToBrushConverter.Instance }));

                style.Triggers.Add(new Trigger
                {
                    Property = IsMouseOverProperty,
                    Value = true,
                    Setters =
                    {
                        new Setter(BorderBrushProperty, new Binding("HiliteDrawData.Items[0].BorderColor") { Converter = WndColorToBrushConverter.Instance })
                    }
                });

                Style = style;

                if (window.Status.HasFlag(WndWindowStatusFlags.Border))
                {
                    BorderThickness = new Thickness(1);
                }
            }
        }

        private sealed class WndColorToBrushConverter : IValueConverter
        {
            public static readonly IValueConverter Instance = new WndColorToBrushConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var c = (WndColor) value;
                return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
