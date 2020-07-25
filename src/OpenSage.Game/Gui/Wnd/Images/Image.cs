using System;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd.Images
{
    public sealed class Image
    {
        private readonly ImageSource _source;
        private readonly bool _grayscale;
        private Texture _texture;
        private Size _size;

        public string Name { get; }

        public Size NaturalSize => _source.NaturalSize;

        internal Image(string name, ImageSource source, bool grayscale = false)
        {
            Name = name;
            _source = source;
            _grayscale = grayscale;
        }

        internal void SetSize(in Size size)
        {
            var actualSize = _source.NaturalSize.Width > size.Width
                ? _source.NaturalSize
                : size;

            if (_size == actualSize)
            {
                return;
            }

            _texture = _source.GetTexture(actualSize);
            if(size.Width == 0 || size.Height == 0)
            {
                return;
            }

            if (_texture == null)
            {
                throw new InvalidOperationException();
            }

            _size = actualSize;
        }

        public Image WithGrayscale(bool grayscale)
        {
            return new Image(Name, _source, grayscale);
        }

        internal void Draw(DrawingContext2D drawingContext, in Rectangle destinationRect)
        {
            if (_texture == null)
            {
                // TODO: crashes in multiplayer mode
                return;
                // throw new InvalidOperationException();
            }
            drawingContext.DrawImage(_texture, null, destinationRect, grayscale: _grayscale);
        }
    }
}
