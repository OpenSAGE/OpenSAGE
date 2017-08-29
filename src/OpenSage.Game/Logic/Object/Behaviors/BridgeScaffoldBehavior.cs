using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the object to surround the parent object like a scaffold.
    /// </summary>
    public sealed class BridgeScaffoldBehaviorModuleData : BehaviorModuleData
    {
        internal static BridgeScaffoldBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BridgeScaffoldBehaviorModuleData> FieldParseTable = new IniParseTable<BridgeScaffoldBehaviorModuleData>();
    }
}
