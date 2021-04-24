using System.IO;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class FireWeaponCollide : CollideModule
    {
        private readonly FireWeaponCollideModuleData _moduleData;
        private readonly Weapon _collideWeapon;

        internal FireWeaponCollide(GameObject gameObject, FireWeaponCollideModuleData moduleData)
        {
            _moduleData = moduleData;

            _collideWeapon = new Weapon(
                gameObject,
                moduleData.CollideWeapon.Value,
                0,
                WeaponSlot.Primary,
                gameObject.GameContext);
        }

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            var unknown1 = reader.ReadBooleanChecked();

            _collideWeapon.Load(reader);

            var unknown2 = reader.ReadBooleanChecked();
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
