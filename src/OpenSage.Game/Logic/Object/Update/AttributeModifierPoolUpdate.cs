using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AttributeModifierPoolUpdateModuleData : UpdateModuleData
    {
        internal static AttributeModifierPoolUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AttributeModifierPoolUpdateModuleData> FieldParseTable = new IniParseTable<AttributeModifierPoolUpdateModuleData>();
    }
}
