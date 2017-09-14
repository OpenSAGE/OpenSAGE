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
    }
}
