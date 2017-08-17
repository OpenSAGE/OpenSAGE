using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Used on TRANSPORT KindOfs that follow a specific pre-defined waypoint path in a scripted 
    /// manner.
    /// </summary>
    public sealed class RailedTransportAIUpdate : ObjectBehavior
    {
        internal static RailedTransportAIUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RailedTransportAIUpdate> FieldParseTable = new IniParseTable<RailedTransportAIUpdate>
        {
            { "PathPrefixName", (parser, x) => x.PathPrefixName = parser.ParseAssetReference() }
        };

        /// <summary>
        /// Waypoint prefix name.
        /// </summary>
        public string PathPrefixName { get; private set; }
    }
}
