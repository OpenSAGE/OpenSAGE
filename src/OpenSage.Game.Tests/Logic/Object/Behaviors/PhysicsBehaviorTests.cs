using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Tests.Logic.Object.Update;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Behaviors;

public class PhysicsBehaviorTests : UpdateModuleTest<PhysicsBehavior, PhysicsBehaviorModuleData>
{
    protected override UpdateOrder UpdateOrder => UpdateOrder.Order1;

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
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.Equal(0.0f, behavior.YawRate);
        Assert.Equal(0.0f, behavior.RollRate);
        Assert.Equal(0.0f, behavior.PitchRate);
        Assert.Equal(Vector3.Zero, behavior.Acceleration);
        Assert.Equal(Vector3.Zero, behavior.LastAcceleration);
        Assert.Equal(Vector3.Zero, behavior.Velocity);
        Assert.Equal(PhysicsTurningType.None, behavior.Turning);
        Assert.Equal(0u, behavior.IgnoreCollisionsWith);
        Assert.Equal(PhysicsFlagType.AllowCollideForce | PhysicsFlagType.UpdateEverRun, behavior.Flags);
        Assert.Equal(50.0f, behavior.Mass);
        Assert.Equal(0u, behavior.CurrentOverlap);
        Assert.Equal(0u, behavior.PreviousOverlap);
        Assert.Equal(0u, behavior.MotiveForceExpires.Value);
        Assert.Equal(0.0f, behavior.ExtraBounciness);
        Assert.Equal(0.0f, behavior.ExtraFriction);
        Assert.Equal(0.0f, behavior.VelocityMagnitude);
    }

    private static readonly byte[] ZeroHourUnitMovingFrame1 =
        [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe5, 0x38, 0x8e, 0x3f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x42, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void ZeroHour_UnitMovingFrame1_V2()
    {
        var stream = SaveData(ZeroHourUnitMovingFrame1, V2);
        var reader = new StateReader(stream, ZeroHour);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.Equal(0.0f, behavior.YawRate);
        Assert.Equal(0.0f, behavior.RollRate);
        Assert.Equal(0.0f, behavior.PitchRate);
        Assert.Equal(new Vector3(1.1111113f, 0, 0), behavior.Acceleration);
        Assert.Equal(Vector3.Zero, behavior.LastAcceleration);
        Assert.Equal(Vector3.Zero, behavior.Velocity);
        Assert.Equal(PhysicsTurningType.None, behavior.Turning);
        Assert.Equal(0u, behavior.IgnoreCollisionsWith);
        Assert.Equal(PhysicsFlagType.AllowCollideForce | PhysicsFlagType.UpdateEverRun, behavior.Flags);
        Assert.Equal(50.0f, behavior.Mass);
        Assert.Equal(0u, behavior.CurrentOverlap);
        Assert.Equal(0u, behavior.PreviousOverlap);
        Assert.Equal(161u, behavior.MotiveForceExpires.Value);
        Assert.Equal(0.0f, behavior.ExtraBounciness);
        Assert.Equal(0.0f, behavior.ExtraFriction);
        Assert.Equal(0.0f, behavior.VelocityMagnitude);
    }

    private static readonly byte[] ZeroHourUnitMovingFrame2 =
        [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe5, 0x38, 0x0e, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe5, 0x38, 0x0e, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xd4, 0xb4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x42, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0xbf];

    [Fact]
    public void ZeroHour_UnitMovingFrame2_V2()
    {
        var stream = SaveData(ZeroHourUnitMovingFrame2, V2);
        var reader = new StateReader(stream, ZeroHour);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.Equal(0.0f, behavior.YawRate);
        Assert.Equal(0.0f, behavior.RollRate);
        Assert.Equal(0.0f, behavior.PitchRate);
        Assert.Equal(Vector3.Zero, behavior.Acceleration);
        Assert.Equal(new Vector3(2.2222226f, 0, 0), behavior.LastAcceleration);
        Assert.Equal(new Vector3(2.2222226f, 0, -3.9488077e-07f), behavior.Velocity);
        Assert.Equal(PhysicsTurningType.None, behavior.Turning);
        Assert.Equal(0u, behavior.IgnoreCollisionsWith);
        Assert.Equal(PhysicsFlagType.AllowCollideForce | PhysicsFlagType.UpdateEverRun, behavior.Flags);
        Assert.Equal(50.0f, behavior.Mass);
        Assert.Equal(0u, behavior.CurrentOverlap);
        Assert.Equal(0u, behavior.PreviousOverlap);
        Assert.Equal(162u, behavior.MotiveForceExpires.Value);
        Assert.Equal(0.0f, behavior.ExtraBounciness);
        Assert.Equal(0.0f, behavior.ExtraFriction);
        Assert.Equal(-1f, behavior.VelocityMagnitude);
    }

    private static readonly byte[] ZeroHourUnitMovingFrame3 =
        [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4f, 0x8e, 0x63, 0xbe, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x42, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa3, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0xbf];

    [Fact]
    public void ZeroHour_UnitMovingFrame3_V2()
    {
        var stream = SaveData(ZeroHourUnitMovingFrame3, V2);
        var reader = new StateReader(stream, ZeroHour);
        var behavior = SampleModule();
        behavior.Load(reader);

        Assert.Equal(0.0f, behavior.YawRate);
        Assert.Equal(0.0f, behavior.RollRate);
        Assert.Equal(0.0f, behavior.PitchRate);
        Assert.Equal(Vector3.Zero, behavior.Acceleration);
        Assert.Equal(new Vector3(-0.22222255f, 0, 0), behavior.LastAcceleration);
        Assert.Equal(new Vector3(2, 0, 0), behavior.Velocity);
        Assert.Equal(PhysicsTurningType.None, behavior.Turning);
        Assert.Equal(0u, behavior.IgnoreCollisionsWith);
        Assert.Equal(PhysicsFlagType.AllowCollideForce | PhysicsFlagType.UpdateEverRun, behavior.Flags);
        Assert.Equal(50.0f, behavior.Mass);
        Assert.Equal(0u, behavior.CurrentOverlap);
        Assert.Equal(0u, behavior.PreviousOverlap);
        Assert.Equal(163u, behavior.MotiveForceExpires.Value);
        Assert.Equal(0.0f, behavior.ExtraBounciness);
        Assert.Equal(0.0f, behavior.ExtraFriction);
        Assert.Equal(-1f, behavior.VelocityMagnitude);
    }

    [Fact]
    public void AddForceSetsAcceleration()
    {
        var moduleData = new PhysicsBehaviorModuleData
        {
            Mass = 5
        };

        var behavior = SampleModule(moduleData);

        behavior.ApplyForce(new Vector3(45, 40, 35));

        Assert.Equal(new Vector3(9, 8, 7), behavior.Acceleration);
        Assert.Equal(Vector3.Zero, behavior.Velocity);
    }

    [Fact]
    public void UpdateCalculatesVelocityAndPosition()
    {
        var behavior = SampleModule(out var gameObject);

        behavior.ApplyForce(new Vector3(1, 2, 3));

        behavior.Update();

        Assert.Equal(Vector3.Zero, behavior.Acceleration);
        Assert.Equal(new Vector3(1, 2, 3), behavior.Velocity);
        Assert.Equal(new Vector3(1, 2, 3), gameObject.Translation);
    }

    private PhysicsBehavior SampleModule(PhysicsBehaviorModuleData moduleData = null)
    {
        return SampleModule(out _, moduleData);
    }

    private PhysicsBehavior SampleModule(out GameObject gameObject, PhysicsBehaviorModuleData moduleData = null)
    {
        // TODO: Simplify test setup.

        var objectDefinition = new ObjectDefinition();
        gameObject = new GameObject(objectDefinition, ZeroHour.Context, null);

        return SampleModule(ZeroHour, moduleData, gameObject: gameObject);
    }
}
