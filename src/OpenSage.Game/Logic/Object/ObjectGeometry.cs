using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum ObjectGeometry
    {
        None,

        [IniEnum("BOX")]
        Box,

        [IniEnum("SPHERE")]
        Sphere,

        [IniEnum("CYLINDER")]
        Cylinder
    }
}
