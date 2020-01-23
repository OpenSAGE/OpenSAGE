using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class DamageFieldUpdateModuleData : UpdateModuleData
    {
        internal static DamageFieldUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DamageFieldUpdateModuleData> FieldParseTable = new IniParseTable<DamageFieldUpdateModuleData>
        {
            { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
            { "ObjectFilter", (parser, x) => x.ObjectFilter = ObjectFilter.Parse(parser) },
            { "RequiredUpgrade", (parser, x) => x.RequiredUpgrade = parser.ParseAssetReference() },
            { "FireWeaponNugget", (parser, x) => x.FireWeaponNugget = WeaponNugget.Parse(parser) }
        };

        public int Radius { get; private set; }
        public ObjectFilter ObjectFilter { get; private set; }
        public string RequiredUpgrade { get; private set; }
        public WeaponNugget FireWeaponNugget { get; private set; }
    }
}
