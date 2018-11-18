using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class RunOffMapBehaviorModuleData : BehaviorModuleData
    {
        internal static RunOffMapBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RunOffMapBehaviorModuleData> FieldParseTable = new IniParseTable<RunOffMapBehaviorModuleData>
        {
            { "RequiresSpecificTrigger", (parser, x) => x.RequiresSpecificTrigger = parser.ParseBoolean() },
        };

        public bool RequiresSpecificTrigger { get; private set; }
    }
}
