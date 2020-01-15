using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class DamageFieldNugget : WeaponEffectNuggetData
    {
        internal static DamageFieldNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DamageFieldNugget> FieldParseTable = WeaponEffectNuggetData.FieldParseTable
            .Concat(new IniParseTable<DamageFieldNugget>
            {
                { "WeaponTemplateName", (parser, x) => x.WeaponTemplateName = parser.ParseAssetReference() },
                { "Duration", (parser, x) => x.Duration = parser.ParseInteger() }
            });

        public string WeaponTemplateName { get; private set; }
        public int Duration { get; private set; }
    }
}
