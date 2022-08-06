using System.IO;
using System.Numerics;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dPivot
    {
        /// <summary>
        /// Name of the node (UR_ARM, LR_LEG, TORSO, etc)
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 0xffffffff = root pivot; no parent
        /// </summary>
        public int ParentIdx { get; private set; }

        /// <summary>
        /// translation to pivot point
        /// </summary>
        public Vector3 Translation { get; private set; }

        /// <summary>
        /// orientation of the pivot point
        /// </summary>
        public Vector3 EulerAngles { get; private set; }

        /// <summary>
        /// orientation of the pivot point
        /// </summary>
        public Quaternion Rotation { get; private set; }

        internal static W3dPivot Parse(BinaryReader reader)
        {
            return new W3dPivot
            {
                Name = reader.ReadFixedLengthString(W3dConstants.NameLength),
                ParentIdx = reader.ReadInt32(),
                Translation = reader.ReadVector3(),
                EulerAngles = reader.ReadVector3(),
                Rotation = reader.ReadQuaternion()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
            writer.Write(ParentIdx);
            writer.Write(Translation);
            writer.Write(EulerAngles);
            writer.Write(Rotation);
        }
    }
}
