using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public class WeaponOCLNugget : WeaponEffectNuggetData
    {
        internal static WeaponOCLNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<WeaponOCLNugget> FieldParseTable = WeaponEffectNuggetData.FieldParseTable
            .Concat(new IniParseTable<WeaponOCLNugget>
            {
                { "WeaponOCLName", (parser, x) => x.WeaponOCLName = parser.ParseAssetReference() },
            });

        public string WeaponOCLName { get; private set; }
    }
}
