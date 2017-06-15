using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.W3d
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
        public uint ParentIdx { get; private set; }

        /// <summary>
        /// translation to pivot point
        /// </summary>
        public W3dVector Translation { get; private set; }

        /// <summary>
        /// orientation of the pivot point
        /// </summary>
        public W3dVector EulerAngles { get; private set; }

        /// <summary>
        /// orientation of the pivot point
        /// </summary>
        public W3dQuaternion Rotation { get; private set; }

        public static W3dPivot Parse(BinaryReader reader)
        {
            return new W3dPivot
            {
                Name = reader.ReadFixedLengthString(W3dConstants.NameLength),
                ParentIdx = reader.ReadUInt32(),
                Translation = W3dVector.Parse(reader),
                EulerAngles = W3dVector.Parse(reader),
                Rotation = W3dQuaternion.Parse(reader)
            };
        }
    }
}
