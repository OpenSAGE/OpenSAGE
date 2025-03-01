using OpenSage.Logic.Object;
using Xunit;

namespace OpenSage.Tests.Logic.Object;

public class WeaponTests : StatePersisterTest
{
    /// <summary>
    /// Weapon that has never been fired.
    /// </summary>
    private static readonly byte[] ZeroHourInactiveSecondaryWeapon = [0x1B, 0x48, 0x75, 0x6D, 0x76, 0x65, 0x65, 0x4D, 0x69, 0x73, 0x73, 0x69, 0x6C, 0x65, 0x57, 0x65, 0x61, 0x70, 0x6F, 0x6E, 0x41, 0x69, 0x72, 0x44, 0x75, 0x6D, 0x6D, 0x79, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x7F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x7F, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void InactiveSecondaryWeapon_V3()
    {
        var stream = SaveData(ZeroHourInactiveSecondaryWeapon);
        var reader = new StateReader(stream, ZeroHour);
        var weaponTemplate = new WeaponTemplate
        {
            Name = "HumveeMissileWeaponAirDummy"
        };
        var weapon = new Weapon(CreateTestGameObject(), weaponTemplate, WeaponSlot.Secondary, ZeroHour.Context);
        weapon.Persist(reader);

        Assert.Equal(weaponTemplate, weapon.Template);
        Assert.Equal(WeaponSlot.Secondary, weapon.Slot);
        Assert.Equal(WeaponStatus.Ready, weapon.Status);
        Assert.Equal(int.MaxValue, weapon.ShotsLeftInClip);
        Assert.Equal(0u, weapon.NextFrameToFire.Value);
        Assert.Equal(0u, weapon.LastFrameFired1.Value);
        Assert.Equal(0u, weapon.LastFrameFired1.Value);
        Assert.Equal(0u, weapon.UnknownFrame.Value);
        Assert.Equal(0u, weapon.UnknownObjectId);
        Assert.Equal(int.MaxValue, weapon.MaxShots);
        Assert.Equal(0u, weapon.UnknownInt1);
        Assert.Equal(1u, weapon.UnknownInt2);
        Assert.False(weapon.UnknownBool1);
        Assert.False(weapon.UnknownBool2);
    }

    /// <summary>
    /// Weapon that has already been fired, and is going to fire again this frame (523).
    /// </summary>
    private static readonly byte[] ZeroHourPrimaryWeaponZeroClipSizeFiringThisFrame = [0x09, 0x48, 0x75, 0x6D, 0x76, 0x65, 0x65, 0x47, 0x75, 0x6E, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0xC1, 0xFF, 0xFF, 0x7F, 0x0B, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x02, 0x00, 0x00, 0x05, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC1, 0xFF, 0xFF, 0x7F, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void PrimaryWeaponZeroClipSizeFiringThisFrame_V3()
    {
        var stream = SaveData(ZeroHourPrimaryWeaponZeroClipSizeFiringThisFrame);
        var reader = new StateReader(stream, ZeroHour);
        var weaponTemplate = new WeaponTemplate
        {
            Name = "HumveeGun"
        };
        var weapon = new Weapon(CreateTestGameObject(), weaponTemplate, WeaponSlot.Primary, ZeroHour.Context);
        weapon.Persist(reader);

        Assert.Equal(weaponTemplate, weapon.Template);
        Assert.Equal(WeaponSlot.Primary, weapon.Slot);
        Assert.Equal(WeaponStatus.BetweenShots, weapon.Status);
        Assert.Equal(2147483585, weapon.ShotsLeftInClip);
        Assert.Equal(523u, weapon.NextFrameToFire.Value);
        Assert.Equal(517u, weapon.LastFrameFired1.Value);
        Assert.Equal(517u, weapon.LastFrameFired1.Value);
        Assert.Equal(0u, weapon.UnknownFrame.Value);
        Assert.Equal(0u, weapon.UnknownObjectId);
        Assert.Equal(2147483585, weapon.MaxShots);
        Assert.Equal(1u, weapon.UnknownInt1);
        Assert.Equal(1u, weapon.UnknownInt2);
        Assert.False(weapon.UnknownBool1);
        Assert.False(weapon.UnknownBool2);
    }

    /// <summary>
    /// Weapon that was fired on the previous frame (523) and we're now on frame 524.
    /// </summary>
    private static readonly byte[] ZeroHourPrimaryWeaponZeroClipSizeFiredPreviousFrame = [0x09, 0x48, 0x75, 0x6D, 0x76, 0x65, 0x65, 0x47, 0x75, 0x6E, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0xC0, 0xFF, 0xFF, 0x7F, 0x11, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x02, 0x00, 0x00, 0x0B, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC0, 0xFF, 0xFF, 0x7F, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void PrimaryWeaponZeroClipSizeFiredPreviousFrame_V3()
    {
        var stream = SaveData(ZeroHourPrimaryWeaponZeroClipSizeFiredPreviousFrame);
        var reader = new StateReader(stream, ZeroHour);
        var weaponTemplate = new WeaponTemplate
        {
            Name = "HumveeGun"
        };
        var weapon = new Weapon(CreateTestGameObject(), weaponTemplate, WeaponSlot.Primary, ZeroHour.Context);
        weapon.Persist(reader);

        Assert.Equal(weaponTemplate, weapon.Template);
        Assert.Equal(WeaponSlot.Primary, weapon.Slot);
        Assert.Equal(WeaponStatus.BetweenShots, weapon.Status);
        Assert.Equal(2147483584, weapon.ShotsLeftInClip);
        Assert.Equal(529u, weapon.NextFrameToFire.Value);
        Assert.Equal(523u, weapon.LastFrameFired1.Value);
        Assert.Equal(523u, weapon.LastFrameFired1.Value);
        Assert.Equal(0u, weapon.UnknownFrame.Value);
        Assert.Equal(0u, weapon.UnknownObjectId);
        Assert.Equal(2147483584, weapon.MaxShots);
        Assert.Equal(1u, weapon.UnknownInt1);
        Assert.Equal(1u, weapon.UnknownInt2);
        Assert.False(weapon.UnknownBool1);
        Assert.False(weapon.UnknownBool2);
    }

    /// <summary>
    /// Weapon that started reloading a few frames ago.
    /// </summary>
    private static readonly byte[] ZeroHourPrimaryWeaponWithClipSizeReloading = [0x19, 0x52, 0x61, 0x6E, 0x67, 0x65, 0x72, 0x41, 0x64, 0x76, 0x61, 0x6E, 0x63, 0x65, 0x64, 0x43, 0x6F, 0x6D, 0x62, 0x61, 0x74, 0x52, 0x69, 0x66, 0x6C, 0x65, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0xB6, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA1, 0x00, 0x00, 0x00, 0xA1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFC, 0xFF, 0xFF, 0x7F, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void PrimaryWeaponWithClipSizeReloading_V3()
    {
        var stream = SaveData(ZeroHourPrimaryWeaponWithClipSizeReloading);
        var reader = new StateReader(stream, ZeroHour);
        var weaponTemplate = new WeaponTemplate
        {
            Name = "RangerAdvancedCombatRifle"
        };
        var weapon = new Weapon(CreateTestGameObject(), weaponTemplate, WeaponSlot.Primary, ZeroHour.Context);
        weapon.Persist(reader);

        Assert.Equal(weaponTemplate, weapon.Template);
        Assert.Equal(WeaponSlot.Primary, weapon.Slot);
        Assert.Equal(WeaponStatus.Reloading, weapon.Status);
        Assert.Equal(3, weapon.ShotsLeftInClip);
        Assert.Equal(182u, weapon.NextFrameToFire.Value);
        Assert.Equal(161u, weapon.LastFrameFired1.Value);
        Assert.Equal(161u, weapon.LastFrameFired1.Value);
        Assert.Equal(0u, weapon.UnknownFrame.Value);
        Assert.Equal(0u, weapon.UnknownObjectId);
        Assert.Equal(2147483644, weapon.MaxShots);
        Assert.Equal(1u, weapon.UnknownInt1);
        Assert.Equal(1u, weapon.UnknownInt2);
        Assert.False(weapon.UnknownBool1);
        Assert.False(weapon.UnknownBool2);
    }

    [Fact]
    public void InitialState()
    {
        var weaponTemplate = new WeaponTemplate
        {
            ClipSize = 3,
        };
        var weapon = new Weapon(CreateTestGameObject(), weaponTemplate, WeaponSlot.Primary, ZeroHour.Context);

        Assert.Equal(WeaponSlot.Primary, weapon.Slot);
        Assert.Equal(WeaponStatus.Ready, weapon.Status);
        Assert.Equal(3, weapon.ShotsLeftInClip);
        Assert.Equal(0u, weapon.NextFrameToFire.Value);
        Assert.Equal(0u, weapon.LastFrameFired1.Value);
        Assert.Equal(0u, weapon.LastFrameFired1.Value);
        Assert.Equal(0x7FFFFFFF, weapon.MaxShots);
    }

    private GameObject CreateTestGameObject()
    {
        var objectDefinition = new ObjectDefinition();
        return new GameObject(objectDefinition, ZeroHour.Context, null);
    }
}
