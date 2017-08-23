using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the object to surround the parent object like a scaffold.
    /// </summary>
    public sealed class BridgeScaffoldBehavior : ObjectBehavior
    {
        internal static BridgeScaffoldBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BridgeScaffoldBehavior> FieldParseTable = new IniParseTable<BridgeScaffoldBehavior>();
    }
}
