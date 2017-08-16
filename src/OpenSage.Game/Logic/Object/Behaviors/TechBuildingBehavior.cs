using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// This module is required when KindOf contains TECH_BUILDING.
    /// </summary>
    public sealed class TechBuildingBehavior : ObjectBehavior
    {
        internal static TechBuildingBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TechBuildingBehavior> FieldParseTable = new IniParseTable<TechBuildingBehavior>();
    }
}
