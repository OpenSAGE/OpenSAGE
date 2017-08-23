using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class FireWeaponCollide : ObjectBehavior
    {
        internal static FireWeaponCollide Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireWeaponCollide> FieldParseTable = new IniParseTable<FireWeaponCollide>
        {
            { "CollideWeapon", (parser, x) => x.CollideWeapon = parser.ParseAssetReference() },
            { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnum<ModelConditionFlag>() }
        };

        public string CollideWeapon { get; private set; }
        public ModelConditionFlag RequiredStatus { get; private set; }
    }
}
