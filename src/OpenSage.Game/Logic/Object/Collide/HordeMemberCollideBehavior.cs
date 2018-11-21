using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class HordeMemberCollideModuleData : BehaviorModuleData
    {
        internal static HordeMemberCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<HordeMemberCollideModuleData> FieldParseTable = new IniParseTable<HordeMemberCollideModuleData>();
    }
}
