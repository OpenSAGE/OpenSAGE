using System;

namespace LL.Graphics3D
{
    [Flags]
    public enum TextureBindFlags
    {
        None = 0,

        ShaderResource = 0x1,

        RenderTarget = 0x2
    }
}
