using System;
using System.Collections.Generic;
using OpenSage.Audio;
using OpenSage.Content.Loaders;
using OpenSage.Data.Map;
using OpenSage.DataStructures;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Rendering.Shadows;
using OpenSage.Graphics.Rendering.Water;
using OpenSage.Gui;
using OpenSage.Gui.DebugUI;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Rendering;
using OpenSage.Scripting;
using OpenSage.Settings;
using OpenSage.Terrain;
using OpenSage.Terrain.Roads;
using Player = OpenSage.Logic.Player;

namespace OpenSage;

public interface IScene3D : IDisposable
{
    IEditorCameraController EditorCameraController { get; }
    IGameEngine GameEngine { get; }
    SelectionGui SelectionGui { get; }
    DebugOverlay DebugOverlay { get; }
    internal ParticleSystemManager ParticleSystemManager { get; }
    Camera Camera { get; }
    TacticalView TacticalView { get; }
    MapFile MapFile { get; }
    Terrain.Terrain Terrain { get; }
    IQuadtree<GameObject> Quadtree { get; }
    bool ShowTerrain { get; set; }
    WaterAreaCollection WaterAreas { get; }
    bool ShowWater { get; set; }
    RoadCollection Roads { get; }
    bool ShowRoads { get; set; }
    Bridge[] Bridges { get; }
    bool ShowBridges { get; set; }
    bool FrustumCulling { get; set; }
    PlayerScriptsList PlayerScripts { get; }
    IGameObjectCollection GameObjects { get; }
    bool ShowObjects { get; set; }
    CameraCollection Cameras { get; }
    WaypointCollection Waypoints { get; }
    WorldLighting Lighting { get; }
    ShadowSettings Shadows { get; }
    WaterSettings Waters { get; }
    IReadOnlyList<Player> Players { get; }
    Player LocalPlayer { get; }
    Navigation.Navigation Navigation { get; }
    AudioSystem Audio { get; }
    internal AssetLoadContext AssetLoadContext { get; }
    Radar Radar { get; }
    IGame Game { get; }
    GameObject BuildPreviewObject { get; set; }
    RenderScene RenderScene { get; }
    RadarDrawUtil RadarDrawUtil { get; }
    int GetPlayerIndex(Player player);
    void LogicTick(in TimeInterval time);
    void LocalLogicTick(in TimeInterval gameTime, float tickT);
    void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime);
    void Render(DrawingContext2D drawingContext);
    GameObject CreateSkirmishPlayerStartingBuilding(in PlayerSetting playerSetting, Player player);
}
