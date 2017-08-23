using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows this object to move randomly about its point of origin using a SET_WANDER locomotor.
    /// </summary>
    public sealed class WanderAIUpdate : ObjectBehavior
    {
        internal static WanderAIUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<WanderAIUpdate> FieldParseTable = new IniParseTable<WanderAIUpdate>();
    }
}
