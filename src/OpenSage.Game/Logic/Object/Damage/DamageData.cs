#nullable enable

using System.Numerics;

namespace OpenSage.Logic.Object;

/// <summary>
/// Damage to be dealt, and actual damage dealt.
/// </summary>
public struct DamageData : IPersistableObject
{
    /// <summary>
    /// Attempted damage to be dealt.
    /// </summary>
    public DamageDataRequest Request;

    /// <summary>
    /// Actual damage applied.
    /// </summary>
    public DamageDataResult Result;

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistObject(ref Request);
        reader.PersistObject(ref Result);
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
    /// Source of the damage.
    /// </summary>
    public uint DamageDealer;

    public ushort Unknown1 => _unknown1; // some enum?

    /// <summary>
    /// Damage type of the damage being dealt.
    /// </summary>
    public DamageType DamageType;

    public DamageType DamageTypeUnknown => _damageTypeUnknown;

    /// <summary>
    /// Death type to apply if the target is killed from this damage.
    /// </summary>
    public DeathType DeathType;

    /// <summary>
    /// Raw damage to apply before any armor calculations.
    /// </summary>
    public float DamageToDeal;

    /// <summary>
    /// Object Definition name of the unit which conducted this attack.
    /// </summary>
    public string AttackerName;

    private ushort _unknown1; // some enum?
    private DamageType _damageTypeUnknown;

    /// <summary>
    /// Will always cause object to die regardless of damage.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public bool Kill;

    /// <summary>
    /// If status damage, what type.
    /// </summary>
    public ObjectStatus DamageStatusType;

    /// <summary>
    /// Incoming damage vector.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public Vector3 ShockWaveVector;

    /// <summary>
    /// Amount of shockwave created by the damage. 0 = no shockwave, 1 = shockwave equal to damage.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public float ShockWaveAmount;

    /// <summary>
    /// Effect radius of the shockwave.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public float ShockWaveRadius;

    /// <summary>
    /// Taper off effect of the shockwave at the tip of the radius. 0 means shockwave is 0% at the radius edge.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public float ShockWaveTaperOff;

    /// <param name="damageDealer">The object dealing the damage</param>
    /// <param name="damageType">Damage type of the damage being dealt</param>
    /// <param name="deathType">Death type to apply if the target is killed from this damage</param>
    /// <param name="damageToDeal">Raw damage to apply before any armor calculations</param>
    /// <param name="damageDealerName">The object definition name of the attacker</param>
    public DamageDataRequest(uint damageDealer, DamageType damageType, DeathType deathType, float damageToDeal, string damageDealerName)
    {
        DamageDealer = damageDealer;
        DamageType = damageType;
        DeathType = deathType;
        DamageToDeal = damageToDeal;
        AttackerName = damageDealerName;
    }

    public void Persist(StatePersister reader)
    {
        var version = reader.PersistVersion(3);

        reader.PersistObjectID(ref DamageDealer);
        reader.PersistUInt16(ref _unknown1);
        reader.PersistEnum(ref DamageType);

        if (version >= 3)
        {
            reader.PersistEnum(ref _damageTypeUnknown);
        }

        reader.PersistEnum(ref DeathType);
        reader.PersistSingle(ref DamageToDeal);

        if (version >= 3)
        {
            reader.PersistBoolean(ref Kill);
            reader.PersistEnum(ref DamageStatusType);
            reader.PersistVector3(ref ShockWaveVector);
            reader.PersistSingle(ref ShockWaveAmount);
            reader.PersistSingle(ref ShockWaveRadius);
            reader.PersistSingle(ref ShockWaveTaperOff);
            reader.PersistAsciiString(ref AttackerName);
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
    public float DamageAfterArmorCalculation;

    /// <summary>
    /// Actual damage dealt to the object, which may be lower if the object has less HP than <see cref="DamageAfterArmorCalculation"/>, or may be negative if healing.
    /// </summary>
    public float ActualDamageApplied;

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistSingle(ref DamageAfterArmorCalculation);
        reader.PersistSingle(ref ActualDamageApplied);

        reader.SkipUnknownBytes(1);
    }
}
