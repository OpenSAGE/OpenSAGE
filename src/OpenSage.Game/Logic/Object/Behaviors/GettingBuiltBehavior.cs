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
        };

        public string WorkerName { get; private set; }
    }
}
