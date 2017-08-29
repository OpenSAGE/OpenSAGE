using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Special-case draw module used by ObjectCreationList.INI when using the CreateDebris code 
    /// which defaults to calling the GenericDebris object definition as a template for each debris 
    /// object generated.
    /// </summary>
    public sealed class W3dDebrisDrawModuleData : DrawModuleData
    {
        internal static W3dDebrisDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<W3dDebrisDrawModuleData> FieldParseTable = new IniParseTable<W3dDebrisDrawModuleData>();
    }
}
