using System;
using System.IO;
using System.Linq;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Content;
using OpenSage.Content.Util;
using OpenSage.Data.Wnd;
using OpenSage.Graphics;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Elements
{
    public abstract class UIElement : DisposableBase
    {
        private bool _initialized;

        private WndWindow _wndWindow;

        private Texture _texture;
        private RenderTarget _renderTarget;

        private Texture _imageTexture;

        private SpriteBatch _spriteBatch;
        private DrawingContext _drawingContext;

        private ColorRgbaF? _backgroundColor;
        private ColorRgbaF? _borderColor;
        private StretchableImage _image;

        private string _text;

        protected string Text => _text;

        private bool _needsRender;

        public Rectangle Frame { get; private set; }

        public string Name => _wndWindow.Name;

        public Texture Texture => _texture;

        public bool Hidden { get; private set; }

        public UIElement Parent { get; internal set; }

        public UIElementCollection Children { get; } = new UIElementCollection();

        private bool _highlighted;
        public bool Highlighted
        {
            get => _highlighted;
            set
            {
                _highlighted = value;
                _needsRender = true;
            }
        }

        public void Initialize(WndWindow wndWindow, ContentManager contentManager)
        {
            if (_initialized)
            {
                throw new InvalidOperationException();
            }

            _wndWindow = wndWindow;
            _needsRender = true;

            _spriteBatch = AddDisposable(new SpriteBatch(contentManager));
            _drawingContext = new DrawingContext(_spriteBatch);

            _text = contentManager.TranslationManager.Lookup(wndWindow.Text);

            if (!wndWindow.Status.HasFlag(WndWindowStatusFlags.Image))
            {
                _backgroundColor = wndWindow.EnabledDrawData.Items[0].Color.ToColorRgbaF();
            }

            if (wndWindow.Status.HasFlag(WndWindowStatusFlags.Border))
            {
                _borderColor = wndWindow.EnabledDrawData.Items[0].BorderColor.ToColorRgbaF();
            }

            if (wndWindow.Status.HasFlag(WndWindowStatusFlags.Image))
            {
                _image = LoadImage(wndWindow, contentManager);

                if (_image != null)
                {
                    _imageTexture = AddDisposable(_image.RenderToTexture(contentManager.GraphicsDevice, _spriteBatch));
                }
            }

            Hidden = wndWindow.Status.HasFlag(WndWindowStatusFlags.Hidden);

            // TODO

            _initialized = true;
        }

        public void CreateSizeDependentResources(ContentManager contentManager, in Size windowSize)
        {
            Frame = CalculateFrame(_wndWindow.ScreenRect, windowSize);

            RemoveAndDispose(ref _renderTarget);
            RemoveAndDispose(ref _texture);

            _texture = AddDisposable(Texture.CreateTexture2D(
                contentManager.GraphicsDevice,
                PixelFormat.Rgba8UNorm,
                Frame.Width,
                Frame.Height,
                TextureBindFlags.ShaderResource | TextureBindFlags.RenderTarget));

            _renderTarget = AddDisposable(new RenderTarget(
                contentManager.GraphicsDevice,
                _texture));

            _needsRender = true;
        }

        private static Rectangle CalculateFrame(in WndScreenRect wndScreenRect, in Size viewportSize)
        {
            // Figure out the ratio.
            var ratioX = viewportSize.Width / (float) wndScreenRect.CreationResolution.Width;
            var ratioY = viewportSize.Height / (float) wndScreenRect.CreationResolution.Height;

            // Use whichever multiplier is smaller.
            var ratio = ratioX < ratioY ? ratioX : ratioY;

            var originalWidth = wndScreenRect.BottomRight.X - wndScreenRect.UpperLeft.X;
            var originalHeight = wndScreenRect.BottomRight.Y - wndScreenRect.UpperLeft.Y;

            // Now we can get the new height and width
            var newWidth = Convert.ToInt32(originalWidth * ratio);
            var newHeight = Convert.ToInt32(originalHeight * ratio);

            var newX = Convert.ToInt32(wndScreenRect.UpperLeft.X * ratio);
            var newY = Convert.ToInt32(wndScreenRect.UpperLeft.Y * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero)
            var posX = Convert.ToInt32((viewportSize.Width - (wndScreenRect.CreationResolution.Width * ratio)) / 2) + newX;
            var posY = Convert.ToInt32((viewportSize.Height - (wndScreenRect.CreationResolution.Height * ratio)) / 2) + newY;

            return new Rectangle(posX, posY, newWidth, newHeight);
        }

        private bool _isMouseOver;
        public bool IsMouseOver
        {
            get => _isMouseOver;
            set
            {
                _isMouseOver = value;
                Invalidate();
            }
        }

        protected internal virtual void OnMouseEnter(EventArgs args) { }
        protected internal virtual void OnMouseExit(EventArgs args) { }

        protected void Invalidate()
        {
            _needsRender = true;
        }

        public void Render(GraphicsDevice graphicsDevice)
        {
            if (!_needsRender)
            {
                return;
            }

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var renderPassDescriptor = new RenderPassDescriptor();

            var clearColour = _backgroundColor.GetValueOrDefault(new ColorRgbaF(0, 0, 0, 0));

            renderPassDescriptor.SetRenderTargetDescriptor(
                _renderTarget,
                LoadAction.Clear,
                clearColour);

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            var viewport = new Rectangle(0, 0, Frame.Width, Frame.Height);
            _drawingContext.Begin(commandEncoder, viewport, SamplerStateDescription.LinearClamp);

            commandEncoder.SetViewport(new Viewport(0, 0, Frame.Width, Frame.Height));

            if (_imageTexture != null)
            {
                _drawingContext.DrawImage(_imageTexture, viewport);
            }

            OnRender(_drawingContext);

            _drawingContext.End();

            commandEncoder.Close();

            commandBuffer.Commit();

            _needsRender = false;
        }

        protected abstract void OnRender(DrawingContext drawingContext);

        private StretchableImage LoadImage(WndWindow wndWindow, ContentManager contentManager)
        {
            switch (wndWindow.WindowType)
            {
                case WndWindowType.GenericWindow:
                    {
                        var image = LoadImage(wndWindow, 0, contentManager);
                        return image != null 
                            ? StretchableImage.CreateNormal(wndWindow.ScreenRect.ToRectangle().Width, image)
                            : null;
                    }

                case WndWindowType.PushButton:
                    {
                        var imageLeft = LoadImage(wndWindow, 0, contentManager);
                        var imageMiddle = LoadImage(wndWindow, 5, contentManager);
                        var imageRight = LoadImage(wndWindow, 6, contentManager);

                        if (imageLeft != null && imageMiddle != null && imageRight != null)
                            return StretchableImage.CreateStretchable(wndWindow.ScreenRect.ToRectangle().Width, imageLeft, imageMiddle, imageRight);

                        if (imageLeft != null)
                            return StretchableImage.CreateNormal(wndWindow.ScreenRect.ToRectangle().Width, imageLeft);

                        return null;
                    }

                default:
                    // TODO
                    return null;
            }
        }

        private CroppedBitmap LoadImage(WndWindow wndWindow, int drawDataIndex, ContentManager contentManager)
        {
            var image = wndWindow.EnabledDrawData.Items[drawDataIndex].Image;

            if (string.IsNullOrEmpty(image) || image == "NoImage")
            {
                return null;
            }

            var mappedImage = contentManager.IniDataContext.MappedImages.FirstOrDefault(x => x.Name == image);
            if (mappedImage == null)
            {
                return null;
            }

            var texture = contentManager.Load<Texture>(
                new[]
                {
                    Path.Combine(@"Data\English\Art\Textures", mappedImage.Texture),
                    Path.Combine(@"Art\Textures", mappedImage.Texture)
                },
                new TextureLoadOptions { GenerateMipMaps = false });

            var textureRect = mappedImage.Coords.ToRectangle();

            return new CroppedBitmap(texture, textureRect);
        }

        /*
         * private UIElement CreateWindowElement(WndWindow window, FrameworkElement contentElement)
        {
            var result = new Border
            {
                Name = window.Name.Replace(".", string.Empty).Replace(":", string.Empty),
                DataContext = window
            };

            var style = new Style(typeof(Border));

            var hasBackground = !window.Status.HasFlag(WndWindowStatusFlags.Image);

            style.Setters.Add(new Setter(TextBlock.ForegroundProperty, new Binding("TextColor.Enabled") { Converter = ColorRgbaToBrushConverter.Instance }));
            if (hasBackground)
            {
                style.Setters.Add(new Setter(Border.BackgroundProperty, new Binding("EnabledDrawData.Items[0].Color") { Converter = ColorRgbaToBrushConverter.Instance }));
                style.Setters.Add(new Setter(Border.BorderBrushProperty, new Binding("EnabledDrawData.Items[0].BorderColor") { Converter = ColorRgbaToBrushConverter.Instance }));
            }
            else
                result.Background = Brushes.Transparent;

            var trigger = new Trigger
            {
                Property = UIElement.IsMouseOverProperty,
                Value = true,
            };

            trigger.Setters.Add(new Setter(TextBlock.ForegroundProperty, new Binding("TextColor.Hilite") { Converter = ColorRgbaToBrushConverter.Instance }));
            if (hasBackground)
            {
                trigger.Setters.Add(new Setter(Border.BackgroundProperty, new Binding("HiliteDrawData.Items[0].Color") { Converter = ColorRgbaToBrushConverter.Instance }));
                trigger.Setters.Add(new Setter(Border.BorderBrushProperty, new Binding("HiliteDrawData.Items[0].BorderColor") { Converter = ColorRgbaToBrushConverter.Instance }));
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
         * */
    }
}
