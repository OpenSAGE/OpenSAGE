using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class QuaternionUtility
    {
        public static Quaternion CreateFromYawPitchRoll_ZUp(float yaw, float pitch, float roll)
        {
            var q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
            var zTemp = q.Z;
            q.Z = q.Y;
            q.Y = -zTemp;
            return q;
        }

        public static Quaternion CreateRotation(in Vector3 fromDirection, in Vector3 toDirection)
        {
            // http://stackoverflow.com/questions/1171849/finding-quaternion-representing-the-rotation-from-one-vector-to-another
            var dotProduct = Vector3.Dot(fromDirection, toDirection);
            var scalarPart = MathF.Sqrt(fromDirection.LengthSquared() * toDirection.LengthSquared()) + dotProduct;
            return Quaternion.Normalize(new Quaternion(Vector3.Cross(fromDirection, toDirection), scalarPart));
        }

        public static Quaternion CreateLookRotation(in Vector3 forward, Vector3 up)
        {
            if (forward == up)
                up = Vector3.UnitY;
            var matrix = Matrix4x4.CreateLookAt(Vector3.Zero, forward, up);
            return Quaternion.CreateFromRotationMatrix(Matrix4x4Utility.Invert(matrix));
        }

        public static Quaternion CreateLookRotation(in Vector3 forward)
        {
            return CreateLookRotation(forward, Vector3.UnitZ);
        }
    }
}
