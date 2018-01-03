namespace OpenSage.LowLevel.Graphics2D
{
    public struct RawTriangleF
    {
        public float X1;
        public float Y1;
        public float X2;
        public float Y2;
        public float X3;
        public float Y3;

        public RawTriangleF(float x1, float y1, float x2, float y2,
                            float x3, float y3)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            X3 = x3;
            Y3 = y3;
        }
    }
}
