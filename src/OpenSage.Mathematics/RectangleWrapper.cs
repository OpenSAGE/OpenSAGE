using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.Mathematics
{
    public class RectangleWrapper
    {
        private int x;
        private int y;
        private int width;
        private int height;

        public int X
        {
            get
            {
                return x;
            }

            set
            {
                x = value;
                Rectangle = new Rectangle(X, Y, Width, Height);
            }
        }

        public int Y
        {
            get
            {
                return y;
            }

            set
            {
                y = value;
                Rectangle = new Rectangle(X, Y, Width, Height);
            }
        }

        public int Width
        {
            get
            {
                return width;
            }

            set
            {
                width = value;
                Rectangle = new Rectangle(X, Y, Width, Height);
            }
        }

        public int Height
        {
            get
            {
                return height;
            }

            set
            {
                height = value;
                Rectangle = new Rectangle(X, Y, Width, Height);
            }
        }

        public Rectangle Rectangle {get; private set;}

        public RectangleWrapper()
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
            Rectangle = new Rectangle(X, Y, Width, Height);
        }

        public RectangleWrapper(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Rectangle = new Rectangle(X, Y, Width, Height);
        }

        public RectangleWrapper(Rectangle rectangle)
        {
            X = rectangle.X;
            Y = rectangle.Y;
            Width = rectangle.Width;
            Height = rectangle.Height;
            Rectangle = rectangle;
        }
    }
}
