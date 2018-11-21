using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class FireWeaponCollideModuleData : CollideModuleData
    {
        internal static FireWeaponCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireWeaponCollideModuleData> FieldParseTable = new IniParseTable<FireWeaponCollideModuleData>
        {
            { "CollideWeapon", (parser, x) => x.CollideWeapon = parser.ParseAssetReference() },
            { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnum<ModelConditionFlag>() }
        };

        public string CollideWeapon { get; private set; }
        public ModelConditionFlag RequiredStatus { get; private set; }
    }
}
