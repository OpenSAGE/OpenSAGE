using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using Xunit;

namespace OpenSage.Tests;

public class RadarTests : StatePersisterTest
{
    #region RadarItem Tests

    /// <summary>
    /// Radar item for the red player in Generals.
    /// </summary>
    private static readonly byte[] GeneralsRedRadarItem = [0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff];

    [Fact]
    public void RadarItem_Red_V1()
    {
        var stream = SaveData(GeneralsRedRadarItem);
        var reader = new StateReader(stream, Generals);
        var data = new RadarItem();
        data.Persist(reader);

        Assert.Equal(7u, data.ObjectId);
        Assert.Equal(new ColorRgba(0xff, 0, 0, 0xff), data.Color);
    }

    /// <summary>
    /// Radar item for the green player in Generals.
    /// </summary>
    private static readonly byte[] GeneralsGreenRadarItem = [0x07, 0x00, 0x00, 0x00, 0x2e, 0xd1, 0x3e, 0xff];

    [Fact]
    public void RadarItem_Green_V1()
    {
        var stream = SaveData(GeneralsGreenRadarItem);
        var reader = new StateReader(stream, Generals);
        var data = new RadarItem();
        data.Persist(reader);

        Assert.Equal(7u, data.ObjectId);
        Assert.Equal(new ColorRgba(0x3e, 0xd1, 0x2e, 0xff), data.Color);
    }

    /// <summary>
    /// Radar item for the blue player in Generals.
    /// </summary>
    private static readonly byte[] GeneralsBlueRadarItem = [0x07, 0x00, 0x00, 0x00, 0xfe, 0x68, 0x43, 0xff];

    [Fact]
    public void RadarItem_Blue_V1()
    {
        var stream = SaveData(GeneralsBlueRadarItem);
        var reader = new StateReader(stream, Generals);
        var data = new RadarItem();
        data.Persist(reader);

        Assert.Equal(7u, data.ObjectId);
        Assert.Equal(new ColorRgba(0x43, 0x68, 0xfe, 0xff), data.Color);
    }

    #endregion

    #region RadarItemCollection Tests

    /// <summary>
    /// Radar item collection for the blue player in Generals containing two items.
    /// </summary>
    private static readonly byte[] GeneralsBlueRadarItemCollection = [0x02, 0x00, 0x01, 0x07, 0x00, 0x00, 0x00, 0xfe, 0x68, 0x43, 0xff, 0x01, 0x08, 0x00, 0x00, 0x00, 0xfe, 0x68, 0x43, 0xff];

    [Fact]
    public void RadarItemCollection_Blue_V1()
    {
        var stream = SaveData(GeneralsBlueRadarItemCollection);
        var reader = new StateReader(stream, Generals);
        var data = new RadarItemCollection();
        data.Persist(reader);

        Assert.True(data.TryGetValue(7u, out var object7));
        Assert.Equal(7u, object7.ObjectId);
        Assert.Equal(new ColorRgba(0x43, 0x68, 0xfe, 0xff), object7.Color);

        Assert.True(data.TryGetValue(8u, out var object8));
        Assert.Equal(8u, object8.ObjectId);
        Assert.Equal(new ColorRgba(0x43, 0x68, 0xfe, 0xff), object8.Color);
    }

    #endregion

    #region RadarEvent Tests

    // UnderAttack = 3,
    // Information = 4,
    // EnemyInfiltrationDetected = 6, // building captured
    // BattlePlanInitiated = 7, // strategy center battle plan changed
    // StealthUnitDiscovered = 8, // discovered enemy stealth unit
    // StealthUnitNeutralized = 9, // your stealth unit has been discovered
    // UnitLost = 10,


    /// <summary>
    /// Default radar event in Generals.
    /// </summary>
    private static readonly byte[] GeneralsInvalidRadarEvent = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void RadarEvent_Invalid()
    {
        var stream = SaveDataNoVersion(GeneralsInvalidRadarEvent);
        var reader = new StateReader(stream, Generals);
        var data = new RadarEvent();
        data.Persist(reader);

        Assert.Equal(RadarEventType.Invalid, data.Type);
        Assert.False(data.ActivelyAnimating);
        Assert.Equal(LogicFrame.Zero, data.StartFrame);
        Assert.Equal(LogicFrame.Zero, data.EndFrame);
        Assert.Equal(LogicFrame.Zero, data.FadeBeginFrame);
        Assert.Equal(new ColorRgba(0, 0, 0, 0), data.Color1);
        Assert.Equal(new ColorRgba(0, 0, 0, 0), data.Color2);
        Assert.Equal(Vector3.Zero, data.Position);
        Assert.Equal(0u, data.MapTileXCoordinate);
        Assert.Equal(0u, data.MapTileYCoordinate);
        Assert.False(data.Visible);
    }

    /// <summary>
    /// Construction radar event in Generals, saved while the event is still playing for an object roughly constructed in the top-middle of the map with no radar upgrade.
    /// </summary>
    private static readonly byte[] GeneralsConstructionRadarEvent = [0x01, 0x00, 0x00, 0x00, 0x01, 0x88, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0xf1, 0x03, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x86, 0xa3, 0xda, 0x43, 0x15, 0x64, 0x55, 0x44, 0x00, 0x00, 0x20, 0x41, 0x37, 0x00, 0x00, 0x00, 0x6d, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void RadarEvent_Construction()
    {
        var stream = SaveDataNoVersion(GeneralsConstructionRadarEvent);
        var reader = new StateReader(stream, Generals);
        var data = new RadarEvent();
        data.Persist(reader);

        Assert.Equal(RadarEventType.Construction, data.Type);
        Assert.True(data.ActivelyAnimating);
        Assert.Equal(new LogicFrame(904), data.StartFrame);
        Assert.Equal(new LogicFrame(1024), data.EndFrame);
        Assert.Equal(new LogicFrame(1009), data.FadeBeginFrame);
        Assert.Equal(new ColorRgba(128, 128, 255, 255), data.Color1);
        Assert.Equal(new ColorRgba(128, 255, 255, 255), data.Color2);
        Assert.Equal(new Vector3(437.27753f, 853.56378f, 10), data.Position);
        Assert.Equal(55u, data.MapTileXCoordinate);
        Assert.Equal(109u, data.MapTileYCoordinate);
        Assert.False(data.Visible);
    }

    /// <summary>
    /// Radar Upgrade radar event in Generals, saved after the event is finished playing.
    /// </summary>
    private static readonly byte[] GeneralsUpgradeRadarEvent = [0x02, 0x00, 0x00, 0x00, 0x00, 0x60, 0x06, 0x00, 0x00, 0xd8, 0x06, 0x00, 0x00, 0xc9, 0x06, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0xb9, 0x00, 0x00, 0x00, 0xdc, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x3a, 0x2e, 0x0d, 0x44, 0x29, 0x47, 0x4f, 0x44, 0x00, 0x00, 0x20, 0x41, 0x48, 0x00, 0x00, 0x00, 0x6a, 0x00, 0x00, 0x00, 0x01];

    [Fact]
    public void RadarEvent_Upgrade()
    {
        var stream = SaveDataNoVersion(GeneralsUpgradeRadarEvent);
        var reader = new StateReader(stream, Generals);
        var data = new RadarEvent();
        data.Persist(reader);

        Assert.Equal(RadarEventType.Upgrade, data.Type);
        Assert.False(data.ActivelyAnimating);
        Assert.Equal(new LogicFrame(1632), data.StartFrame);
        Assert.Equal(new LogicFrame(1752), data.EndFrame);
        Assert.Equal(new LogicFrame(1737), data.FadeBeginFrame);
        Assert.Equal(new ColorRgba(128, 0, 64, 255), data.Color1);
        Assert.Equal(new ColorRgba(255, 185, 220, 255), data.Color2);
        Assert.Equal(new Vector3(564.72229f, 829.11188f, 10), data.Position);
        Assert.Equal(72u, data.MapTileXCoordinate);
        Assert.Equal(106u, data.MapTileYCoordinate);
        Assert.True(data.Visible);
    }


    /// <summary>
    /// Unit Lost radar event in Generals, which does not appear on the radar due to having no color.
    /// </summary>
    private static readonly byte[] GeneralsUnitLostRadarEvent = [0x0a, 0x00, 0x00, 0x00, 0x01, 0xf3, 0x07, 0x00, 0x00, 0x6b, 0x08, 0x00, 0x00, 0x5c, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1e, 0xac, 0xfa, 0x43, 0xef, 0xa3, 0x31, 0x44, 0x00, 0x00, 0x20, 0x41, 0x40, 0x00, 0x00, 0x00, 0x5a, 0x00, 0x00, 0x00, 0x00];

    [Fact]
    public void RadarEvent_UnitLost()
    {
        var stream = SaveDataNoVersion(GeneralsUnitLostRadarEvent);
        var reader = new StateReader(stream, Generals);
        var data = new RadarEvent();
        data.Persist(reader);

        Assert.Equal(RadarEventType.UnitLost, data.Type);
        Assert.True(data.ActivelyAnimating);
        Assert.Equal(new LogicFrame(2035), data.StartFrame);
        Assert.Equal(new LogicFrame(2155), data.EndFrame);
        Assert.Equal(new LogicFrame(2140), data.FadeBeginFrame);
        Assert.Equal(new ColorRgba(0, 0, 0, 0), data.Color1);
        Assert.Equal(new ColorRgba(0, 0, 0, 0), data.Color2);
        Assert.Equal(new Vector3(501.34467f, 710.56146f, 10), data.Position);
        Assert.Equal(64u, data.MapTileXCoordinate);
        Assert.Equal(90u, data.MapTileYCoordinate);
        Assert.False(data.Visible);
    }

    #endregion

    #region Radar tests

    [Fact]
    public void Radar_Create()
    {
        var radar = new Radar();

        Assert.Empty(radar.VisibleItems);
        Assert.Empty(radar.HiddenItems);
        Assert.Equal(64, radar.RadarEvents.Length);
    }

    [Fact]
    public void Radar_AddNonRadarObject()
    {
        var radar = new Radar();

        var objectDefinition = new ObjectDefinition
        {
            RadarPriority = RadarPriority.NotOnRadar,
        };
        var gameObject = new GameObject(objectDefinition, Generals.Context, null);
        radar.AddGameObject(gameObject);

        Assert.False(radar.VisibleItems.TryGetValue(0, out _));
    }

    [Fact]
    public void Radar_AddObject()
    {
        var radar = new Radar();

        var objectDefinition = new ObjectDefinition
        {
            RadarPriority = RadarPriority.Unit,
        };
        var gameObject = new GameObject(objectDefinition, Generals.Context, null);
        radar.AddGameObject(gameObject);

        Assert.True(radar.VisibleItems.TryGetValue(0, out var radarItem));
        Assert.Equal(0u, radarItem.ObjectId);
    }

    [Fact]
    public void Radar_RemoveObject()
    {
        var radar = new Radar();

        var objectDefinition = new ObjectDefinition
        {
            RadarPriority = RadarPriority.Unit,
        };
        var gameObject = new GameObject(objectDefinition, Generals.Context, null);

        radar.AddGameObject(gameObject);

        Assert.True(radar.VisibleItems.TryGetValue(0, out var radarItem));
        Assert.Equal(0u, radarItem.ObjectId);

        radar.RemoveGameObject(gameObject);
        Assert.False(radar.VisibleItems.TryGetValue(0, out _));
    }

    [Fact]
    public void Radar_AddEvent()
    {
        var radar = new Radar();
        const RadarEventType type = RadarEventType.Construction;
        var position = new Vector3(1, 2, 3);
        var currentFrame = LogicFrame.Zero;
        const uint xCoordinate = 1u;
        const uint yCoordinate = 2u;

        radar.AddRadarEvent(type, position, currentFrame, xCoordinate, yCoordinate);

        var radarEvent = radar.RadarEvents[0];

        Assert.Equal(type, radarEvent.Type);
        Assert.True(radarEvent.ActivelyAnimating);
        Assert.Equal(currentFrame, radarEvent.StartFrame);
        Assert.Equal(new LogicFrame(120), radarEvent.EndFrame);
        Assert.Equal(new LogicFrame(105), radarEvent.FadeBeginFrame);
    }

    [Fact]
    public void Radar_StopAnimation()
    {
        var radar = new Radar();
        const RadarEventType type = RadarEventType.Construction;
        var position = new Vector3(1, 2, 3);
        var currentFrame = LogicFrame.Zero;
        const uint xCoordinate = 1u;
        const uint yCoordinate = 2u;

        radar.AddRadarEvent(type, position, currentFrame, xCoordinate, yCoordinate);

        var radarEvent = radar.RadarEvents[0];

        Assert.True(radarEvent.ActivelyAnimating);

        radar.StopAnimationForEvent(0);

        radarEvent = radar.RadarEvents[0];
        Assert.False(radarEvent.ActivelyAnimating);
    }

    [Fact]
    public void Radar_GetLastEventLocation()
    {
        var radar = new Radar();
        const RadarEventType type = RadarEventType.Construction;
        var position = new Vector3(1, 2, 3);
        var currentFrame = LogicFrame.Zero;
        const uint xCoordinate = 1u;
        const uint yCoordinate = 2u;

        radar.AddRadarEvent(type, position, currentFrame, xCoordinate, yCoordinate);

        Assert.True(radar.TryGetLastEventLocation(out var eventLocation));
        Assert.Equal(position, eventLocation);
    }

    [Fact]
    public void Radar_GetLastEventLocation_NoPreviousEvents()
    {
        var radar = new Radar();

        Assert.False(radar.TryGetLastEventLocation(out _));
    }

    #endregion
}
