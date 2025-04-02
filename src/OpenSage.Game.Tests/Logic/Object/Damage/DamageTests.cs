using OpenSage.Logic.Object;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Behaviors;

public class DamageTests : StatePersisterTest
{
    #region DamageData

    /// <summary>
    /// Generals default DamageInfo object.
    /// </summary>
    private static readonly byte[] GeneralsDefaultDamageData = [V1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, V1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void DamageData_Default_V1()
    {
        var stream = SaveData(GeneralsDefaultDamageData);
        var reader = new StateReader(stream, Generals);
        var data = new DamageInfo();
        data.Persist(reader);
    }

    #endregion

    #region DamageData Request

    /// <summary>
    /// Generals default DamageInfoInput object.
    /// </summary>
    private static readonly byte[] GeneralsDefaultDamageDataRequest = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void DamageDataRequest_Default_V1()
    {
        var stream = SaveData(GeneralsDefaultDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageInfoInput();
        data.Persist(reader);

        Assert.Equal(ObjectId.Invalid, data.SourceID);
        Assert.Equal(PlayerMaskType.None, data.PlayerMaskType);
        Assert.Equal(DamageType.Explosion, data.DamageType); // this is correct - a DamageType of 0 is Explosion (see Generals_DamageDataRequest_Tomahawk)
        Assert.Equal(DamageType.Unresistable, data.DamageFXOverride); // v3
        Assert.Equal(DeathType.Normal, data.DeathType);
        Assert.Equal(0, data.Amount);
        Assert.Null(data.SourceTemplate); // v3
    }

    /// <summary>
    /// When damage has been applied, a damage dealer is set, damage and death types are set, and damage to deal is set.
    /// </summary>
    private static readonly byte[] GeneralsMissileDefenderDamageDataRequest = [0x07, 0x00, 0x00, 0x00, 0x04, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x42];

    [Fact]
    public void DamageDataRequest_MissileDefender_V1()
    {
        var stream = SaveData(GeneralsMissileDefenderDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageInfoInput();
        data.Persist(reader);

        Assert.Equal(new ObjectId(7u), data.SourceID);
        Assert.Equal(new PlayerMaskType(4), data.PlayerMaskType);
        Assert.Equal(DamageType.InfantryMissile, data.DamageType);
        Assert.Equal(DamageType.Unresistable, data.DamageFXOverride); // v3
        Assert.Equal(DeathType.Normal, data.DeathType);
        Assert.Equal(40, data.Amount, 0.01);
        Assert.Null(data.SourceTemplate); // v3
    }

    /// <summary>
    /// When damage has been applied, a damage dealer is set, damage and death types are set, and damage to deal is set.
    /// </summary>
    private static readonly byte[] GeneralsTomahawkDamageDataRequest = [0x0f, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x16, 0x43];

    [Fact]
    public void DamageDataRequest_Tomahawk_V1()
    {
        var stream = SaveData(GeneralsTomahawkDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageInfoInput();
        data.Persist(reader);

        Assert.Equal(new ObjectId(15u), data.SourceID);
        Assert.Equal(new PlayerMaskType(4), data.PlayerMaskType);
        Assert.Equal(DamageType.Explosion, data.DamageType);
        Assert.Equal(DamageType.Unresistable, data.DamageFXOverride); // v3
        Assert.Equal(DeathType.Exploded, data.DeathType);
        Assert.Equal(150, data.Amount, 0.01);
        Assert.Null(data.SourceTemplate); // v3
    }

    /// <summary>
    /// When damage has been applied, a damage dealer is set, damage and death types are set, and damage to deal is set.
    /// </summary>
    private static readonly byte[] GeneralsHumveeGunDamageDataRequest = [0x11, 0x00, 0x00, 0x00, 0x04, 0x00, 0x1e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x41];

    [Fact]
    public void DamageDataRequest_HumveeGun_V1()
    {
        var stream = SaveData(GeneralsHumveeGunDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageInfoInput();
        data.Persist(reader);

        Assert.Equal(new ObjectId(17u), data.SourceID);
        Assert.Equal(new PlayerMaskType(4), data.PlayerMaskType);
        Assert.Equal(DamageType.ComancheVulcan, data.DamageType);
        Assert.Equal(DamageType.Unresistable, data.DamageFXOverride); // v3
        Assert.Equal(DeathType.Normal, data.DeathType);
        Assert.Equal(8, data.Amount, 0.01);
        Assert.Null(data.SourceTemplate); // v3
    }

    /// <summary>
    /// When damage has been applied, a damage dealer is still, damage and death types are set to be healing-related, and damage to deal is set.
    /// </summary>
    private static readonly byte[] GeneralsHealingDamageDataRequest = [0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x56, 0x55, 0x55, 0x40];

    [Fact]
    public void DamageDataRequest_Healing_V1()
    {
        var stream = SaveData(GeneralsHealingDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageInfoInput();
        data.Persist(reader);

        Assert.Equal(new ObjectId(5u), data.SourceID);
        Assert.Equal(PlayerMaskType.None, data.PlayerMaskType);
        Assert.Equal(DamageType.Healing, data.DamageType);
        Assert.Equal(DamageType.Unresistable, data.DamageFXOverride); // v3
        Assert.Equal(DeathType.None, data.DeathType);
        Assert.Equal(3.333, data.Amount, 0.01);
        Assert.Null(data.SourceTemplate); // v3
    }

    /// <summary>
    /// When damaged by a burning ember, the damage dealer is the burning unit.
    /// </summary>
    private static readonly byte[] GeneralsBurningEmberDamageDataRequest = [0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x40];

    [Fact]
    public void DamageDataRequest_BurningEmber_V1()
    {
        var stream = SaveData(GeneralsBurningEmberDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageInfoInput();
        data.Persist(reader);

        Assert.Equal(new ObjectId(5u), data.SourceID);
        Assert.Equal(PlayerMaskType.None, data.PlayerMaskType);
        Assert.Equal(DamageType.Flame, data.DamageType);
        Assert.Equal(DamageType.Unresistable, data.DamageFXOverride); // v3
        Assert.Equal(DeathType.Burned, data.DeathType);
        Assert.Equal(3, data.Amount, 0.01);
        Assert.Null(data.SourceTemplate); // v3
    }

    /// <summary>
    /// Tnt attacks have a death type of suicided
    /// </summary>
    private static readonly byte[] GeneralsTntDamageDataRequest = [0x07, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0xfa, 0x43];

    [Fact]
    public void DamageDataRequest_Tnt_V1()
    {
        var stream = SaveData(GeneralsTntDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageInfoInput();
        data.Persist(reader);

        Assert.Equal(new ObjectId(7u), data.SourceID);
        Assert.Equal(new PlayerMaskType(4), data.PlayerMaskType);
        Assert.Equal(DamageType.Explosion, data.DamageType);
        Assert.Equal(DamageType.Unresistable, data.DamageFXOverride); // v3
        Assert.Equal(DeathType.Suicided, data.DeathType);
        Assert.Equal(500, data.Amount, 0.01);
        Assert.Null(data.SourceTemplate); // v3
    }

    /// <summary>
    /// Tnt attacks have a death type of suicided
    /// </summary>
    private static readonly byte[] GeneralsToxinFieldDamageDataRequest = [0x1c, 0x00, 0x00, 0x00, 0x04, 0x00, 0x09, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40];

    [Fact]
    public void DamageDataRequest_ToxinField_V1()
    {
        var stream = SaveData(GeneralsToxinFieldDamageDataRequest);
        var reader = new StateReader(stream, Generals);
        var data = new DamageInfoInput();
        data.Persist(reader);

        Assert.Equal(new ObjectId(28u), data.SourceID);
        Assert.Equal(new PlayerMaskType(4), data.PlayerMaskType);
        Assert.Equal(DamageType.Poison, data.DamageType);
        Assert.Equal(DamageType.Unresistable, data.DamageFXOverride); // v3
        Assert.Equal(DeathType.Poisoned, data.DeathType);
        Assert.Equal(2, data.Amount, 0.01);
        Assert.Null(data.SourceTemplate); // v3
    }

    /// <summary>
    /// Zero Hour default DamageInfoInput object.
    /// </summary>
    private static readonly byte[] ZeroHourDefaultDamageDataRequest = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void DamageDataRequest_Default_V3()
    {
        var stream = SaveData(ZeroHourDefaultDamageDataRequest, V3);
        var reader = new StateReader(stream, ZeroHour);
        var data = new DamageInfoInput();
        data.Persist(reader);

        Assert.Equal(ObjectId.Invalid, data.SourceID);
        Assert.Equal(PlayerMaskType.None, data.PlayerMaskType);
        Assert.Equal(DamageType.Explosion, data.DamageType);
        Assert.Equal(DamageType.Unresistable, data.DamageFXOverride);
        Assert.Equal(DeathType.Normal, data.DeathType);
        Assert.Equal(0, data.Amount);
        Assert.Null(data.SourceTemplate);
    }


    /// <summary>
    /// When damage has been applied, in addition to the V1 fields and attacker name will also be set.
    /// </summary>
    private static readonly byte[] ZeroHourMissileDefenderDamageDataRequest = [0x05, 0x00, 0x00, 0x00, 0x04, 0x00, 0x18, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x42, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1e, 0x41, 0x6d, 0x65, 0x72, 0x69, 0x63, 0x61, 0x49, 0x6e, 0x66, 0x61, 0x6e, 0x74, 0x72, 0x79, 0x4d, 0x69, 0x73, 0x73, 0x69, 0x6c, 0x65, 0x44, 0x65, 0x66, 0x65, 0x6e, 0x64, 0x65, 0x72];

    [Fact]
    public void DamageDataRequest_MissileDefender_V3()
    {
        ZeroHour.AssetStore.ObjectDefinitions.Add(new ObjectDefinition("AmericaInfantryMissileDefender"));

        var stream = SaveData(ZeroHourMissileDefenderDamageDataRequest, V3);
        var reader = new StateReader(stream, ZeroHour);
        var data = new DamageInfoInput();
        data.Persist(reader);

        Assert.Equal(new ObjectId(5u), data.SourceID);
        Assert.Equal(new PlayerMaskType(4), data.PlayerMaskType);
        Assert.Equal(DamageType.InfantryMissile, data.DamageType);
        Assert.Equal(DamageType.Unresistable, data.DamageFXOverride);
        Assert.Equal(DeathType.Normal, data.DeathType);
        Assert.Equal(40, data.Amount, 0.01);
        Assert.Equal("AmericaInfantryMissileDefender", data.SourceTemplate.Name);
    }

    #endregion

    #region DamageData Result

    /// <summary>
    /// Generals default DamageInfoOutput object.
    /// </summary>
    private static readonly byte[] GeneralsDefaultDamageDataResult = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void DamageDataResult_Default_V1()
    {
        var stream = SaveData(GeneralsDefaultDamageDataResult);
        var reader = new StateReader(stream, Generals);
        var data = new DamageInfoOutput();
        data.Persist(reader);

        Assert.Equal(0, data.ActualDamageDealt);
        Assert.Equal(0, data.ActualDamageClipped);
    }

    /// <summary>
    ///
    /// </summary>
    private static readonly byte[] GeneralsMissileDamageDataResult = [0x00, 0x00, 0x20, 0x42, 0x00, 0x00, 0x20, 0x42, 0x00];

    [Fact]
    public void DamageDataResult_Missile_V1()
    {
        var stream = SaveData(GeneralsMissileDamageDataResult);
        var reader = new StateReader(stream, Generals);
        var data = new DamageInfoOutput();
        data.Persist(reader);

        Assert.Equal(40, data.ActualDamageDealt, 0.01);
        Assert.Equal(40, data.ActualDamageClipped, 0.01);
    }

    /// <summary>
    /// When an object is healing, the actual damage applied is negative.
    /// </summary>
    private static readonly byte[] GeneralsHealingDamageDataResult = [0x56, 0x55, 0x55, 0x40, 0x00, 0x48, 0x55, 0xc0, 0x00];

    [Fact]
    public void DamageDataResult_V1()
    {
        var stream = SaveData(GeneralsHealingDamageDataResult);
        var reader = new StateReader(stream, Generals);
        var data = new DamageInfoOutput();
        data.Persist(reader);

        Assert.Equal(3.333, data.ActualDamageDealt, 0.01);
        Assert.Equal(-3.333, data.ActualDamageClipped, 0.01);
    }

    #endregion
}
