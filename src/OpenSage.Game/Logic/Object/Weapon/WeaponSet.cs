using OpenSage.Data.Sav;

namespace OpenSage.Logic.Object
{
    public sealed class WeaponSet
    {
        private readonly GameObject _gameObject;
        private readonly Weapon[] _weapons;
        private WeaponTemplateSet _currentWeaponTemplateSet;
        private WeaponSlot _currentWeaponSlot;
        private uint _unknown1;
        private uint _filledWeaponSlots;
        private WeaponAntiFlags _combinedAntiMask;
        private uint _unknown2;
        private bool _unknown3;
        private bool _unknown4;

        internal Weapon CurrentWeapon => _weapons[(int) _currentWeaponSlot];

        internal WeaponSet(GameObject gameObject)
        {
            _gameObject = gameObject;

            _weapons = new Weapon[WeaponTemplateSet.NumWeaponSlots];
        }

        internal void Update()
        {
            if (!_gameObject.Definition.WeaponSets.TryGetValue(_gameObject.WeaponSetConditions, out var weaponTemplateSet))
            {
                return;
            }

            if (_currentWeaponTemplateSet == weaponTemplateSet)
            {
                return;
            }

            _currentWeaponTemplateSet = weaponTemplateSet;

            _currentWeaponSlot = WeaponSlot.Primary;

            _filledWeaponSlots = 0;
            _combinedAntiMask = WeaponAntiFlags.None;

            for (var i = 0; i < _weapons.Length; i++)
            {
                var weaponTemplate = _currentWeaponTemplateSet.Slots[i]?.Weapon.Value;
                if (weaponTemplate != null)
                {
                    _weapons[i] = new Weapon(_gameObject, weaponTemplate, (WeaponSlot) i, _gameObject.GameContext);

                    _filledWeaponSlots |= (uint) (1 << i);
                    _combinedAntiMask |= weaponTemplate.AntiMask;
                }
            }
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            // This is the object definition which defined the WeaponSet
            // (either a normal object or DefaultThingTemplate)
            var objectDefinitionName = "";
            reader.ReadAsciiString(ref objectDefinitionName);

            var conditions = reader.ReadBitArray<WeaponSetConditions>();

            _currentWeaponTemplateSet = _gameObject.Definition.WeaponSets[conditions];

            // In Generals there are 3 possible weapons.
            // Later games have up to 5.
            for (var i = 0; i < 3; i++)
            {
                var slotFilled = _weapons[i] != null;
                reader.ReadBoolean(ref slotFilled);
                if (slotFilled)
                {
                    _weapons[i] = new Weapon(_gameObject, _currentWeaponTemplateSet.Slots[i].Weapon.Value, (WeaponSlot) i, _gameObject.GameContext);
                    _weapons[i].Load(reader);
                }
                else
                {
                    _weapons[i] = null;
                }
            }

            reader.ReadEnum(ref _currentWeaponSlot);

            _unknown1 = reader.ReadUInt32();

            _filledWeaponSlots = reader.ReadUInt32();
            reader.ReadEnumFlags(ref _combinedAntiMask);

            _unknown2 = reader.ReadUInt32();

            reader.ReadBoolean(ref _unknown3);
            reader.ReadBoolean(ref _unknown4);
        }
    }
}
