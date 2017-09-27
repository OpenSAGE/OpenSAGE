namespace OpenSage.Mathematics
{
    /// <summary>
    /// Describes a floating-point rectangle.
    /// </summary>
    public struct RectangleF
    {
        /// <summary>
        /// Gets or sets the x component of the <see cref="RectangleF"/>.
        /// </summary>
        public float X;

        /// <summary>
        /// Gets or sets the x component of the <see cref="RectangleF"/>.
        /// </summary>
        public float Y;

        /// <summary>
        /// Gets or sets the width of the <see cref="RectangleF"/>.
        /// </summary>
        public float Width;

        /// <summary>
        /// Gets or sets the height of the <see cref="RectangleF"/>.
        /// </summary>
        public float Height;

        /// <summary>
        /// Creates a new <see cref="RectangleF"/>.
        /// </summary>
        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{{X:{X} Y:{Y} Width:{Width} Height:{Height}}}";
        }
    }
}
