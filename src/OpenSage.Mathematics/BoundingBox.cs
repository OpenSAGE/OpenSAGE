using System.Numerics;

namespace OpenSage.Mathematics
{
    public readonly struct BoundingBox : IBoundingVolume
    {
        public readonly Vector3 Min;
        public readonly Vector3 Max;

        public BoundingBox(in Vector3 min, in Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public Vector3 GetCenter() => (Min + Max) / 2;

      
        public bool Intersects(RectangleF bounds)
        {
            //var position = Min.Vector2XY();
            //var maxPosition = Max.Vector2XY();
            //var rect = new TransformedRectangle(position, )
            return false;
        }

        public static BoundingBox Transform(in BoundingBox box, in Matrix4x4 matrix)
        {
            var min = Vector3.Transform(box.Min, matrix);
            var max = Vector3.Transform(box.Max, matrix);
            return new BoundingBox(min, max);
        }

        public override string ToString()
        {
            return $"{nameof(Min)}: {Min}, {nameof(Max)}: {Max}";
        }
    }
}
