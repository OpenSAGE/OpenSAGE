using System;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Gui.Wnd
{
    internal sealed class ImageButton
    {
        private readonly Texture _texture;
        private readonly Texture _textureHighlighted;
        private readonly Action _onClick;
        private readonly Action _invalidateParent;

        public Size TextureSize => new Size((int) _texture.Width, (int) _texture.Height);

        public RectangleF Frame { get; set; }

        public bool IsMouseOver { get; private set; }

        public ImageButton(
            Texture texture,
            Texture textureHighlighted,
            Action onClick,
            Action invalidateParent)
        {
            _texture = texture;
            _textureHighlighted = textureHighlighted;
            _onClick = onClick;
            _invalidateParent = invalidateParent;
        }

        public void HandleInput(WndWindowMessage message)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.MouseMove:
                    IsMouseOver = Frame.Contains(message.MousePosition);
                    _invalidateParent();
                    break;

                case WndWindowMessageType.MouseExit:
                    IsMouseOver = false;
                    break;

                case WndWindowMessageType.MouseUp:
                    if (IsMouseOver)
                    {
                        _onClick();
                    }
                    break;
            }
        }

        public void Draw(DrawingContext2D drawingContext)
        {
            drawingContext.DrawImage(
                IsMouseOver ? _textureHighlighted : _texture,
                null,
                Frame);
        }
    }
}
