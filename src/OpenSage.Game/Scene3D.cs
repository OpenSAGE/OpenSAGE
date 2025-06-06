﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Audio;
using OpenSage.Content.Loaders;
using OpenSage.Content.Util;
using OpenSage.Data.Map;
using OpenSage.DataStructures;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Rendering.Shadows;
using OpenSage.Graphics.Rendering.Water;
using OpenSage.Gui;
using OpenSage.Gui.DebugUI;
using OpenSage.Input;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Rendering;
using OpenSage.Scripting;
using OpenSage.Settings;
using OpenSage.Terrain;
using OpenSage.Terrain.Roads;
using Veldrid;
using Player = OpenSage.Logic.Player;

namespace OpenSage;

public sealed class Scene3D : DisposableBase, IScene3D
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    // TODO: Remove or refactor these
    // RtsCameraController uses its own input handling, so these are only used for debug cameras
    //private readonly EditorCameraInputMessageHandler _editorCameraInputMessageHandler;
    //private EditorCameraInputState _editorCameraInputState;
    public IEditorCameraController EditorCameraController { get; }

    public IGameEngine GameEngine { get; }

    public SelectionGui SelectionGui { get; }

    private readonly DebugMessageHandler _debugMessageHandler;
    public DebugOverlay DebugOverlay { get; private set; }

    ParticleSystemManager IScene3D.ParticleSystemManager => ParticleSystemManager;
    internal ParticleSystemManager ParticleSystemManager { get; }

    private readonly OrderGeneratorSystem _orderGeneratorSystem;

    public Camera Camera { get; }

    public TacticalView TacticalView { get; }

    public MapFile MapFile { get; }

    public Terrain.Terrain Terrain { get; }

    public IQuadtree<GameObject> Quadtree { get; }
    public bool ShowTerrain
    {
        get => RenderScene.GetRenderBucket("Terrain").Visible;
        set => RenderScene.GetRenderBucket("Terrain").Visible = value;
    }

    public WaterAreaCollection WaterAreas { get; }
    public bool ShowWater { get; set; } = true;

    public RoadCollection Roads { get; }
    public bool ShowRoads
    {
        get => RenderScene.GetRenderBucket("Roads").Visible;
        set => RenderScene.GetRenderBucket("Roads").Visible = value;
    }

    public Bridge[] Bridges { get; }
    public bool ShowBridges { get; set; } = true;

    public bool FrustumCulling { get; set; } = false;

    public PlayerScriptsList PlayerScripts { get; private set; }

    public IGameObjectCollection GameObjects => GameEngine.GameLogic;
    public bool ShowObjects { get; set; } = true;
    public CameraCollection Cameras { get; }
    public WaypointCollection Waypoints { get; }

    public WorldLighting Lighting { get; }

    public ShadowSettings Shadows { get; } = new ShadowSettings();

    public WaterSettings Waters { get; } = new WaterSettings();

    public IReadOnlyList<Player> Players => Game.PlayerManager.Players;
    public Player LocalPlayer => Game.PlayerManager.LocalPlayer;
    public Navigation.Navigation Navigation { get; }

    public AudioSystem Audio => Game.Audio;

    AssetLoadContext IScene3D.AssetLoadContext => AssetLoadContext;
    internal AssetLoadContext AssetLoadContext => Game.AssetStore.LoadContext;

    private readonly OrderGeneratorInputHandler _orderGeneratorInputHandler;

    public Radar Radar { get; }

    public IGame Game { get; }

    public GameObject BuildPreviewObject { get; set; }

    public RenderScene RenderScene { get; }

    private readonly Scene25D _scene25D;

    public RadarDrawUtil RadarDrawUtil { get; }

    internal Scene3D(
        IGame game,
        MapFile mapFile,
        string mapPath,
        Data.Map.Player[] mapPlayers,
        Data.Map.Team[] mapTeams,
        ScriptList[] mapScriptLists,
        GameType gameType)
        : this(game, () => game.Viewport, game.InputMessageBuffer, false, mapFile, mapPath)
    {
        game.Scene3D = this;

        game.PlayerManager.OnNewGame(mapPlayers, gameType);

        game.TeamFactory.Initialize(mapTeams);

        Lighting = new WorldLighting(
            mapFile.GlobalLighting.LightingConfigurations.ToLightSettingsDictionary(),
            mapFile.GlobalLighting.Time);

        LoadObjects(
            game.AssetStore.LoadContext,
            Terrain.HeightMap,
            mapFile.ObjectsList.Objects,
            MapFile.NamedCameras,
            out var waypoints,
            out var roads,
            out var bridges,
            out var cameras);

        Roads = roads;
        Bridges = bridges;
        Waypoints = waypoints;
        Cameras = cameras;

        PlayerScripts = new PlayerScriptsList
        {
            ScriptLists = mapScriptLists
        };

        TacticalView = new TacticalView(game.AssetStore.GameData.Current, Camera, game.TerrainLogic, game.Cursors, GameEngine.MsPerLogicFrame);
        RegisterInputHandler(TacticalView.LookAtTranslator, game.InputMessageBuffer);

        game.ContentManager.GraphicsDevice.WaitForIdle();

        _scene25D = game.Definition.CreateScene25D(this, AssetLoadContext.AssetStore);
    }

    private void LoadObjects(
        AssetLoadContext loadContext,
        HeightMap heightMap,
        MapObject[] mapObjects,
        NamedCameras namedCameras,
        out WaypointCollection waypointCollection,
        out RoadCollection roads,
        out Bridge[] bridges,
        out CameraCollection cameras)
    {
        var waypoints = new List<Waypoint>();

        var bridgesList = new List<Bridge>();

        var roadTopology = new RoadTopology();

        for (var i = 0; i < mapObjects.Length; i++)
        {
            var mapObject = mapObjects[i];

            switch (mapObject.RoadType & RoadType.PrimaryType)
            {
                case RoadType.None:
                    switch (mapObject.TypeName)
                    {
                        case Waypoint.ObjectTypeName:
                            waypoints.Add(new Waypoint(mapObject));
                            break;

                        default:
                            GameObject.FromMapObject(mapObject, GameEngine, overwriteAngle: null);
                            break;
                    }
                    break;

                case RoadType.BridgeStart:
                case RoadType.BridgeEnd:
                    // Multiple invalid bridges can be found in e.g GLA01.
                    if ((i + 1) >= mapObjects.Length || !mapObjects[i + 1].RoadType.HasFlag(RoadType.BridgeEnd))
                    {
                        Logger.Warn($"Invalid bridge: {mapObject.ToString()}, skipping...");
                        continue;
                    }

                    var bridgeEnd = mapObjects[++i];

                    bridgesList.Add(AddDisposable(new Bridge(
                        GameEngine,
                        mapObject,
                        mapObject.Position,
                        bridgeEnd.Position)));

                    break;

                case RoadType.Start:
                case RoadType.End:
                    var roadEnd = mapObjects[++i];

                    // Some maps have roads with invalid start- or endpoints.
                    // We'll skip processing them altogether.
                    if (mapObject.TypeName == "" || roadEnd.TypeName == "")
                    {
                        Logger.Warn($"Road {mapObject} has invalid start- or endpoint, skipping...");
                        continue;
                    }

                    if (!mapObject.RoadType.HasFlag(RoadType.Start) || !roadEnd.RoadType.HasFlag(RoadType.End))
                    {
                        throw new InvalidDataException();
                    }

                    // Note that we're searching with the type of either end.
                    // This is because of weirdly corrupted roads with unmatched ends in USA04, which work fine in WB and SAGE.
                    var roadTemplate =
                        loadContext.AssetStore.RoadTemplates.GetByName(mapObject.TypeName)
                        ?? loadContext.AssetStore.RoadTemplates.GetByName(roadEnd.TypeName);

                    if (roadTemplate == null)
                    {
                        throw new InvalidDataException($"Missing road template: {mapObject.TypeName}");
                    }

                    roadTopology.AddSegment(roadTemplate, mapObject, roadEnd);
                    break;

            }

            loadContext.GraphicsDevice.WaitForIdle();
        }

        cameras = new CameraCollection(namedCameras?.Cameras);
        roads = AddDisposable(new RoadCollection(roadTopology, loadContext, heightMap, RenderScene));
        waypointCollection = new WaypointCollection(waypoints, MapFile.WaypointsList.WaypointPaths);
        bridges = bridgesList.ToArray();
    }

    internal Scene3D(
        IGame game,
        InputMessageBuffer inputMessageBuffer,
        Func<Viewport> getViewport,
        IEditorCameraController cameraController,
        WorldLighting lighting,
        bool isDiagnosticScene = false)
        : this(game, getViewport, inputMessageBuffer, isDiagnosticScene, null, null)
    {
        WaterAreas = AddDisposable(new WaterAreaCollection());
        Lighting = lighting;

        Roads = AddDisposable(new RoadCollection());
        Bridges = Array.Empty<Bridge>();
        Waypoints = new WaypointCollection();
        Cameras = new CameraCollection();

        EditorCameraController = cameraController;
    }

    private Scene3D(IGame game, Func<Viewport> getViewport, InputMessageBuffer inputMessageBuffer, bool isDiagnosticScene, MapFile mapFile, string mapPath)
    {
        Game = game;

        Camera = new Camera(getViewport);

        SelectionGui = new SelectionGui();

        DebugOverlay = new DebugOverlay(this, game.ContentManager);

        RenderScene = new RenderScene();

        if (mapFile != null)
        {
            MapFile = mapFile;
            Terrain = AddDisposable(new Terrain.Terrain(mapFile, game.TerrainLogic.HeightMap, game.AssetStore.LoadContext, RenderScene));
            WaterAreas = AddDisposable(new WaterAreaCollection(mapFile.PolygonTriggers, mapFile.StandingWaterAreas, mapFile.StandingWaveAreas, game.AssetStore.LoadContext));
            Navigation = new Navigation.Navigation(mapFile.BlendTileData, Terrain.HeightMap);
        }

        if (!isDiagnosticScene)
        {
            RegisterInputHandler(new SelectionMessageHandler(game.Selection), inputMessageBuffer);
            RegisterInputHandler(_orderGeneratorInputHandler = new OrderGeneratorInputHandler(game.OrderGenerator), inputMessageBuffer);
            RegisterInputHandler(_debugMessageHandler = new DebugMessageHandler(DebugOverlay), inputMessageBuffer);
        }

        ParticleSystemManager = AddDisposable(new ParticleSystemManager(this, game.AssetStore.LoadContext));

        Radar = new Radar();

        if (mapFile != null)
        {
            var borderWidth = mapFile.HeightMapData.BorderWidth * HeightMap.HorizontalScale;
            var width = mapFile.HeightMapData.Width * HeightMap.HorizontalScale;
            var height = mapFile.HeightMapData.Height * HeightMap.HorizontalScale;
            Quadtree = new Quadtree<GameObject>(new RectangleF(-borderWidth, -borderWidth, width, height));
        }

        GameEngine = new GameEngine(
            game.AssetStore.LoadContext,
            game.Audio,
            ParticleSystemManager,
            new ObjectCreationListManager(),
            Terrain,
            Navigation,
            Radar,
            Quadtree,
            this,
            game);

        RadarDrawUtil = new RadarDrawUtil(Radar, Terrain?.HeightMap, GameObjects, Camera, AssetLoadContext.AssetStore, mapPath);

        Game.GameLogic.Reset();
        Game.GameClient.Reset();

        _orderGeneratorSystem = game.OrderGenerator;
    }

    private void RegisterInputHandler(InputMessageHandler handler, InputMessageBuffer inputMessageBuffer)
    {
        inputMessageBuffer.Handlers.Add(handler);
        AddDisposeAction(() => inputMessageBuffer.Handlers.Remove(handler));
    }

    // TODO: Move this over to a player collection?
    public int GetPlayerIndex(Player player) => Game.PlayerManager.GetPlayerIndex(player);

    public void LogicTick(in TimeInterval time)
    {
        Game.PlayerManager.LogicTick();

        foreach (var gameObject in GameObjects.Objects)
        {
            gameObject.LogicTick(time);
        }

        GameObjects.DeleteDestroyed();
    }

    public void LocalLogicTick(in TimeInterval gameTime, float tickT)
    {
        _orderGeneratorInputHandler?.Update();

        foreach (var gameObject in GameObjects.Objects)
        {
            gameObject.LocalLogicTick(gameTime, tickT, Terrain?.HeightMap);
        }

        // TODO: Remove or refactor these
        //_editorCameraInputMessageHandler?.UpdateInputState(ref _editorCameraInputState);
        //EditorCameraController?.UpdateCamera(Camera, _editorCameraInputState, gameTime);
        TacticalView.Update(gameTime);

        DebugOverlay.Update(gameTime);

        if (_orderGeneratorInputHandler != null)
        {
            _orderGeneratorSystem.Update(gameTime, _orderGeneratorInputHandler.KeyModifiers);
        }

        Terrain?.Update(Waters, gameTime);

        ParticleSystemManager?.Update(gameTime);
    }

    public void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime)
    {
        if (ShowWater)
        {
            WaterAreas.BuildRenderList(renderList);
        }

        if (ShowBridges)
        {
            foreach (var bridge in Bridges)
            {
                bridge.BuildRenderList(renderList, camera);
            }
        }

        if (ShowObjects)
        {
            foreach (var gameObject in GameObjects.Objects)
            {
                if (!FrustumCulling
                    || gameObject.Definition.KindOf.Get(ObjectKinds.NoCollide)
                    || gameObject.RoughCollider.Intersects(Camera.BoundingFrustum))
                {
                    gameObject.BuildRenderList(renderList, camera, gameTime);
                }
            }
        }

        _orderGeneratorSystem.BuildRenderList(renderList, camera, gameTime);
    }

    // This is for drawing 2D elements which depend on the Scene3D, e.g tooltips and health bars.
    public void Render(DrawingContext2D drawingContext)
    {
        _scene25D?.Draw(drawingContext);

        SelectionGui?.Draw(drawingContext);
        DebugOverlay?.Draw(drawingContext, Camera);
    }

    // TODO(Port): placeNetworkBuildingsForPlayer in GameLogic.cpp
    public GameObject CreateSkirmishPlayerStartingBuilding(in PlayerSetting playerSetting, Player player)
    {
        // TODO: Not sure what the OG does here.
        var playerStartPosition = new Vector3(80, 80, 0);
        if (Waypoints.TryGetPlayerStart(playerSetting.StartPosition ?? 1, out var startWaypoint))
        {
            playerStartPosition = startWaypoint.Position;
        }
        playerStartPosition.Z += Terrain.HeightMap.GetHeight(playerStartPosition.X, playerStartPosition.Y);

        if (player.Template.StartingBuilding != null)
        {
            var startingBuilding = GameObjects.CreateObject(player.Template.StartingBuilding.Value, player);
            var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathUtility.ToRadians(startingBuilding.Definition.PlacementViewAngle));
            startingBuilding.UpdateTransform(playerStartPosition, rotation);

            Navigation.UpdateAreaPassability(startingBuilding, false);

            var startingUnit0 = GameObjects.CreateObject(player.Template.StartingUnits[0].Unit.Value, player);
            var startingUnit0Position = playerStartPosition;
            startingUnit0Position += Vector3.Transform(Vector3.UnitX, startingBuilding.Rotation) * startingBuilding.Definition.Geometry.Shapes[0].MajorRadius;
            startingUnit0.SetTranslation(startingUnit0Position);

            Game.Selection.SetSelectedObjects(player, [startingBuilding], playAudio: false);
            return startingBuilding;
        }
        else
        {
            // BFME specific logic

            var castleBehaviors = new List<(CastleBehavior, TeamTemplate)>();
            GameObject lastGameObject = null;
            foreach (var gameObject in GameObjects.Objects)
            {
                var team = gameObject.TeamTemplate;
                if (team?.Name == $"Player_{playerSetting.StartPosition}_Inherit")
                {
                    var castleBehavior = gameObject.FindBehavior<CastleBehavior>();
                    if (castleBehavior != null)
                    {
                        castleBehaviors.Add((castleBehavior, team));
                        lastGameObject = gameObject;
                    }
                }
            }
            foreach (var (castleBehavior, _) in castleBehaviors)
            {
                castleBehavior.Unpack(player, instant: true);
            }

            // This is basically guaranteed to be wrong
            if (lastGameObject != null)
            {
                return lastGameObject;
            }
        }

        return null;
    }
}
