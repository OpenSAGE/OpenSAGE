namespace OpenSage.Mathematics;

public readonly record struct Point3D(int X, int Y, int Z)
{
    public Point2D ToPoint2D()
    {
        return new Point2D(X, Y);
    }
}
