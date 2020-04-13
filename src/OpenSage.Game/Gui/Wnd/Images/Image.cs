using System;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Gui.Wnd.Images
{
    public sealed class Image
    {
        private readonly ImageSource _source;
        private Texture _texture;
        private Size _size;

        public string Name { get; }

        public Size NaturalSize => _source.NaturalSize;

        internal Image(string name, ImageSource source)
        {
            Name = name;
            _source = source;
        }

        internal void SetSize(in Size size)
        {
            if (_size == size)
            {
                return;
            }

            _texture = _source.GetTexture(size);
            if(size.Width == 0 || size.Height == 0)
            {
                return;
            }

            if (_texture == null)
            {
                throw new InvalidOperationException();
            }

            _size = size;
        }

        internal void Draw(DrawingContext2D drawingContext, in Rectangle destinationRect)
        {
            drawingContext.DrawImage(_texture, null, destinationRect);
        }
    }
}
