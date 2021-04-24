using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum ObjectGeometry
    {
        [IniEnum("SPHERE")]
        Sphere = 0,

        [IniEnum("CYLINDER")]
        Cylinder = 1,

        [IniEnum("BOX")]
        Box = 2,
    }
}
