using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the object to behave as a stream like water or other liquid ordinance.
    /// </summary>
    public sealed class ProjectileStreamUpdate : ObjectBehavior
    {
        internal static ProjectileStreamUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ProjectileStreamUpdate> FieldParseTable = new IniParseTable<ProjectileStreamUpdate>();
    }
}
