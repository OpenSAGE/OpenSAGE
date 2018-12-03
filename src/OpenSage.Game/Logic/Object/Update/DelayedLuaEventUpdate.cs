using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class DelayedLuaEventUpdateModuleData : UpdateModuleData
    {
        internal static DelayedLuaEventUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DelayedLuaEventUpdateModuleData> FieldParseTable = new IniParseTable<DelayedLuaEventUpdateModuleData>();

    }
}
