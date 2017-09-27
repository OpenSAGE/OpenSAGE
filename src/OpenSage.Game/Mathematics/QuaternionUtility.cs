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

        public static Quaternion FromEuler(Vector3 euler)
        {
            return FromEuler(euler.X, euler.Y, euler.Z);
        }

        public static Quaternion FromEuler(float x, float y, float z)
        {
            // https://code.google.com/p/3d-editor-toolkit/source/browse/trunk/PureCpp/MathCore/Quaternion.cpp

            var c1 = MathUtility.Cos(y / 2.0f);
            var s1 = MathUtility.Sin(y / 2.0f);
            var c2 = MathUtility.Cos(z / 2.0f);
            var s2 = MathUtility.Sin(z / 2.0f);
            var c3 = MathUtility.Cos(x / 2.0f);
            var s3 = MathUtility.Sin(x / 2.0f);
            var c1c2 = c1 * c2;
            var s1s2 = s1 * s2;

            return new Quaternion(
                c1c2 * s3 + s1s2 * c3,
                s1 * c2 * c3 + c1 * s2 * s3,
                c1 * s2 * c3 - s1 * c2 * s3,
                c1c2 * c3 - s1s2 * s3);
        }

        public static Vector3 ToEuler(Quaternion q)
        {
            // https://code.google.com/p/3d-editor-toolkit/source/browse/trunk/PureCpp/MathCore/Quaternion.cpp

            var w = q.W;
            var x = q.X;
            var y = q.Y;
            var z = q.Z;

            var sqw = w * w;
            var sqx = x * x;
            var sqy = y * y;
            var sqz = z * z;
            var unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            var test = x * y + z * w;

            var v = Vector3.Zero;
            if (test > 0.499f * unit) // singularity at north pole
            {
                v.Y = 2.0f * MathUtility.Atan2(x, w);
                v.Z = MathUtility.PiOver2;
                v.X = 0.0f;
            }
            else if (test < -0.499f * unit) // singularity at south pole
            {
                v.Y = -2.0f * MathUtility.Atan2(x, w);
                v.Z = -MathUtility.PiOver2;
                v.X = 0;
            }
            else
            {
                v.Y = MathUtility.Atan2(2.0f * y * w - 2.0f * x * z, sqx - sqy - sqz + sqw);
                v.Z = MathUtility.Asin(2.0f * test / unit);
                v.X = MathUtility.Atan2(2.0f * x * w - 2.0f * y * z, -sqx + sqy - sqz + sqw);
            }
            return v;
        }

        public static Quaternion CreateRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            // http://stackoverflow.com/questions/1171849/finding-quaternion-representing-the-rotation-from-one-vector-to-another
            var dotProduct = Vector3.Dot(fromDirection, toDirection);
            var scalarPart = MathUtility.Sqrt(fromDirection.LengthSquared() * toDirection.LengthSquared()) + dotProduct;
            return Quaternion.Normalize(new Quaternion(Vector3.Cross(fromDirection, toDirection), scalarPart));
        }

        public static Quaternion CreateLookRotation(Vector3 forward, Vector3 up)
        {
            if (forward == up)
                up = Vector3.UnitY;
            var matrix = Matrix4x4.CreateLookAt(Vector3.Zero, forward, up);
            return Quaternion.CreateFromRotationMatrix(Matrix4x4Utility.Invert(matrix));
        }

        public static Quaternion CreateLookRotation(Vector3 forward)
        {
            return CreateLookRotation(forward, Vector3.UnitZ);
        }
    }
}
