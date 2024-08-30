#nullable enable

namespace OpenSage.Logic.Object.Damage
{
    /// <summary>
    /// Damage to be dealt, and actual damage dealt.
    /// </summary>
    public struct DamageData : IPersistableObject
    {
        private DamageDataRequest _request;
        private DamageDataResult _result;

        /// <summary>
        /// Attempted damage to be dealt.
        /// </summary>
        public DamageDataRequest Request => _request;

        /// <summary>
        /// Actual damage applied.
        /// </summary>
        public DamageDataResult Result => _result;

        /// <param name="request">Attempted damage to be dealt</param>
        /// <param name="result">Actual damage applied</param>
        public DamageData(DamageDataRequest request, DamageDataResult result)
        {
            _request = request;
            _result = result;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistObject(ref _request);
            reader.PersistObject(ref _result);
        }
    }

    /// <summary>
    /// Raw damage to be applied to a body prior to any calculations.
    /// </summary>
    /// <remarks>
    /// This is also used to heal objects with <see cref="DamageType.Healing"/> and <see cref="DeathType.None"/>.
    /// </remarks>
    public struct DamageDataRequest : IPersistableObject
    {
        /// <summary>
        /// The object dealing the damage.
        /// </summary>
        public uint DamageDealer => _damageDealer;

        public ushort Unknown1 => _unknown1; // some enum?

        /// <summary>
        /// Damage type of the damage being dealt.
        /// </summary>
        public DamageType DamageType => _damageType;

        public DamageType DamageTypeUnknown => _damageTypeUnknown;

        /// <summary>
        /// Death type to apply if the target is killed from this damage.
        /// </summary>
        public DeathType DeathType => _deathType;

        /// <summary>
        /// Raw damage to apply before any armor calculations.
        /// </summary>
        public float DamageToDeal => _damageToDeal;

        /// <summary>
        /// Object Definition name of the unit which conducted this attack.
        /// </summary>
        public string AttackerName => _attackerName ?? string.Empty; // send empty string if null for compatibility with future versions

        private uint _damageDealer;
        private ushort _unknown1; // some enum?
        private DamageType _damageType;
        private DamageType _damageTypeUnknown;
        private DeathType _deathType;
        private float _damageToDeal;
        private string? _attackerName;

        /// <param name="damageDealer">The object dealing the damage</param>
        /// <param name="damageType">Damage type of the damage being dealt</param>
        /// <param name="deathType">Death type to apply if the target is killed from this damage</param>
        /// <param name="damageToDeal">Raw damage to apply before any armor calculations</param>
        /// <param name="damageDealerName">The object definition name of the attacker</param>
        public DamageDataRequest(uint damageDealer, DamageType damageType, DeathType deathType, float damageToDeal, string damageDealerName)
        {
            _damageDealer = damageDealer;
            _damageType = damageType;
            _deathType = deathType;
            _damageToDeal = damageToDeal;
            _attackerName = damageDealerName;
        }

        public void Persist(StatePersister reader)
        {
            var version = reader.PersistVersion(3);

            reader.PersistObjectID(ref _damageDealer);
            reader.PersistUInt16(ref _unknown1);
            reader.PersistEnum(ref _damageType);

            if (version >= 3)
            {
                reader.PersistEnum(ref _damageTypeUnknown);
            }

            reader.PersistEnum(ref _deathType);
            reader.PersistSingle(ref _damageToDeal);

            if (version >= 3)
            {
                reader.SkipUnknownBytes(29);
                reader.PersistAsciiString(ref _attackerName);
            }
        }
    }

    /// <summary>
    /// Actual damage values calculated from damage to be dealt * armor resistance.
    /// </summary>
    public struct DamageDataResult : IPersistableObject
    {
        /// <summary>
        /// Damage to be dealt, factoring in armor.
        /// </summary>
        public float DamageAfterArmorCalculation => _damageAfterArmorCalculation;

        /// <summary>
        /// Actual damage dealt to the object, which may be lower if the object has less HP than <see cref="DamageAfterArmorCalculation"/>, or may be negative if healing.
        /// </summary>
        public float ActualDamageApplied => _actualDamageApplied;

        private float _damageAfterArmorCalculation;
        private float _actualDamageApplied;

        /// <param name="damageAfterArmorCalculation">Damage to be dealt, factoring in armor</param>
        /// <param name="actualDamageApplied"> Actual damage dealt to the object</param>
        public DamageDataResult(float damageAfterArmorCalculation, float actualDamageApplied)
        {
            _damageAfterArmorCalculation = damageAfterArmorCalculation;
            _actualDamageApplied = actualDamageApplied;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistSingle(ref _damageAfterArmorCalculation);
            reader.PersistSingle(ref _actualDamageApplied);

            reader.SkipUnknownBytes(1);
        }
    }
}
