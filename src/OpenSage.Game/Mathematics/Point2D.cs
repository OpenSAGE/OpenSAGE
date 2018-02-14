namespace OpenSage.Mathematics
{
    public struct Point2D
    {
        public int X;
        public int Y;

        public Point2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"<{X}, {Y}>";
        }
    }
}
