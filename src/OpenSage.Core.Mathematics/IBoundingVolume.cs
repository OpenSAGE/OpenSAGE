namespace OpenSage.Mathematics
{
    public interface IBoundingVolume
    {
        bool Intersects(RectangleF bounds);
    }
}
