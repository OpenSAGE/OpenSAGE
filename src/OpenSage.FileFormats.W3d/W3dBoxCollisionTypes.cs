using System;

namespace OpenSage.Data.W3d
{
    [Flags]
    public enum W3dBoxCollisionTypes
    {
        None = 0,

        Physical   = 0x10,
        Projectile = 0x20,
        Vis        = 0x40,
        Camera     = 0x80,
        Vehicle    = 0x100
    }
}
