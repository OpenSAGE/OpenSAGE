using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class FireWeaponCollide : CollideModule
    {
        // TODO
    }

    public sealed class FireWeaponCollideModuleData : CollideModuleData
    {
        internal static FireWeaponCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireWeaponCollideModuleData> FieldParseTable = new IniParseTable<FireWeaponCollideModuleData>
        {
            { "CollideWeapon", (parser, x) => x.CollideWeapon = parser.ParseWeaponTemplateReference() },
            { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnum<ModelConditionFlag>() }
        };

        public LazyAssetReference<WeaponTemplate> CollideWeapon { get; private set; }
        public ModelConditionFlag RequiredStatus { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new FireWeaponCollide();
        }
    }
}
