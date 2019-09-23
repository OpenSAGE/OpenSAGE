using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Used on TRANSPORT KindOfs that follow a specific pre-defined waypoint path in a scripted 
    /// manner.
    /// </summary>
    public sealed class RailedTransportAIUpdateModuleData : AIUpdateModuleData
    {
        internal static new RailedTransportAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<RailedTransportAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<RailedTransportAIUpdateModuleData>
            {
                { "PathPrefixName", (parser, x) => x.PathPrefixName = parser.ParseAssetReference() }
            });

        /// <summary>
        /// Waypoint prefix name.
        /// </summary>
        public string PathPrefixName { get; private set; }
    }
}
