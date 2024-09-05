using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Tests.Logic.Object.Update;
using Xunit;

namespace OpenSage.Tests.Logic.Object.Behaviors;

public class ParkingPlaceBehaviorTests : UpdateModuleTest<ParkingPlaceBehaviour, ParkingPlaceBehaviorModuleData>
{
    #region Persister tests

    /// <summary>
    /// An empty airfield in a 2x2 configuration has a length-4 array of empty parking spots, a length-2 array of empty runway assignments, no healing data, and a healing frame value of 0xffffff3f
    /// </summary>
    private static readonly byte[] AmericaEmptyAirfield =
    [
        0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xff, 0xff, 0xff, 0x3f,
    ];

    [Fact]
    public void Generals_Airfield_Empty()
    {
        var stream = SaveData(AmericaEmptyAirfield, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2,
            NumCols = 2,
            HasRunways = true,
        });
        behavior.Load(reader);

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);
        Assert.Equal(2, behavior.RunwayAssignments.Length);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);
        Assert.Empty(behavior.HealingData);
        Assert.Null(behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
    }

    /// <summary>
    /// An airfield with one unit in queue has the first parking slot reserved.
    /// </summary>
    private static readonly byte[] AmericaAirfieldBuilding1 =
    [
        0x04, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xff, 0xff, 0xff, 0x3f,
    ];

    [Fact]
    public void Generals_Airfield_Building1()
    {
        var stream = SaveData(AmericaAirfieldBuilding1, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2, NumCols = 2, HasRunways = true,
        });
        behavior.Load(reader);

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.UnderConstruction, behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);
        Assert.Equal(2, behavior.RunwayAssignments.Length);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);
        Assert.Empty(behavior.HealingData);
        Assert.Null(behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
    }

    /// <summary>
    /// An airfield with four units in queue has all parking slots reserved.
    /// </summary>
    private static readonly byte[] AmericaAirfieldBuilding4 =
    [
        0x04, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xff, 0xff, 0xff, 0x3f,
    ];

    [Fact]
    public void Generals_Airfield_Building4()
    {
        var stream = SaveData(AmericaAirfieldBuilding4, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2,
            NumCols = 2,
            HasRunways = true,
        });
        behavior.Load(reader);

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.UnderConstruction, behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.UnderConstruction, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.UnderConstruction, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.UnderConstruction, behavior.ParkingSlots[3]);
        Assert.Equal(2, behavior.RunwayAssignments.Length);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);
        Assert.Empty(behavior.HealingData);
        Assert.Null(behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
    }

    /// <summary>
    /// An airfield with one unit built but not yet idle in the parking spot has the aircraft's object id in the slot, but no healing data.
    /// </summary>
    private static readonly byte[] AmericaAirfieldBuilt1 =
    [
        0x04, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xff, 0xff, 0xff, 0x3f,
    ];

    [Fact]
    public void Generals_Airfield_Built1()
    {
        var stream = SaveData(AmericaAirfieldBuilt1, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2,
            NumCols = 2,
            HasRunways = true,
        });

        behavior.Load(reader);

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(13), behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);
        Assert.Equal(2, behavior.RunwayAssignments.Length);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);
        Assert.Empty(behavior.HealingData);
        Assert.Null(behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
    }

    /// <summary>
    /// An airfield with one unit built and idle has a parking spot reserved, healing data for the unit, and a final healing frame that is not static.
    /// </summary>
    private static readonly byte[] AmericaAirfield1Idle =
    [
        0x04, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x01, 0x0d, 0x00, 0x00, 0x00, 0xa9, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xbb, 0x18, 0x00, 0x00,
    ];

    [Fact]
    public void Generals_Airfield_1Idle()
    {
        var stream = SaveData(AmericaAirfield1Idle, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2,
            NumCols = 2,
            HasRunways = true,
        });

        behavior.Load(reader);

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(13), behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);
        Assert.Equal(2, behavior.RunwayAssignments.Length);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);
        Assert.Single(behavior.HealingData);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingPlaceHealingData(13, new LogicFrame(6313)), behavior.HealingData[0]);
        Assert.Null(behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(6331), behavior.NextHealFrame);
    }

    /// <summary>
    /// An airfield with one unit leaving has no healing data, an aircraft assigned to the runway, and a landing slot still reserved.
    /// </summary>
    /// <remarks>
    /// This is also true for the same aircraft coming back in to land.
    /// </remarks>
    private static readonly byte[] AmericaAirfield1Leaving =
    [
        0x04, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x02, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xff, 0xff, 0xff, 0x3f,
    ];

    [Fact]
    public void Generals_Airfield_1Leaving()
    {
        var stream = SaveData(AmericaAirfield1Leaving, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2,
            NumCols = 2,
            HasRunways = true,
        });

        behavior.Load(reader);

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(13), behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);
        Assert.Equal(2, behavior.RunwayAssignments.Length);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(13), behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);
        Assert.Empty(behavior.HealingData);
        Assert.Null(behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
    }

    /// <summary>
    /// An airfield with two units leaving has no healing data, an aircraft assigned to each runway, and landing slots still reserved.
    /// </summary>
    /// <remarks>
    /// This is also true for the same aircraft coming back in to land.
    /// </remarks>
    private static readonly byte[] AmericaAirfield2Leaving =
    [
        0x04, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x02, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xff, 0xff, 0xff, 0x3f,
    ];

    [Fact]
    public void Generals_Airfield_2Leaving()
    {
        var stream = SaveData(AmericaAirfield2Leaving, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2,
            NumCols = 2,
            HasRunways = true,
        });

        behavior.Load(reader);

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(13), behavior.ParkingSlots[0]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(14), behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);
        Assert.Equal(2, behavior.RunwayAssignments.Length);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(13), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(14), behavior.RunwayAssignments[1]);
        Assert.Empty(behavior.HealingData);
        Assert.Null(behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
    }

    /// <summary>
    /// An airfield with four units leaving has no healing data, two aircraft assigned to each runway, and landing slots still reserved.
    /// </summary>
    private static readonly byte[] AmericaAirfield4Leaving =
    [
        0x04, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00,
        0x00, 0x00, 0x00, 0x02, 0x0f, 0x00, 0x00, 0x00, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x00, 0x00, 0x10,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xff, 0xff, 0xff, 0x3f,
    ];

    [Fact]
    public void Generals_Airfield_4Leaving()
    {
        var stream = SaveData(AmericaAirfield4Leaving, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2,
            NumCols = 2,
            HasRunways = true,
        });

        behavior.Load(reader);

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(13), behavior.ParkingSlots[0]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(14), behavior.ParkingSlots[1]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(15), behavior.ParkingSlots[2]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(16), behavior.ParkingSlots[3]);
        Assert.Equal(2, behavior.RunwayAssignments.Length);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(15, 13), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(14, 16), behavior.RunwayAssignments[1]);
        Assert.Empty(behavior.HealingData);
        Assert.Null(behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
    }

    /// <summary>
    /// An airfield with four units leaving, two of which are taking off and two of which are taxiing to active runways, has
    /// a corresponding boolean set on RunwayAssignment for the aircraft taxiing onto an active runway.
    /// </summary>
    private static readonly byte[] AmericaAirfield4Leaving2Taxi2Takeoff =
    [
        0x04, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x00, 0x00, 0x00, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00,
        0x00, 0x00, 0x00, 0x02, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x0e, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xff, 0xff, 0xff, 0x3f,
    ];

    [Fact]
    public void Generals_Airfield_4Leaving_2Taxi2Takeoff()
    {
        var stream = SaveData(AmericaAirfield4Leaving2Taxi2Takeoff, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2,
            NumCols = 2,
            HasRunways = true,
        });

        behavior.Load(reader);

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(13), behavior.ParkingSlots[0]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(14), behavior.ParkingSlots[1]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(15), behavior.ParkingSlots[2]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(16), behavior.ParkingSlots[3]);
        Assert.Equal(2, behavior.RunwayAssignments.Length);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(15, runwayActive: true), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(14, runwayActive: true), behavior.RunwayAssignments[1]);
        Assert.Empty(behavior.HealingData);
        Assert.Null(behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
    }

    /// <summary>
    /// An airfield with one unit built and idle has a parking spot reserved, healing data for the unit, and a final healing frame that is not static.
    /// </summary>
    private static readonly byte[] AmericaAirfield1Departed =
    [
        0x04, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xff, 0xff, 0xff, 0x3f,
    ];

    [Fact]
    public void Generals_Airfield_1Departed()
    {
        var stream = SaveData(AmericaAirfield1Departed, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2,
            NumCols = 2,
            HasRunways = true,
        });

        behavior.Load(reader);

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(13), behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);
        Assert.Equal(2, behavior.RunwayAssignments.Length);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);
        Assert.Empty(behavior.HealingData);
        Assert.Null(behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
    }

    /// <summary>
    /// An airfield with 2 rows and 1 column only has 2 parking slots and 1 runway.
    /// </summary>
    private static readonly byte[] AmericaAirfield1Runway =
    [
        0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff,
        0xff, 0xff, 0x3f,
    ];

    [Fact]
    public void Generals_Airfield_1Runway()
    {
        var stream = SaveData(AmericaAirfield1Runway, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2,
            NumCols = 1,
            HasRunways = true,
        });

        behavior.Load(reader);

        Assert.Equal(2, behavior.ParkingSlots.Length);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(1, behavior.RunwayAssignments.Length);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Empty(behavior.HealingData);
        Assert.Null(behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
    }

    /// <summary>
    /// An airfield with 2 rows and 2 columns but HasRunways = No has no runway assignments.
    /// </summary>
    private static readonly byte[] AmericaAirfieldNoRunways =
    [
        0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xff, 0xff, 0xff, 0x3f,
    ];

    [Fact]
    public void Generals_Airfield_NoRunways()
    {
        var stream = SaveData(AmericaAirfieldNoRunways, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2,
            NumCols = 2,
            HasRunways = false,
        });

        behavior.Load(reader);

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);
        Assert.Equal(0, behavior.RunwayAssignments.Length);
        Assert.Empty(behavior.HealingData);
        Assert.Null(behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
    }

    // 0x0, hasrunways = true
    // 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0x3f


    /// <summary>
    /// An airfield with no parking slots has no parking and no runways.
    /// </summary>
    /// <remarks>
    /// This is true regardless of HasRunways.
    /// </remarks>
    private static readonly byte[] AmericaAirfieldNoParkingNoRunways =
    [
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff,
        0xff, 0x3f,
    ];

    [Fact]
    public void Generals_Airfield_NoParkingNoRunways()
    {
        var stream = SaveData(AmericaAirfieldNoParkingNoRunways, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 0,
            NumCols = 0,
            HasRunways = true,
        });

        behavior.Load(reader);

        Assert.Equal(0, behavior.ParkingSlots.Length);
        Assert.Equal(0, behavior.RunwayAssignments.Length);
        Assert.Empty(behavior.HealingData);
        Assert.Null(behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
    }

    /// <summary>
    /// An airfield with only a rally point set has a rally point available in the RallyPointManager.
    /// </summary>
    private static readonly byte[] AmericaRallyPointAirfield =
    [
        0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x3d, 0xda, 0x55, 0x44, 0x92, 0x3c, 0x39, 0x44, 0x00, 0x00, 0x20, 0x41, 0x01,
        0xff, 0xff, 0xff, 0x3f,
    ];

    [Fact]
    public void Generals_Airfield_RallyPoint()
    {
        var stream = SaveData(AmericaRallyPointAirfield, V3);
        var reader = new StateReader(stream, Generals);
        var behavior = SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2,
            NumCols = 2,
            HasRunways = true,
        });
        behavior.Load(reader);

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);
        Assert.Equal(2, behavior.RunwayAssignments.Length);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);
        Assert.Empty(behavior.HealingData);
        Assert.Equal(new Vector3(855.40997f, 740.94641f, 10), behavior.RallyPointManager.RallyPoint);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
    }

    #endregion

    private ParkingPlaceBehaviour SampleBehavior()
    {
        return SampleModule(moduleData: new ParkingPlaceBehaviorModuleData
        {
            NumRows = 2,
            NumCols = 2,
            HasRunways = true,
        });
    }

    [Fact]
    public void ParkingPlaceBehavior_EnqueueUnit()
    {
        var behavior = SampleBehavior();

        behavior.EnqueueObject();

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.UnderConstruction, behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);
    }

    [Fact]
    public void ParkingPlaceBehavior_CancelQueuedUnit()
    {
        var behavior = SampleBehavior();

        behavior.EnqueueObject();
        behavior.CancelQueuedObject();

        Assert.Equal(4, behavior.ParkingSlots.Length);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);
    }


    [Fact]
    public void ParkingPlaceBehavior_OccupySlots()
    {
        var behavior = SampleBehavior();

        Assert.True(behavior.HasFreeSlots());
        behavior.EnqueueObject();
        Assert.True(behavior.HasFreeSlots());
        behavior.EnqueueObject();
        Assert.True(behavior.HasFreeSlots());
        behavior.EnqueueObject();
        Assert.True(behavior.HasFreeSlots());
        behavior.EnqueueObject();
        Assert.False(behavior.HasFreeSlots());
    }

    [Fact]
    public void ParkingPlaceBehavior_ClearProducedVehicle_InParkingSpot()
    {
        var behavior = SampleBehavior();

        const uint vehicleId = 1;

        behavior.EnqueueObject();
        behavior.ReportSpawn(vehicleId);

        behavior.ClearObjectFromSlot(vehicleId);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);

        Assert.Empty(behavior.HealingData);
    }

    [Fact]
    public void ParkingPlaceBehavior_ClearProducedVehicle_OnDeparture()
    {
        var behavior = SampleBehavior();

        const uint vehicleId = 1;

        behavior.EnqueueObject();
        behavior.ReportSpawn(vehicleId);
        behavior.ReportReadyToTaxi(vehicleId, out _);

        behavior.ClearObjectFromSlot(vehicleId);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);

        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);
    }
    [Fact]
    public void ParkingPlaceBehavior_ClearProducedVehicle_Away()
    {
        var behavior = SampleBehavior();

        const uint vehicleId = 1;

        behavior.EnqueueObject();
        behavior.ReportSpawn(vehicleId);
        behavior.ReportReadyToTaxi(vehicleId, out _);
        behavior.ReportEngineRunUp(vehicleId);
        behavior.ReportDeparted(vehicleId);

        behavior.ClearObjectFromSlot(vehicleId);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);
    }

    [Fact]
    public void ParkingPlaceBehavior_ReportSpawn()
    {
        var behavior = SampleBehavior();

        const uint vehicleId1 = 1;
        const uint vehicleId2 = 2;
        const uint vehicleId3 = 3;
        const uint vehicleId4 = 4;

        // no objects are enqueued
        Assert.Throws<InvalidStateException>(() => behavior.ReportSpawn(0));

        behavior.EnqueueObject();
        behavior.EnqueueObject();

        Assert.Equal(0, behavior.ReportSpawn(vehicleId1));
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId1), behavior.ParkingSlots[0]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.UnderConstruction, behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);

        Assert.Equal(1, behavior.ReportSpawn(vehicleId2));
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId1), behavior.ParkingSlots[0]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId2), behavior.ParkingSlots[1]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);

        behavior.EnqueueObject();
        behavior.EnqueueObject();

        Assert.Equal(2, behavior.ReportSpawn(vehicleId3));
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId1), behavior.ParkingSlots[0]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId2), behavior.ParkingSlots[1]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId3), behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.UnderConstruction, behavior.ParkingSlots[3]);

        Assert.Equal(3, behavior.ReportSpawn(vehicleId4));
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId1), behavior.ParkingSlots[0]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId2), behavior.ParkingSlots[1]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId3), behavior.ParkingSlots[2]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId4), behavior.ParkingSlots[3]);

        Assert.Empty(behavior.HealingData);

        Assert.Throws<InvalidStateException>(behavior.EnqueueObject);
    }

    [Fact]
    public void ParkingPlaceBehavior_ReportParkedIdle()
    {
        var behavior = SampleBehavior();

        const uint vehicleId1 = 1;
        const uint vehicleId2 = 2;
        var logicFrame = new LogicFrame(42);

        behavior.EnqueueObject();
        behavior.EnqueueObject();

        behavior.ReportSpawn(vehicleId1);
        behavior.ReportSpawn(vehicleId2);

        Assert.Empty(behavior.HealingData);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);
        behavior.ReportParkedIdle(vehicleId1, logicFrame);

        Assert.Single(behavior.HealingData);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingPlaceHealingData(vehicleId1, logicFrame), behavior.HealingData[0]);
        Assert.Equal(logicFrame, behavior.NextHealFrame);

        var logicFrame2 = logicFrame + new LogicFrameSpan(1);
        behavior.ReportParkedIdle(vehicleId2, logicFrame2);
        Assert.Equal(2, behavior.HealingData.Count);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingPlaceHealingData(vehicleId2, logicFrame2), behavior.HealingData[1]);
        Assert.Equal(logicFrame, behavior.NextHealFrame);
    }

    [Fact]
    public void ParkingPlaceBehavior_ReportReadyToTaxi()
    {
        var behavior = SampleBehavior();

        const uint vehicleId1 = 1;
        const uint vehicleId2 = 2;
        const uint vehicleId3 = 3;
        const uint vehicleId4 = 4;

        behavior.EnqueueObject();
        behavior.EnqueueObject();
        behavior.EnqueueObject();
        behavior.EnqueueObject();

        behavior.ReportSpawn(vehicleId1);
        behavior.ReportSpawn(vehicleId2);
        behavior.ReportSpawn(vehicleId3);
        behavior.ReportSpawn(vehicleId4);

        var logicFrame = new LogicFrame(42);
        behavior.ReportParkedIdle(vehicleId1, logicFrame);
        behavior.ReportParkedIdle(vehicleId2, logicFrame);
        behavior.ReportParkedIdle(vehicleId3, logicFrame);
        behavior.ReportParkedIdle(vehicleId4, logicFrame);

        Assert.True(behavior.ReportReadyToTaxi(vehicleId1, out var v1Runway));
        Assert.Equal(0, v1Runway);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId1), behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);

        Assert.True(behavior.ReportReadyToTaxi(vehicleId2, out var v2Runway));
        Assert.Equal(1, v2Runway);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId1), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId2), behavior.RunwayAssignments[1]);

        Assert.False(behavior.ReportReadyToTaxi(vehicleId3, out var v3Runway));
        Assert.Equal(0, v3Runway);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId1, vehicleId3), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId2), behavior.RunwayAssignments[1]);

        Assert.False(behavior.ReportReadyToTaxi(vehicleId4, out var v4Runway));
        Assert.Equal(1, v4Runway);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId1, vehicleId3), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId2, vehicleId4), behavior.RunwayAssignments[1]);

        Assert.Empty(behavior.HealingData);
        Assert.Equal(new LogicFrame(0x3FFFFFFFu), behavior.NextHealFrame);

        // ensure all slots are still reserved
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId1), behavior.ParkingSlots[0]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId2), behavior.ParkingSlots[1]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId3), behavior.ParkingSlots[2]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId4), behavior.ParkingSlots[3]);
    }

    [Fact]
    public void ParkingPlaceBehavior_ReportEngineRunUp()
    {
        var behavior = SampleBehavior();

        const uint vehicleId1 = 1;
        const uint vehicleId2 = 2;
        const uint vehicleId3 = 3;
        const uint vehicleId4 = 4;

        behavior.EnqueueObject();
        behavior.EnqueueObject();
        behavior.EnqueueObject();
        behavior.EnqueueObject();

        behavior.ReportSpawn(vehicleId1);
        behavior.ReportSpawn(vehicleId2);
        behavior.ReportSpawn(vehicleId3);
        behavior.ReportSpawn(vehicleId4);

        var logicFrame = new LogicFrame(42);
        behavior.ReportParkedIdle(vehicleId1, logicFrame);
        behavior.ReportParkedIdle(vehicleId2, logicFrame);
        behavior.ReportParkedIdle(vehicleId3, logicFrame);
        behavior.ReportParkedIdle(vehicleId4, logicFrame);

        behavior.ReportReadyToTaxi(vehicleId1, out _);
        behavior.ReportReadyToTaxi(vehicleId2, out _);
        behavior.ReportReadyToTaxi(vehicleId3, out _);
        behavior.ReportReadyToTaxi(vehicleId4, out _);

        behavior.ReportEngineRunUp(vehicleId1);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId3, runwayActive: true), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId2, vehicleId4), behavior.RunwayAssignments[1]);

        behavior.ReportEngineRunUp(vehicleId2);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId3, runwayActive: true), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId4, runwayActive: true), behavior.RunwayAssignments[1]);

        behavior.ReportEngineRunUp(vehicleId3);
        behavior.ReportEngineRunUp(vehicleId4);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId3, runwayActive: true), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId4, runwayActive: true), behavior.RunwayAssignments[1]);
    }

    [Fact]
    public void ParkingPlaceBehavior_Report2Departed()
    {
        var behavior = SampleBehavior();

        const uint vehicleId1 = 1;
        const uint vehicleId2 = 2;

        behavior.EnqueueObject();
        behavior.EnqueueObject();

        behavior.ReportSpawn(vehicleId1);
        behavior.ReportSpawn(vehicleId2);

        var logicFrame = new LogicFrame(42);
        behavior.ReportParkedIdle(vehicleId1, logicFrame);
        behavior.ReportParkedIdle(vehicleId2, logicFrame);

        behavior.ReportReadyToTaxi(vehicleId1, out _);
        behavior.ReportReadyToTaxi(vehicleId2, out _);

        behavior.ReportEngineRunUp(vehicleId1);
        behavior.ReportEngineRunUp(vehicleId2);

        behavior.ReportDeparted(vehicleId1);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId2), behavior.RunwayAssignments[1]);

        behavior.ReportDeparted(vehicleId2);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);
    }

    [Fact]
    public void ParkingPlaceBehavior_Report4Departed()
    {
        var behavior = SampleBehavior();

        const uint vehicleId1 = 1;
        const uint vehicleId2 = 2;
        const uint vehicleId3 = 3;
        const uint vehicleId4 = 4;

        behavior.EnqueueObject();
        behavior.EnqueueObject();
        behavior.EnqueueObject();
        behavior.EnqueueObject();

        behavior.ReportSpawn(vehicleId1);
        behavior.ReportSpawn(vehicleId2);
        behavior.ReportSpawn(vehicleId3);
        behavior.ReportSpawn(vehicleId4);

        var logicFrame = new LogicFrame(42);
        behavior.ReportParkedIdle(vehicleId1, logicFrame);
        behavior.ReportParkedIdle(vehicleId2, logicFrame);
        behavior.ReportParkedIdle(vehicleId3, logicFrame);
        behavior.ReportParkedIdle(vehicleId4, logicFrame);

        behavior.ReportReadyToTaxi(vehicleId1, out _);
        behavior.ReportReadyToTaxi(vehicleId2, out _);
        behavior.ReportReadyToTaxi(vehicleId3, out _);
        behavior.ReportReadyToTaxi(vehicleId4, out _);

        behavior.ReportEngineRunUp(vehicleId1);
        behavior.ReportEngineRunUp(vehicleId2);
        behavior.ReportEngineRunUp(vehicleId3);
        behavior.ReportEngineRunUp(vehicleId4);

        behavior.ReportDeparted(vehicleId1);
        behavior.ReportDeparted(vehicleId2);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId3, runwayActive: true), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId4, runwayActive: true), behavior.RunwayAssignments[1]);

        behavior.ReportDeparted(vehicleId3);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId4, runwayActive: true), behavior.RunwayAssignments[1]);

        behavior.ReportDeparted(vehicleId4);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);
    }

    [Fact]
    public void ParkingPlaceBehavior_ReportInbound()
    {
        var behavior = SampleBehavior();

        const uint vehicleId1 = 1;
        const uint vehicleId2 = 2;
        const uint vehicleId3 = 3;

        behavior.EnqueueObject();
        behavior.EnqueueObject();

        behavior.ReportSpawn(vehicleId1);
        behavior.ReportSpawn(vehicleId2);

        var logicFrame = new LogicFrame(42);
        behavior.ReportParkedIdle(vehicleId1, logicFrame);
        behavior.ReportParkedIdle(vehicleId2, logicFrame);

        behavior.ReportReadyToTaxi(vehicleId1, out _);
        behavior.ReportReadyToTaxi(vehicleId2, out _);

        behavior.ReportEngineRunUp(vehicleId1);
        behavior.ReportEngineRunUp(vehicleId2);

        behavior.ReportDeparted(vehicleId1);
        behavior.ReportDeparted(vehicleId2);

        // a new spot should be assigned to this aircraft
        Assert.Equal(2, behavior.ReportInbound(vehicleId3));
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId1), behavior.ParkingSlots[0]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId2), behavior.ParkingSlots[1]);
        Assert.Equal(new ParkingPlaceBehaviour.ParkingSlot(vehicleId3), behavior.ParkingSlots[2]);
        Assert.Equal(ParkingPlaceBehaviour.ParkingSlot.Empty, behavior.ParkingSlots[3]);

        // these aircraft already have slots
        Assert.Equal(0, behavior.ReportInbound(vehicleId1));
        Assert.Equal(1, behavior.ReportInbound(vehicleId2));

        behavior.EnqueueObject();

        // now that we've enqueued another object, this airfield is full, so we can't take any more aircraft
        Assert.Throws<InvalidStateException>(() => behavior.ReportInbound(99));
    }

    [Fact]
    public void ParkingPlaceBehavior_ReportLanding()
    {
        var behavior = SampleBehavior();

        const uint vehicleId1 = 1;
        const uint vehicleId2 = 2;
        const uint vehicleId3 = 3;
        const uint vehicleId4 = 4;

        behavior.EnqueueObject();
        behavior.EnqueueObject();
        behavior.EnqueueObject();
        behavior.EnqueueObject();

        behavior.ReportSpawn(vehicleId1);
        behavior.ReportSpawn(vehicleId2);
        behavior.ReportSpawn(vehicleId3);
        behavior.ReportSpawn(vehicleId4);

        var logicFrame = new LogicFrame(42);
        behavior.ReportParkedIdle(vehicleId1, logicFrame);
        behavior.ReportParkedIdle(vehicleId2, logicFrame);
        behavior.ReportParkedIdle(vehicleId3, logicFrame);
        behavior.ReportParkedIdle(vehicleId4, logicFrame);

        behavior.ReportReadyToTaxi(vehicleId1, out _);
        behavior.ReportReadyToTaxi(vehicleId2, out _);
        behavior.ReportReadyToTaxi(vehicleId3, out _);

        behavior.ReportEngineRunUp(vehicleId1);
        behavior.ReportEngineRunUp(vehicleId2);
        behavior.ReportEngineRunUp(vehicleId3);

        behavior.ReportDeparted(vehicleId1);
        behavior.ReportDeparted(vehicleId2);
        behavior.ReportDeparted(vehicleId3);

        behavior.ReportInbound(vehicleId1);
        behavior.ReportInbound(vehicleId2);
        behavior.ReportInbound(vehicleId3);

        Assert.Equal(0, behavior.ReportLanding(vehicleId1));
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId1), behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);

        Assert.Equal(1, behavior.ReportLanding(vehicleId2));
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId1), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId2), behavior.RunwayAssignments[1]);

        // landing clearances are never double-stacked
        Assert.Equal(-1, behavior.ReportLanding(vehicleId3));

        // however, aircraft departing can stack with aircraft landing
        Assert.False(behavior.ReportReadyToTaxi(vehicleId4, out var runway));
        Assert.Equal(0, runway);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId1, vehicleId4), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId2), behavior.RunwayAssignments[1]);
    }

    [Fact]
    public void ParkingPlaceBehavior_ReportLanded()
    {
        var behavior = SampleBehavior();

        const uint vehicleId1 = 1;
        const uint vehicleId2 = 2;
        const uint vehicleId3 = 3;
        const uint vehicleId4 = 4;

        behavior.EnqueueObject();
        behavior.ReportSpawn(vehicleId1);

        var logicFrame = new LogicFrame(42);
        behavior.ReportParkedIdle(vehicleId1, logicFrame);

        behavior.ReportInbound(vehicleId2);
        behavior.ReportInbound(vehicleId3);

        behavior.ReportLanding(vehicleId2);
        behavior.ReportLanding(vehicleId3);

        Assert.Equal((0, 1), behavior.ReportLanded(vehicleId2));

        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId3), behavior.RunwayAssignments[1]);

        behavior.ReportInbound(vehicleId4);
        behavior.ReportLanding(vehicleId4);

        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId4), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId3), behavior.RunwayAssignments[1]);

        Assert.False(behavior.ReportReadyToTaxi(vehicleId1, out _));

        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId4, vehicleId1), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId3), behavior.RunwayAssignments[1]);

        behavior.ReportLanded(vehicleId4);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId1, runwayActive: true), behavior.RunwayAssignments[0]);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId3), behavior.RunwayAssignments[1]);

        behavior.ReportLanded(vehicleId3);
        Assert.Equal(new ParkingPlaceBehaviour.RunwayAssignment(vehicleId1, runwayActive: true), behavior.RunwayAssignments[0]);
        Assert.Equal(ParkingPlaceBehaviour.RunwayAssignment.Empty, behavior.RunwayAssignments[1]);
    }
}
