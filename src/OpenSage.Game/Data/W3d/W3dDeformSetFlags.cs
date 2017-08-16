using System;

namespace OpenSage.Data.W3d
{
    [Flags]
    public enum W3dDeformSetFlags : uint
    {
        /// <summary>
        /// set is isn't applied during sphere or point tests.
        /// </summary>
        ManualDeform = 0x00000001
    }
}
