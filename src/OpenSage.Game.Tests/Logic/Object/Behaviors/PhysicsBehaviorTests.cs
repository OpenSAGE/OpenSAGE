using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Tests.Logic.Object.Update;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Behaviors;

public class PhysicsBehaviorTests : UpdateModuleTest<PhysicsBehavior, PhysicsBehaviorModuleData>
{
    /// <summary>
    /// When a unit is stationary, acceleration and velocity are zero.
    /// </summary>
    private static readonly byte[] ZeroHourUnitStationary =
        [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x42, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void ZeroHour_UnitStationary_V2()
    {
        var stream = SaveData(ZeroHourUnitStationary, V2);
        var reader = new StateReader(stream, ZeroHour);
        var behavior = SampleModule(ZeroHour);
        behavior.Load(reader);

        Assert.Equal(Vector3.Zero, behavior.UnknownVector);
        Assert.Equal(Vector3.Zero, behavior.Acceleration);
        Assert.Equal(Vector3.Zero, behavior.LastAcceleration);
        Assert.Equal(Vector3.Zero, behavior.Velocity);
        Assert.Equal(0, behavior.UnknownInt1);
        Assert.Equal(0u, behavior.UnknownInt2);
        Assert.Equal(40u, behavior.UnknownInt3);
        Assert.Equal(50.0f, behavior.Mass);
        Assert.Equal(0u, behavior.UnknownInt4);
        Assert.Equal(0u, behavior.UnknownInt5);
        Assert.Equal(0u, behavior.UnknownFrame.Value);
        Assert.Equal(0, behavior.UnknownByte1);
        Assert.Equal(0, behavior.UnknownByte2);
        Assert.Equal(0.0f, behavior.VelocityMagnitude);
    }

    private static readonly byte[] ZeroHourUnitMovingFrame1 =
        [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe5, 0x38, 0x8e, 0x3f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x42, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void ZeroHour_UnitMovingFrame1_V2()
    {
        var stream = SaveData(ZeroHourUnitMovingFrame1, V2);
        var reader = new StateReader(stream, ZeroHour);
        var behavior = SampleModule(ZeroHour);
        behavior.Load(reader);

        Assert.Equal(Vector3.Zero, behavior.UnknownVector);
        Assert.Equal(new Vector3(1.1111113f, 0, 0), behavior.Acceleration);
        Assert.Equal(Vector3.Zero, behavior.LastAcceleration);
        Assert.Equal(Vector3.Zero, behavior.Velocity);
        Assert.Equal(0, behavior.UnknownInt1);
        Assert.Equal(0u, behavior.UnknownInt2);
        Assert.Equal(40u, behavior.UnknownInt3);
        Assert.Equal(50.0f, behavior.Mass);
        Assert.Equal(0u, behavior.UnknownInt4);
        Assert.Equal(0u, behavior.UnknownInt5);
        Assert.Equal(161u, behavior.UnknownFrame.Value);
        Assert.Equal(0, behavior.UnknownByte1);
        Assert.Equal(0, behavior.UnknownByte2);
        Assert.Equal(0.0f, behavior.VelocityMagnitude);
    }

    private static readonly byte[] ZeroHourUnitMovingFrame2 =
        [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe5, 0x38, 0x0e, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe5, 0x38, 0x0e, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xd4, 0xb4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x42, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0xbf];

    [Fact]
    public void ZeroHour_UnitMovingFrame2_V2()
    {
        var stream = SaveData(ZeroHourUnitMovingFrame2, V2);
        var reader = new StateReader(stream, ZeroHour);
        var behavior = SampleModule(ZeroHour);
        behavior.Load(reader);

        Assert.Equal(Vector3.Zero, behavior.UnknownVector);
        Assert.Equal(Vector3.Zero, behavior.Acceleration);
        Assert.Equal(new Vector3(2.2222226f, 0, 0), behavior.LastAcceleration);
        Assert.Equal(new Vector3(2.2222226f, 0, -3.9488077e-07f), behavior.Velocity);
        Assert.Equal(0, behavior.UnknownInt1);
        Assert.Equal(0u, behavior.UnknownInt2);
        Assert.Equal(40u, behavior.UnknownInt3);
        Assert.Equal(50.0f, behavior.Mass);
        Assert.Equal(0u, behavior.UnknownInt4);
        Assert.Equal(0u, behavior.UnknownInt5);
        Assert.Equal(162u, behavior.UnknownFrame.Value);
        Assert.Equal(0, behavior.UnknownByte1);
        Assert.Equal(0, behavior.UnknownByte2);
        Assert.Equal(-1f, behavior.VelocityMagnitude);
    }

    private static readonly byte[] ZeroHourUnitMovingFrame3 =
        [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4f, 0x8e, 0x63, 0xbe, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x42, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0xbf];

    [Fact]
    public void ZeroHour_UnitMovingFrame3_V2()
    {
        var stream = SaveData(ZeroHourUnitMovingFrame3, V2);
        var reader = new StateReader(stream, ZeroHour);
        var behavior = SampleModule(ZeroHour);
        behavior.Load(reader);

        Assert.Equal(Vector3.Zero, behavior.UnknownVector);
        Assert.Equal(Vector3.Zero, behavior.Acceleration);
        Assert.Equal(new Vector3(-0.22222255f, 0, 0), behavior.LastAcceleration);
        Assert.Equal(new Vector3(2, 0, 0), behavior.Velocity);
        Assert.Equal(0, behavior.UnknownInt1);
        Assert.Equal(0u, behavior.UnknownInt2);
        Assert.Equal(40u, behavior.UnknownInt3);
        Assert.Equal(50.0f, behavior.Mass);
        Assert.Equal(0u, behavior.UnknownInt4);
        Assert.Equal(0u, behavior.UnknownInt5);
        Assert.Equal(163u, behavior.UnknownFrame.Value);
        Assert.Equal(0, behavior.UnknownByte1);
        Assert.Equal(0, behavior.UnknownByte2);
        Assert.Equal(-1f, behavior.VelocityMagnitude);
    }

    [Fact]
    public void AddForceSetsAcceleration()
    {
        var moduleData = new PhysicsBehaviorModuleData
        {
            Mass = 5
        };

        var behavior = SampleModule(ZeroHour, moduleData);

        behavior.AddForce(new Vector3(45, 40, 35));

        Assert.Equal(new Vector3(9, 8, 7), behavior.Acceleration);
        Assert.Equal(Vector3.Zero, behavior.Velocity);
    }

    [Fact]
    public void UpdateCalculatesVelocityAndPosition()
    {
        var objectDefinition = new ObjectDefinition();
        var gameObject = new GameObject(objectDefinition, ZeroHour.Context, null);

        var behavior = SampleModule(ZeroHour, gameObject: gameObject);

        behavior.AddForce(new Vector3(1, 2, 3));

        behavior.Update(null);

        Assert.Equal(Vector3.Zero, behavior.Acceleration);
        Assert.Equal(new Vector3(1, 2, 3), behavior.Velocity);
        Assert.Equal(new Vector3(1, 2, 3), gameObject.Translation);
    }
}
