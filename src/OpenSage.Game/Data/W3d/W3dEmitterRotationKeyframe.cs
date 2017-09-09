using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterRotationKeyframe
    {
        public float Time { get; private set; }

        /// <summary>
        /// Rotational velocity in rotations/sec
        /// </summary>
        public float Rotation { get; private set; }

        internal static W3dEmitterRotationKeyframe Parse(BinaryReader reader)
        {
            return new W3dEmitterRotationKeyframe
            {
                Time = reader.ReadSingle(),
                Rotation = reader.ReadSingle()
            };
        }
    }
}
