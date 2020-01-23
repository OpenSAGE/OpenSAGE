using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// This module is required when KindOf contains TECH_BUILDING.
    /// </summary>
    public sealed class TechBuildingBehaviorModuleData : BehaviorModuleData
    {
        internal static TechBuildingBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TechBuildingBehaviorModuleData> FieldParseTable = new IniParseTable<TechBuildingBehaviorModuleData>();
    }
}
