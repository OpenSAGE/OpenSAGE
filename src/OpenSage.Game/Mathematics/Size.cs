using System;

namespace OpenSage.Mathematics
{
    /// <summary>
    /// Describes an integer size.
    /// </summary>
    public readonly struct Size : IEquatable<Size>
    {
        public static readonly Size Zero = new Size(0, 0);

        /// <summary>
        /// Gets the width.
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// Gets the height.
        /// </summary>
        public readonly int Height;

        /// <summary>
        /// Initializes a new instance of <see cref="Size"/>.
        /// </summary>
        /// <param name="width">Value for the width component.</param>
        /// <param name="height">Value for the height component.</param>
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Size"/>.
        /// </summary>
        /// <param name="value">Value for the width and height components.</param>
        public Size(int value)
        {
            Width = Height = value;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        public bool Equals(Size other)
        {
            return Width == other.Width && Height == other.Height;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Size && Equals((Size) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Width * 397) ^ Height;
            }
        }

        /// <summary>
        /// Tests values for equality.
        /// </summary>
        /// <param name="left">First value.</param>
        /// <param name="right">Second value.</param>
        /// <returns><code>true</code> if the values are equal, otherwise <code>false</code>.</returns>
        public static bool operator ==(Size left, Size right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests values for inequality.
        /// </summary>
        /// <param name="left">First value.</param>
        /// <param name="right">Second value.</param>
        /// <returns><code>true</code> if the values are not equal, otherwise <code>false</code>.</returns>
        public static bool operator !=(Size left, Size right)
        {
            return !left.Equals(right);
        }

        public SizeF ToSizeF() => new SizeF(Width, Height);
    }
}
