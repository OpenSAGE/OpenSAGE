using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AutoAbilityBehaviorModuleData : UpgradeModuleData
    {
        internal static AutoAbilityBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<AutoAbilityBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<AutoAbilityBehaviorModuleData>
        {
            { "SpecialAbility", (parser, x) => x.SpecialAbility = parser.ParseAssetReference() }
        });

        [AddedIn(SageGame.Bfme2)]
        public string SpecialAbility { get; private set; }
    }
}
