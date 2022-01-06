using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class WeaponSet : IPersistableObject
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

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            var objectDefinitionName = _currentWeaponTemplateSet?.ObjectDefinition.Name;
            reader.PersistAsciiString("ObjectDefinitionName", ref objectDefinitionName);

            var conditions = _currentWeaponTemplateSet?.Conditions ?? new BitArray<WeaponSetConditions>();
            reader.PersistBitArray("Conditions", ref conditions);

            if (reader.Mode == StatePersistMode.Read)
            {
                _currentWeaponTemplateSet = _gameObject.Definition.WeaponSets[conditions];
            }

            // In Generals there are 3 possible weapons.
            // Later games have up to 5.
            reader.BeginArray("Weapons");
            for (var i = 0; i < 3; i++)
            {
                reader.BeginObject();

                var slotFilled = _weapons[i] != null;
                reader.PersistBoolean("SlotFilled", ref slotFilled);
                if (slotFilled)
                {
                    if (reader.Mode == StatePersistMode.Read)
                    {
                        _weapons[i] = new Weapon(
                            _gameObject,
                            _currentWeaponTemplateSet.Slots[i].Weapon.Value,
                            (WeaponSlot)i, _gameObject.GameContext);
                    }
                    reader.PersistObject("Value", _weapons[i]);
                }
                else
                {
                    _weapons[i] = null;
                }

                reader.EndObject();
            }
            reader.EndArray();

            reader.PersistEnum("CurrentWeaponSlot", ref _currentWeaponSlot);
            reader.PersistUInt32("Unknown1", ref _unknown1);
            reader.PersistUInt32("FilledWeaponSlots", ref _filledWeaponSlots);
            reader.PersistEnumFlags("CombinedAntiMask", ref _combinedAntiMask);
            reader.PersistUInt32("Unknown2", ref _unknown2);
            reader.PersistBoolean("Unknown3", ref _unknown3);
            reader.PersistBoolean("Unknown4", ref _unknown4);
        }
    }
}
