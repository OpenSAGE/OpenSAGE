using System.Linq;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd.Controls
{
    public class Control : DisposableBase
    {
        public ControlCallback SystemCallback { get; set; }
        public ControlCallback InputCallback { get; set; }
        public ControlCallback TooltipCallback { get; set; }
        public ControlDrawCallback DrawCallback { get; set; }

        public Control Parent
        {
            get => ParentInternal;
            set
            {
                if (ParentInternal != null)
                {
                    ParentInternal.Controls.Remove(this);
                }

                ParentInternal = value;

                ParentInternal.Controls.Add(this);
            }
        }

        internal Control ParentInternal;

        public Window Window { get; internal set; }

        public ControlCollection Controls { get; }

        public string Name { get; set; }

        public string DisplayName => $"{Name} ({GetType().Name}, {(Visible ? "Visible" : "Hidden")}, Opacity {Opacity})";

        public bool Visible { get; set; } = true;

        public void Show() => Visible = true;

        public void Hide() => Visible = false;

        public bool Enabled { get; set; } = true;

        private Rectangle _bounds;
        public Rectangle Bounds
        {
            get => _bounds;
            set
            {
                _bounds = value;
                OnSizeChanged(_bounds.Size);
            }
        }

        public Size Size
        {
            get => _bounds.Size;
            set
            {
                _bounds = new Rectangle(_bounds.Location, value);
                OnSizeChanged(_bounds.Size);
            }
        }

        public Rectangle ClientRectangle => new Rectangle(0, 0, _bounds.Width, _bounds.Height);

        public int Left
        {
            get => _bounds.X;
            set => Bounds = new Rectangle(value, _bounds.Y, _bounds.Width, _bounds.Height);
        }

        public int Top
        {
            get => _bounds.Y;
            set => Bounds = new Rectangle(_bounds.X, value, _bounds.Width, _bounds.Height);
        }

        public int Right => _bounds.Right;

        public int Bottom => _bounds.Bottom;

        public int Width
        {
            get => _bounds.Width;
            set
            {
                _bounds = new Rectangle(_bounds.X, _bounds.Y, value, _bounds.Height);
                OnSizeChanged(_bounds.Size);
            }
        }

        public int Height
        {
            get => _bounds.Height;
            set
            {
                _bounds = new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, value);
                OnSizeChanged(_bounds.Size);
            }
        }

        protected virtual void OnSizeChanged(in Size newSize) { }

        public string Text { get; set; }

        public DrawingFont Font { get; set; }

        public ColorRgbaF BackgroundColor { get; set; } = ColorRgbaF.Transparent;
        public ColorRgbaF? HoverBackgroundColor { get; set; }
        public ColorRgbaF? DisabledBackgroundColor { get; set; }

        public ColorRgbaF TextColor { get; set; } = ColorRgbaF.Black;
        public ColorRgbaF? HoverTextColor { get; set; }
        public ColorRgbaF? DisabledTextColor { get; set; }

        public ColorRgbaF? TextShadowColor { get; set; }
        public ColorRgbaF? HoverTextShadowColor { get; set; }
        public ColorRgbaF? DisabledTextShadowColor { get; set; }

        public ImageBase BackgroundImage { get; set; }
        public ImageBase HoverBackgroundImage { get; set; }
        public ImageBase DisabledBackgroundImage { get; set; }

        public int BorderWidth { get; set; } = 0;

        public ColorRgbaF BorderColor { get; set; } = ColorRgbaF.Transparent;
        public ColorRgbaF? HoverBorderColor { get; set; }
        public ColorRgbaF? DisabledBorderColor { get; set; }

        // TODO: Move these into better Animation classes.
        public ColorRgbaF? BackgroundColorOverride { get; set; }
        public ColorRgbaF? OverlayColor { get; set; }

        public float Opacity { get; set; } = 1;

        public float TextOpacity { get; set; } = 1;

        public bool IsMouseOver { get; private set; }
        public bool IsMouseDown { get; private set; }

        public Control()
        {
            Controls = new ControlCollection(this);

            SystemCallback = (control, message, context) => { };
            InputCallback = DefaultInput;
            DrawCallback = DefaultDraw;
        }

        public virtual Point2D PointToClient(in Point2D point)
        {
            Point2D result;
            if (Parent != null)
            {
                result = Parent.PointToClient(point);
                result.X -= _bounds.X;
                result.Y -= _bounds.Y;
            }
            else
            {
                result = point;
            }

            return result;
        }

        public Control GetSelfOrDescendantAtPoint(in Point2D windowPoint)
        {
            if (!Enabled || !Visible)
            {
                return null;
            }

            var clientPoint = PointToClient(windowPoint);
            if (!ClientRectangle.Contains(clientPoint))
            {
                return null;
            }

            foreach (var child in Controls.Reverse())
            {
                var found = child.GetSelfOrDescendantAtPoint(windowPoint);
                if (found != null)
                {
                    return found;
                }
            }

            return (Opacity == 1)
                ? this
                : null;
        }

        public virtual Size GetPreferredSize(Size proposedSize) => Size.Zero;

        public void Layout()
        {
            LayoutOverride();
        }

        protected virtual void LayoutOverride() { }

        public void Invalidate()
        {

        }

        private void DefaultDraw(Control control, DrawingContext2D drawingContext)
        {
            drawingContext.PushOpacity(Opacity);

            DrawBackground(drawingContext);
            DrawBackgroundImage(drawingContext);
            DrawOverride(drawingContext);
            DrawBorder(drawingContext);
            DrawOverlay(drawingContext);

            drawingContext.PopOpacity();
        }

        protected virtual void DrawOverride(DrawingContext2D drawingContext) { }

        private void DrawBackground(DrawingContext2D drawingContext)
        {
            var color = BackgroundColor;

            if (BackgroundColorOverride != null)
            {
                color = BackgroundColorOverride.Value;
            }
            else if (!Enabled)
            {
                color = DisabledBackgroundColor ?? color;
            }
            else if (IsMouseOver)
            {
                color = HoverBackgroundColor ?? color;
            }

            drawingContext.FillRectangle(
                ClientRectangle,
                color);
        }

        protected virtual void DrawBackgroundImage(DrawingContext2D drawingContext)
        {
            var image = BackgroundImage;

            if (!Enabled)
            {
                image = DisabledBackgroundImage ?? image;
            }
            else if (IsMouseOver)
            {
                image = HoverBackgroundImage ?? image;
            }

            if (image != null)
            {
                image.Draw(drawingContext, ClientRectangle);
            }
        }

        private void DrawBorder(DrawingContext2D drawingContext)
        {
            if (BorderWidth == 0)
            {
                return;
            }

            var color = BorderColor;

            if (!Enabled)
            {
                color = DisabledBorderColor ?? color;
            }
            else if (IsMouseOver)
            {
                color = HoverBorderColor ?? color;
            }

            drawingContext.DrawRectangle(
                ClientRectangle.ToRectangleF(),
                color,
                BorderWidth);
        }

        private void DrawOverlay(DrawingContext2D drawingContext)
        {
            if (OverlayColor == null)
            {
                return;
            }

            var overlayColor = OverlayColor.Value;
            overlayColor.A *= Opacity;

            drawingContext.FillRectangle(
                ClientRectangle,
                overlayColor);
        }

        protected void DrawText(DrawingContext2D drawingContext, TextAlignment textAlignment)
        {
            var color = TextColor;

            if (!Enabled)
            {
                color = DisabledTextColor ?? color;
            }
            else if (IsMouseOver)
            {
                color = HoverTextColor ?? color;
            }

            color.A *= TextOpacity;

            drawingContext.DrawText(
                Text,
                Font,
                textAlignment,
                color,
                ClientRectangle);
        }

        private void DefaultInput(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.MouseEnter:
                    IsMouseOver = true;
                    break;

                case WndWindowMessageType.MouseExit:
                    IsMouseOver = false;
                    IsMouseDown = false;
                    break;

                case WndWindowMessageType.MouseDown:
                    IsMouseDown = true;
                    break;

                case WndWindowMessageType.MouseUp:
                    IsMouseDown = false;
                    break;
            }

            DefaultInputOverride(message, context);
        }

        protected virtual void DefaultInputOverride(WndWindowMessage message, ControlCallbackContext context) { }

        protected override void Dispose(bool disposeManagedResources)
        {
            foreach (var child in Controls)
            {
                child.ParentInternal = null;
                child.Dispose();
            }
            Controls.Clear();

            base.Dispose(disposeManagedResources);
        }
    }

    public delegate void ControlDrawCallback(Control control, DrawingContext2D drawingContext);

    public delegate void ControlCallback(Control control, WndWindowMessage message, ControlCallbackContext context);

    public sealed class ControlCallbackContext
    {
        public WndWindowManager WindowManager { get; }
        public Game Game { get; }

        public ControlCallbackContext(WndWindowManager windowManager, Game game)
        {
            WindowManager = windowManager;
            Game = game;
        }
    }
}
