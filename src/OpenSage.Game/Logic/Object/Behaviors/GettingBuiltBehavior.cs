using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class GettingBuiltBehaviorModuleData : BehaviorModuleData
    {
        internal static GettingBuiltBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GettingBuiltBehaviorModuleData> FieldParseTable = new IniParseTable<GettingBuiltBehaviorModuleData>
        {
            { "WorkerName", (parser, x) => x.WorkerName = parser.ParseString() },
            { "SelfBuildingLoop", (parser, x) => x.SelfBuildingLoop = parser.ParseString() },
            { "SelfRepairFromDamageLoop", (parser, x) => x.SelfRepairFromDamageLoop = parser.ParseString() },
            { "SelfRepairFromRubbleLoop", (parser, x) => x.SelfRepairFromRubbleLoop = parser.ParseString() },
        };

        public string WorkerName { get; private set; }
        public string SelfBuildingLoop { get; private set; }
        public string SelfRepairFromDamageLoop { get; private set; }
        public string SelfRepairFromRubbleLoop { get; private set; }
    }
}
