using System;

namespace OpenSage.LowLevel.Graphics3D
{
    [Flags]
    public enum TextureBindFlags
    {
        None = 0,

        ShaderResource = 0x1,

        RenderTarget = 0x2
    }
}
