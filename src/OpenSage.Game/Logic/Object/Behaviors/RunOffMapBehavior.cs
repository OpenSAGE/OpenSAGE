using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class RunOffMapBehaviorModuleData : BehaviorModuleData
    {
        internal static RunOffMapBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RunOffMapBehaviorModuleData> FieldParseTable = new IniParseTable<RunOffMapBehaviorModuleData>
        {
            { "RequiresSpecificTrigger", (parser, x) => x.RequiresSpecificTrigger = parser.ParseBoolean() },
            { "RunOffMapWaypointName", (parser, x) => x.RunOffMapWaypointName = parser.ParseIdentifier() },
            { "DieOnMap", (parser, x) => x.DieOnMap = parser.ParseBoolean() }
        };

        public bool RequiresSpecificTrigger { get; private set; }

        public string RunOffMapWaypointName { get; private set; }
        public bool DieOnMap { get; private set; }
    }
}
