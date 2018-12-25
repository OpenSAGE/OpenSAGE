using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class PreorderCreateModuleData : CreateModuleData
    {
        internal static PreorderCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PreorderCreateModuleData> FieldParseTable = new IniParseTable<PreorderCreateModuleData>();
    }
}
