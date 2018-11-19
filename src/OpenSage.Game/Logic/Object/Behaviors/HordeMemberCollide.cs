using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class HordeMemberCollideBehaviorData : BehaviorModuleData
    {
        internal static HordeMemberCollideBehaviorData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<HordeMemberCollideBehaviorData> FieldParseTable = new IniParseTable<HordeMemberCollideBehaviorData>();
    }
}
