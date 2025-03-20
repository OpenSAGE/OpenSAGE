#nullable enable

using System.Numerics;

namespace OpenSage.Logic.Object;

/// <summary>
/// Descriptor of damage we're trying to inflict, and actual damage dealt.
/// </summary>
public struct DamageInfo : IPersistableObject
{
    /// <summary>
    /// Attempted damage to be dealt.
    /// </summary>
    public DamageInfoInput Request;

    /// <summary>
    /// Actual damage applied.
    /// </summary>
    public DamageInfoOutput Result;

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
/// This is also used to heal objects with <see cref="DamageType.Healing"/>
/// and <see cref="DeathType.None"/>.
/// </remarks>
public struct DamageInfoInput() : IPersistableObject
{
    private ObjectId _sourceID = ObjectId.Invalid;
    private ObjectDefinition? _sourceTemplate;

    /// <summary>
    /// Source of the damage.
    /// </summary>
    public ObjectId SourceID => _sourceID;

    /// <summary>
    /// Source of the damage (the template).
    /// </summary>
    public ObjectDefinition? SourceTemplate => _sourceTemplate;

    /// <summary>
    /// Player mask of <see cref="SourceID"/>.
    /// </summary>
    public PlayerMaskType PlayerMaskType;

    /// <summary>
    /// Type of damage.
    /// </summary>
    public DamageType DamageType = DamageType.Explosion;

    /// <summary>
    /// If status damage, what type.
    /// </summary>
    public ObjectStatus DamageStatusType;

    /// <summary>
    /// If not marked as the default of <see cref="DamageType.Unresistable"/>,
    /// the damage to use in DoDamageFX instead of the real damage type.
    /// </summary>
    public DamageType DamageFXOverride = DamageType.Unresistable;

    /// <summary>
    /// If this kills us, death type to be used.
    /// </summary>
    public DeathType DeathType = DeathType.Normal;

    /// <summary>
    /// How much damage to inflict (before any armor calculations).
    /// </summary>
    public float Amount;

    // The following fields are used for damage causing shockwave, forcing
    // units affected to be pushed around.

    /// <summary>
    /// Will always cause object to die regardless of damage.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public bool Kill;

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

    public DamageInfoInput(GameObject? source)
        : this()
    {
        _sourceID = source?.Id ?? ObjectId.Invalid;
        _sourceTemplate = source?.Definition;
    }

    public void Persist(StatePersister reader)
    {
        var version = reader.PersistVersion(3);

        reader.PersistObjectId(ref _sourceID);
        reader.PersistEnumUInt16(ref PlayerMaskType);
        reader.PersistEnum(ref DamageType);

        if (version >= 3)
        {
            reader.PersistEnum(ref DamageFXOverride);
        }

        reader.PersistEnum(ref DeathType);
        reader.PersistSingle(ref Amount);

        if (version >= 3)
        {
            reader.PersistBoolean(ref Kill);
            reader.PersistEnum(ref DamageStatusType);
            reader.PersistVector3(ref ShockWaveVector);
            reader.PersistSingle(ref ShockWaveAmount);
            reader.PersistSingle(ref ShockWaveRadius);
            reader.PersistSingle(ref ShockWaveTaperOff);

            var attackerName = SourceTemplate?.Name ?? "";
            reader.PersistAsciiString(ref attackerName);
            if (reader.Mode == StatePersistMode.Read)
            {
                _sourceTemplate = reader.AssetStore.ObjectDefinitions.GetByName(attackerName);
            }
            
        }
    }
}

/// <summary>
/// Actual damage values calculated from damage to be dealt * armor resistance.
/// </summary>
public struct DamageInfoOutput : IPersistableObject
{
    /// <summary>
    /// The damage we tried to apply to object (after multipliers and such).
    /// </summary>
    public float ActualDamageDealt;

    /// <summary>
    /// Value of <see cref="ActualDamageDealt"/>, but clipped to the max health
    /// remaining of the object.
    ///
    /// Example:
    ///   A mammoth tank fires a round at a small tank, attempting 100 damage.
    ///   The small tank has a damage multiplier of 50%, meaning that only 50
    ///   damage is applied.
    ///   Furthermore, the small tank has only 30 health remaining.
    ///   so: <see cref="ActualDamageDealt"/> = 50,
    ///   <see cref="ActualDamageClipped"/> = 30.
    ///
    /// This distinction is useful, since visual fx really wants to do the fx
    /// for "50 damage", even though it was more than necessary to kill this
    /// object. Game logic, on the other hand, may want to know the "clipped"
    /// damage for AI purposes.
    /// </summary>
    public float ActualDamageClipped;

    /// <summary>
    /// If true, no damage was done at all (generally due to being <see cref="InactiveBody"/>).
    /// </summary>
    public bool NoEffect;

    public void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.PersistSingle(ref ActualDamageDealt);
        reader.PersistSingle(ref ActualDamageClipped);
        reader.PersistBoolean(ref NoEffect);
    }
}
