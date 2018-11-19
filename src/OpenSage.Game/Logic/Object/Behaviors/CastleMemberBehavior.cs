using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class CastleMemberBehaviorModuleData : BehaviorModuleData
    {
        internal static CastleMemberBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<CastleMemberBehaviorModuleData> FieldParseTable = new IniParseTable<CastleMemberBehaviorModuleData>
        {
            { "CountsForEvaCastleBreached", (parser, x) => x.CountsForEvaCastleBreached = parser.ParseBoolean() },
        };

        public bool CountsForEvaCastleBreached { get; internal set; }
      
    }
}
