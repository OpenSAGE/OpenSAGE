using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows use of a radius decal cursor from Mouse.INI on the object's weapon when not 
    /// explicitly fired.
    /// </summary>
    public sealed class RadiusDecalUpdate : ObjectBehavior
    {
        internal static RadiusDecalUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RadiusDecalUpdate> FieldParseTable = new IniParseTable<RadiusDecalUpdate>();
    }
}
