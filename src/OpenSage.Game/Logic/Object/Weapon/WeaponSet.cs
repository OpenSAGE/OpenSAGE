using OpenSage.Data.Sav;

namespace OpenSage.Logic.Object
{
    public sealed class WeaponSet
    {
        private readonly Weapon[] _weapons;
        private WeaponTemplateSet _currentWeaponTemplateSet;

        internal WeaponSet()
        {
            // TODO: Later games can have 5 weapons.
            _weapons = new Weapon[3];
        }

        internal void Load(SaveFileReader reader, GameObject gameObject)
        {
            reader.ReadVersion(1);

            // This is the object definition which defined the WeaponSet
            // (either a normal object or DefaultThingTemplate)
            var objectDefinitionName = reader.ReadAsciiString();

            var conditions = reader.ReadBitArray<WeaponSetConditions>();

            _currentWeaponTemplateSet = gameObject.Definition.WeaponSets[conditions];

            // In Generals there are 3 possible weapons.
            // Later games have up to 5.
            for (var i = 0; i < 3; i++)
            {
                var weaponSlot = (WeaponSlot) i;

                var slotFilled = reader.ReadBoolean();
                if (slotFilled)
                {
                    _weapons[i] = new Weapon(gameObject, _currentWeaponTemplateSet.Slots[weaponSlot].Weapon.Value, i, weaponSlot, gameObject.GameContext);
                    _weapons[i].Load(reader);
                }
            }

            var unknown1 = reader.ReadUInt32();
            var unknown2 = reader.ReadUInt32();
            var unknown3 = reader.ReadUInt32();
            var unknown4 = reader.ReadUInt32();
            var unknown5 = reader.ReadUInt32();

            var unknownBool1 = reader.ReadBoolean();
            var unknownBool2 = reader.ReadBoolean();
        }
    }
}
