using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenSage.Data;
using OpenSage.Data.Csf;
using OpenSage.Data.Ini;
using OpenSage.Data.Tga;
using OpenSage.Data.Wnd;
using OpenSage.DataViewer.Framework.Converters;
using OpenSage.Graphics;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class WndFileContentViewModel : FileContentViewModel
    {
        private readonly FileSystem _fileSystem;
        private readonly CsfFile _csfFile;
        private readonly List<MappedImage> _mappedImages;

        public Canvas ContainerView { get; }

        public WndFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            _fileSystem = file.FileSystem;

            _csfFile = CsfFile.FromFileSystemEntry(_fileSystem.GetFile(@"Data\English\generals.csf"));

            var iniDataContext = new IniDataContext();
            foreach (var iniFileEntry in _fileSystem.GetFiles(@"Data\INI\MappedImages\HandCreated\"))
            {
                iniDataContext.LoadIniFile(iniFileEntry);
            }
            foreach (var iniFileEntry in _fileSystem.GetFiles(@"Data\INI\MappedImages\TextureSize_512\"))
            {
                iniDataContext.LoadIniFile(iniFileEntry);
            }
            _mappedImages = iniDataContext.MappedImages;

            var wndFile = WndFile.FromFileSystemEntry(file);

            ContainerView = new NonInheritingCanvas
            {
                Width = wndFile.RootWindow.ScreenRect.CreationResolution.Width,
                Height = wndFile.RootWindow.ScreenRect.CreationResolution.Height,
            };
            AddWindow(wndFile.RootWindow);
        }

        private sealed class NonInheritingCanvas : Canvas
        {
            public NonInheritingCanvas()
            {
                InheritanceBehavior = InheritanceBehavior.SkipAllNext;
            }
        }

        private void AddWindow(WndWindow window)
        {
            var windowText = (!string.IsNullOrEmpty(window.Text))
                ? _csfFile.Lookup(window.Text)
                : string.Empty;

            FrameworkElement contentElement;
            switch (window.WindowType)
            {
                case WndWindowType.PushButton:
                    var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
                    contentPresenterFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    contentPresenterFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                    contentElement = new Button
                    {
                        Content = windowText,
                        Background = Brushes.Transparent,
                        BorderBrush = null,
                        Template = new ControlTemplate(typeof(Button))
                        {
                            VisualTree = contentPresenterFactory
                        }
                    };
                    break;

                case WndWindowType.StaticText:
                    contentElement = new TextBlock { Text = windowText, TextWrapping = TextWrapping.Wrap };
                    break;

                case WndWindowType.CheckBox:
                    contentElement = new CheckBox { Content = windowText };
                    break;

                case WndWindowType.RadioButton:
                    contentElement = new RadioButton { GroupName = "Group" + window.RadioButtonData.Group, Content = windowText };
                    break;

                case WndWindowType.TextEntry:
                    contentElement = new TextBox { Text = windowText, Background = Brushes.Transparent };
                    break;

                case WndWindowType.ProgressBar:
                    contentElement = new ProgressBar { Value = 50 };
                    break;

                case WndWindowType.ComboBox:
                    contentElement = new ComboBox { Items = { new ComboBoxItem { Content = "Test" } }, Background = Brushes.Transparent };
                    break;

                case WndWindowType.HorizontalSlider:
                    contentElement = new Slider { Orientation = Orientation.Horizontal, Minimum = window.SliderData.MinValue, Maximum = window.SliderData.MaxValue };
                    break;

                case WndWindowType.VerticalSlider:
                    contentElement = new Slider { Orientation = Orientation.Vertical, Minimum = window.SliderData.MinValue, Maximum = window.SliderData.MaxValue };
                    break;

                case WndWindowType.ListBox:
                    contentElement = new ListBox { Items = { new ListBoxItem { Content = "Item 1" }, new ListBoxItem { Content = "Item 2" } }, Background = Brushes.Transparent };
                    break;

                case WndWindowType.GenericWindow:
                    contentElement = null;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var windowElement = CreateWindowElement(window, contentElement);
            ContainerView.Children.Add(windowElement);

            foreach (var childWindow in window.ChildWindows)
            {
                AddWindow(childWindow);
            }
        }

        private UIElement CreateWindowElement(WndWindow window, FrameworkElement contentElement)
        {
            var result = new Border
            {
                Name = window.Name.Replace(".", string.Empty).Replace(":", string.Empty),
                DataContext = window
            };

            var style = new Style(typeof(Border));

            var hasBackground = !window.Status.HasFlag(WndWindowStatusFlags.Image);

            style.Setters.Add(new Setter(TextBlock.ForegroundProperty, new Binding("TextColor.Enabled") { Converter = WndColorToBrushConverter.Instance }));
            if (hasBackground)
            {
                style.Setters.Add(new Setter(Border.BackgroundProperty, new Binding("EnabledDrawData.Items[0].Color") { Converter = WndColorToBrushConverter.Instance }));
                style.Setters.Add(new Setter(Border.BorderBrushProperty, new Binding("EnabledDrawData.Items[0].BorderColor") { Converter = WndColorToBrushConverter.Instance }));
            }
            else
                result.Background = Brushes.Transparent;

            var trigger = new Trigger
            {
                Property = UIElement.IsMouseOverProperty,
                Value = true,
            };

            trigger.Setters.Add(new Setter(TextBlock.ForegroundProperty, new Binding("TextColor.Hilite") { Converter = WndColorToBrushConverter.Instance }));
            if (hasBackground)
            {
                trigger.Setters.Add(new Setter(Border.BackgroundProperty, new Binding("HiliteDrawData.Items[0].Color") { Converter = WndColorToBrushConverter.Instance }));
                trigger.Setters.Add(new Setter(Border.BorderBrushProperty, new Binding("HiliteDrawData.Items[0].BorderColor") { Converter = WndColorToBrushConverter.Instance }));
            }

            style.Triggers.Add(trigger);

            result.Style = style;

            if (hasBackground || window.Status.HasFlag(WndWindowStatusFlags.Border))
            {
                result.BorderThickness = new Thickness(1);
            }

            if (window.Status.HasFlag(WndWindowStatusFlags.Hidden) || window.InputCallback == "GameWinBlockInput")
            {
                result.Visibility = Visibility.Hidden;
            }

            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            //if (result.Name != "MainMenuwndEarthMap2" && result.Name != "MainMenuwndEarthMap")
            {
                switch (window.WindowType)
                {
                    case WndWindowType.GenericWindow:
                        grid.Children.Add(CreateImage(result, window, 0, Stretch.Fill));
                        Grid.SetColumnSpan(grid.Children[0], 3);
                        break;

                    case WndWindowType.PushButton:
                        grid.Children.Add(CreateImage(result, window, 0));
                        Grid.SetColumn(grid.Children[0], 0);

                        grid.Children.Add(CreateImage(result, window, 5, Stretch.Fill));
                        Grid.SetColumn(grid.Children[1], 1);

                        grid.Children.Add(CreateImage(result, window, 6));
                        Grid.SetColumn(grid.Children[2], 2);

                        break;
                }
            }

            if (contentElement != null)
            {
                Grid.SetColumnSpan(contentElement, 3);
                grid.Children.Add(contentElement);
            }

            result.Child = grid;

            var screenRect = window.ScreenRect;

            Canvas.SetLeft(result, screenRect.UpperLeft.X);
            Canvas.SetTop(result, screenRect.UpperLeft.Y);

            result.Width = screenRect.BottomRight.X - screenRect.UpperLeft.X;
            result.Height = screenRect.BottomRight.Y - screenRect.UpperLeft.Y;

            TextBlock.SetFontFamily(result, new FontFamily(window.Font.Name));
            TextBlock.SetFontWeight(result, window.Font.Bold ? FontWeights.Bold : FontWeights.Normal);
            TextBlock.SetFontSize(result, window.Font.Size);

            return result;
        }

        private Image CreateImage(FrameworkElement parentElement, WndWindow window, int drawDataIndex, Stretch? stretch = null)
        {
            var image = new Image();

            if (stretch != null)
            {
                image.Stretch = stretch.Value;
            };

            var style = new Style(typeof(Image));

            style.Setters.Add(new Setter(Image.SourceProperty, CreateImageSource(window.EnabledDrawData.Items[drawDataIndex].Image)));

            var hoverImageSource = CreateImageSource(window.HiliteDrawData.Items[drawDataIndex].Image);
            if (hoverImageSource != null)
            {
                style.Triggers.Add(new DataTrigger
                {
                    Binding = new Binding("IsMouseOver") { Source = parentElement },
                    Value = true,
                    Setters =
                    {
                        new Setter(Image.SourceProperty, hoverImageSource),
                    }
                });
            }

            image.Style = style;

            return image;
        }

        private ImageSource CreateImageSource(string image)
        {
            if (string.IsNullOrEmpty(image) || image == "NoImage")
            {
                return null;
            }

            var mappedImage = _mappedImages.FirstOrDefault(x => x.Name == image);
            if (mappedImage == null)
            {
                return null;
            }

            var tgaFileEntry = _fileSystem.SearchFile(
                mappedImage.Texture,
                @"Data\English\Art\Textures\",
                @"Art\Textures\");

            var tgaFile = TgaFile.FromFileSystemEntry(tgaFileEntry);

            var pixelFormat = tgaFile.Header.ImagePixelSize == 32
                ? PixelFormats.Bgra32
                : PixelFormats.Bgr24;

            var stride = tgaFile.Header.ImagePixelSize == 32
                ? tgaFile.Header.Width * 4
                : tgaFile.Header.Width * 3;

            var bitmapSource = BitmapSource.Create(
                tgaFile.Header.Width,
                tgaFile.Header.Height,
                96,
                96,
                pixelFormat,
                null,
                tgaFile.Data,
                stride);

            var sourceRect = new Int32Rect(
                mappedImage.Coords.Left,
                mappedImage.Coords.Top,
                mappedImage.Coords.Right - mappedImage.Coords.Left,
                mappedImage.Coords.Bottom - mappedImage.Coords.Top);

            return new CroppedBitmap(
                bitmapSource,
                sourceRect);
        }
    }
}
