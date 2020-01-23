using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Transfers damage done to itself to its parent bridge too.
    /// </summary>
    public sealed class BridgeTowerBehaviorModuleData : BehaviorModuleData
    {
        internal static BridgeTowerBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BridgeTowerBehaviorModuleData> FieldParseTable = new IniParseTable<BridgeTowerBehaviorModuleData>();
    }
}
