using System;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd.Images
{
    public sealed class Image
    {
        private readonly Func<Size, Texture> _createTexture;
        private readonly bool _flipped;
        private Texture _texture;
        private Size _size;

        public string Name { get; }

        public Size NaturalSize { get; }

        internal Image(string name, in Size naturalSize, Func<Size, Texture> createTexture, in bool flipped = false)
        {
            Name = name;
            NaturalSize = naturalSize;
            _createTexture = createTexture;
            _flipped = flipped;
        }

        internal void SetSize(in Size size)
        {
            if (_size == size)
            {
                return;
            }

            if (_texture != null)
            {
                _texture.Dispose();
                _texture = null;
            }

            _texture = _createTexture(size);

            if (_texture == null)
            {
                throw new InvalidOperationException();
            }

            _size = size;
        }

        internal void Draw(DrawingContext2D drawingContext, in Rectangle destinationRect)
        {
            drawingContext.DrawImage(_texture, null, destinationRect, _flipped);
        }
    }
}
