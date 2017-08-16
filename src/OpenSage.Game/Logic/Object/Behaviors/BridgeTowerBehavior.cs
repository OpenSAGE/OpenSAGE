using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Transfers damage done to itself to its parent bridge too.
    /// </summary>
    public sealed class BridgeTowerBehavior : ObjectBehavior
    {
        internal static BridgeTowerBehavior Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<BridgeTowerBehavior> FieldParseTable = new IniParseTable<BridgeTowerBehavior>();
    }
}
