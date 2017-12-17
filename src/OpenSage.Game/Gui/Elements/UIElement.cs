using System;
using OpenSage.Content;
using OpenSage.Data.Wnd;
using OpenSage.LowLevel;
using OpenSage.LowLevel.Graphics2D;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Elements
{
    public abstract class UIElement : DisposableBase
    {
        private bool _initialized;

        private WndWindow _wndWindow;

        private Texture _texture;
        private RenderTarget _renderTarget;

        private UIElementState _enabledState;
        private UIElementState _highlightState;

        private float _scale;

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

            _text = contentManager.TranslationManager.Lookup(wndWindow.Text);

            _enabledState = AddDisposable(new UIElementState(wndWindow, wndWindow.TextColor.Enabled, wndWindow.TextColor.EnabledBorder, wndWindow.EnabledDrawData, contentManager, contentManager.GraphicsDevice, HostPlatform.GraphicsDevice2D));

            if (wndWindow.WindowType == WndWindowType.PushButton)
            {
                _highlightState = AddDisposable(new UIElementState(wndWindow, wndWindow.TextColor.Hilite, wndWindow.TextColor.HiliteBorder, wndWindow.HiliteDrawData, contentManager, contentManager.GraphicsDevice, HostPlatform.GraphicsDevice2D));
            }

            Hidden = wndWindow.Status.HasFlag(WndWindowStatusFlags.Hidden) || wndWindow.InputCallback == "GameWinBlockInput";

            // TODO

            _initialized = true;
        }

        public void CreateSizeDependentResources(ContentManager contentManager, in Size windowSize)
        {
            Frame = CalculateFrame(_wndWindow.ScreenRect, windowSize, out _scale);

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

        private static Rectangle CalculateFrame(in WndScreenRect wndScreenRect, in Size viewportSize, out float scale)
        {
            // Figure out the ratio.
            var ratioX = viewportSize.Width / (float) wndScreenRect.CreationResolution.Width;
            var ratioY = viewportSize.Height / (float) wndScreenRect.CreationResolution.Height;

            // Use whichever multiplier is smaller.
            var ratio = ratioX < ratioY ? ratioX : ratioY;

            scale = ratio;

            var originalWidth = wndScreenRect.BottomRight.X - wndScreenRect.UpperLeft.X;
            var originalHeight = wndScreenRect.BottomRight.Y - wndScreenRect.UpperLeft.Y;

            // Now we can get the new height and width
            var newWidth = (int) Math.Round(originalWidth * ratio);
            var newHeight = (int) Math.Round(originalHeight * ratio);

            newWidth = Math.Max(newWidth, 1);
            newHeight = Math.Max(newHeight, 1);

            var newX = (int) Math.Round(wndScreenRect.UpperLeft.X * ratio);
            var newY = (int) Math.Round(wndScreenRect.UpperLeft.Y * ratio);

            // Now calculate the X,Y position of the upper-left corner 
            // (one of these will always be zero for the top level window)
            var posX = (int) Math.Round((viewportSize.Width - (wndScreenRect.CreationResolution.Width * ratio)) / 2) + newX;
            var posY = (int) Math.Round((viewportSize.Height - (wndScreenRect.CreationResolution.Height * ratio)) / 2) + newY;

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
            
            var activeState = _isMouseOver
                ? _highlightState ?? _enabledState
                : _enabledState;

            var clearColour = activeState.BackgroundColor.GetValueOrDefault(new ColorRgbaF(0, 0, 0, 0));

            using (var drawingContext = new DrawingContext(HostPlatform.GraphicsDevice2D, _texture))
            {
                drawingContext.Begin();

                drawingContext.Clear(clearColour);

                if (activeState.ImageTexture != null)
                {
                    drawingContext.DrawImage(
                        activeState.ImageTexture,
                        new RawRectangleF(0, 0, activeState.ImageTexture.Width, activeState.ImageTexture.Height),
                        new RawRectangleF(0, 0, Frame.Width, Frame.Height),
                        true);
                }

                if (activeState.BorderColor != null || _highlighted)
                {
                    var borderColor = _highlighted
                        ? new ColorRgbaF(1f, 0.41f, 0.71f, 1)
                        : activeState.BorderColor.Value;

                    var borderWidth = _highlighted ? 8 : 2;

                    drawingContext.DrawRectangle(
                        new RawRectangleF(0, 0, Frame.Width, Frame.Height),
                        borderColor,
                        borderWidth);
                }

                if (!string.IsNullOrEmpty(Text))
                {
                    using (var textFormat = new TextFormat(HostPlatform.GraphicsDevice2D, _wndWindow.Font.Name, _wndWindow.Font.Size * _scale, _wndWindow.Font.Bold ? FontWeight.Bold : FontWeight.Normal))
                    {
                        var rect = new RawRectangleF { X = 0, Y = 0, Width = Frame.Width, Height = Frame.Height };
                        drawingContext.DrawText(Text, textFormat, activeState.TextColor.ToColorRgba(), rect);
                    }
                }

                OnRender(drawingContext);

                drawingContext.End();
            }

            _needsRender = false;
        }

        protected abstract void OnRender(DrawingContext drawingContext);

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
