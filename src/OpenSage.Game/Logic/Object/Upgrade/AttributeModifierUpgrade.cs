using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class AttributeModifierUpgradeModuleData : UpgradeModuleData
    {
        internal static AttributeModifierUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<AttributeModifierUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<AttributeModifierUpgradeModuleData>
            {
                { "AttributeModifier", (parser, x) => x.AttributeModifier = parser.ParseAssetReference() }
            });

        public string AttributeModifier { get; private set; }
    }
}
