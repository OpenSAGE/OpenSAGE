using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of WeaponBonus = parameter on this object's weapons.
    /// </summary>
    public sealed class WeaponBonusUpgrade : ObjectBehavior
    {
        internal static WeaponBonusUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<WeaponBonusUpgrade> FieldParseTable = new IniParseTable<WeaponBonusUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() }
        };

        public string TriggeredBy { get; private set; }
    }
}
