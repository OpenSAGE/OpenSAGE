using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class FireWeaponCollide : CollideModule
    {
        private readonly FireWeaponCollideModuleData _moduleData;

        private bool _unknown1;
        private readonly Weapon _collideWeapon;
        private bool _unknown2;

        internal FireWeaponCollide(GameObject gameObject, FireWeaponCollideModuleData moduleData)
        {
            _moduleData = moduleData;

            _collideWeapon = new Weapon(
                gameObject,
                moduleData.CollideWeapon.Value,
                WeaponSlot.Primary,
                gameObject.GameContext);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            base.Load(reader);

            reader.PersistBoolean("Unknown1", ref _unknown1);
            reader.PersistObject("CollideWeapon", _collideWeapon);
            reader.PersistBoolean("Unknown2", ref _unknown2);
        }
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
            return new FireWeaponCollide(gameObject, this);
        }
    }
}
