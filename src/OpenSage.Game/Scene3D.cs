using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Audio;
using OpenSage.Content.Loaders;
using OpenSage.Content.Util;
using OpenSage.Data.Map;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Rendering.Shadows;
using OpenSage.Gui;
using OpenSage.Gui.DebugUI;
using OpenSage.Input;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Scripting;
using OpenSage.Settings;
using OpenSage.Terrain;
using OpenSage.Terrain.Roads;
using Veldrid;
using Player = OpenSage.Logic.Player;
using Team = OpenSage.Logic.Team;

namespace OpenSage
{
    public sealed class Scene3D : DisposableBase
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly CameraInputMessageHandler _cameraInputMessageHandler;
        private CameraInputState _cameraInputState;

        internal readonly GameContext GameContext;

        public SelectionGui SelectionGui { get; }

        private readonly DebugMessageHandler _debugMessageHandler;
        public DebugOverlay DebugOverlay { get; private set; }

        private readonly ParticleSystemManager _particleSystemManager;

        private readonly OrderGeneratorSystem _orderGeneratorSystem;

        public readonly Camera Camera;

        public readonly ICameraController CameraController;

        public readonly MapFile MapFile;

        public readonly Terrain.Terrain Terrain;
        public bool ShowTerrain { get; set; } = true;

        public readonly WaterAreaCollection WaterAreas;
        public bool ShowWater { get; set; } = true;

        public readonly RoadCollection Roads;
        public bool ShowRoads { get; set; } = true;

        public readonly Bridge[] Bridges;
        public bool ShowBridges { get; set; } = true;

        public MapScriptCollection[] PlayerScripts { get; }

        public readonly GameObjectCollection GameObjects;
        public bool ShowObjects { get; set; } = true;
        public readonly CameraCollection Cameras;
        public readonly WaypointCollection Waypoints;

        public readonly WorldLighting Lighting;

        public readonly ShadowSettings Shadows = new ShadowSettings();

        private readonly List<Team> _teams;
        public IReadOnlyList<Team> Teams => _teams;

        // TODO: Move these to a World class?
        // TODO: Encapsulate this into a custom collection?
        public IReadOnlyList<Player> Players => _players;
        private List<Player> _players;
        public Player LocalPlayer { get; private set; }
        public readonly Navigation.Navigation Navigation;

        internal readonly AudioSystem Audio;
        internal readonly AssetLoadContext AssetLoadContext;

        public readonly Random Random;

        private readonly OrderGeneratorInputHandler _orderGeneratorInputHandler;

        internal Scene3D(Game game, MapFile mapFile, int randomSeed)
            : this(game, () => game.Viewport, game.InputMessageBuffer, randomSeed, false, mapFile)
        {
            var contentManager = game.ContentManager;

            _players = Player.FromMapData(mapFile.SidesList.Players, game.AssetStore).ToList();

            // TODO: This is completely wrong.
            LocalPlayer = _players.FirstOrDefault();

            _teams = (mapFile.SidesList.Teams ?? mapFile.Teams.Items)
                .Select(team => Team.FromMapData(team, _players))
                .ToList();

            Audio = game.Audio;
            AssetLoadContext = game.AssetStore.LoadContext;

            Lighting = new WorldLighting(
                mapFile.GlobalLighting.LightingConfigurations.ToLightSettingsDictionary(),
                mapFile.GlobalLighting.Time);

            LoadObjects(
                game.AssetStore.LoadContext,
                Terrain.HeightMap,
                mapFile.ObjectsList.Objects,
                MapFile.NamedCameras,
                _teams,
                out var waypoints,
                out var roads,
                out var bridges,
                out var cameras);

            Roads = roads;
            Bridges = bridges;
            Waypoints = waypoints;
            Cameras = cameras;

            PlayerScripts = mapFile
                .GetPlayerScriptsList()
                .ScriptLists
                .Select(s => new MapScriptCollection(s))
                .ToArray();

            CameraController = new RtsCameraController(game.AssetStore.GameData.Current)
            {
                TerrainPosition = Terrain.HeightMap.GetPosition(
                    Terrain.HeightMap.Width / 2,
                    Terrain.HeightMap.Height / 2)
            };

            contentManager.GraphicsDevice.WaitForIdle();
        }

        private void LoadObjects(
            AssetLoadContext loadContext,
            HeightMap heightMap,
            MapObject[] mapObjects,
            NamedCameras namedCameras,
            List<Team> teams,
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

                var position = mapObject.Position;

                switch (mapObject.RoadType & RoadType.PrimaryType)
                {
                    case RoadType.None:
                        switch (mapObject.TypeName)
                        {
                            case Waypoint.ObjectTypeName:
                                waypoints.Add(new Waypoint(mapObject));
                                break;

                            default:
                                position.Z += heightMap.GetHeight(position.X, position.Y);

                                GameObject.FromMapObject(mapObject, teams, loadContext.AssetStore, GameObjects, position);

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
                            loadContext,
                            heightMap,
                            mapObject,
                            mapObject.Position,
                            bridgeEnd.Position,
                            GameObjects)));

                        break;

                    case RoadType.Start:
                    case RoadType.End:
                        var roadEnd = mapObjects[++i];

                        // Some maps have roads with invalid start- or endpoints.
                        // We'll skip processing them altogether.
                        if (mapObject.TypeName == "" || roadEnd.TypeName == "")
                        {
                            Logger.Warn($"Road {mapObject.ToString()} has invalid start- or endpoint, skipping...");
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
            roads = AddDisposable(new RoadCollection(roadTopology, loadContext, heightMap));
            waypointCollection = new WaypointCollection(waypoints, MapFile.WaypointsList.WaypointPaths);
            bridges = bridgesList.ToArray();
        }

        internal Scene3D(
            Game game,
            InputMessageBuffer inputMessageBuffer,
            Func<Viewport> getViewport,
            ICameraController cameraController,
            WorldLighting lighting,
            int randomSeed,
            bool isDiagnosticScene = false)
            : this(game, getViewport, inputMessageBuffer, randomSeed, isDiagnosticScene, null)
        {
            _players = new List<Player>();
            _teams = new List<Team>();

            // TODO: This is completely wrong.
            LocalPlayer = _players.FirstOrDefault();

            WaterAreas = AddDisposable(new WaterAreaCollection());
            Lighting = lighting;

            Roads = AddDisposable(new RoadCollection());
            Bridges = Array.Empty<Bridge>();
            Waypoints = new WaypointCollection();
            Cameras = new CameraCollection();

            CameraController = cameraController;
        }

        private Scene3D(Game game, Func<Viewport> getViewport, InputMessageBuffer inputMessageBuffer, int randomSeed, bool isDiagnosticScene, MapFile mapFile)
        {
            Camera = new Camera(getViewport);

            SelectionGui = new SelectionGui();

            DebugOverlay = new DebugOverlay(this, game.ContentManager);

            Random = new Random(randomSeed);

            if (mapFile != null)
            {
                MapFile = mapFile;
                Terrain = AddDisposable(new Terrain.Terrain(mapFile, game.AssetStore.LoadContext));
                WaterAreas = AddDisposable(new WaterAreaCollection(mapFile.PolygonTriggers, mapFile.StandingWaterAreas, mapFile.StandingWaveAreas, game.AssetStore.LoadContext));
                Navigation = new Navigation.Navigation(mapFile.BlendTileData, Terrain.HeightMap);
            }

            RegisterInputHandler(_cameraInputMessageHandler = new CameraInputMessageHandler(), inputMessageBuffer);

            if (!isDiagnosticScene)
            {
                RegisterInputHandler(new SelectionMessageHandler(game.Selection), inputMessageBuffer);
                RegisterInputHandler(_orderGeneratorInputHandler = new OrderGeneratorInputHandler(game.OrderGenerator), inputMessageBuffer);
                RegisterInputHandler(_debugMessageHandler = new DebugMessageHandler(DebugOverlay), inputMessageBuffer);
            }

            _particleSystemManager = AddDisposable(new ParticleSystemManager(game.AssetStore.LoadContext));

            GameContext = new GameContext(
                game.AssetStore.LoadContext,
                game.Audio,
                _particleSystemManager,
                Terrain);

            GameObjects = AddDisposable(
                new GameObjectCollection(
                    GameContext,
                    game.CivilianPlayer,
                    Navigation));

            GameContext.GameObjects = GameObjects;

            _orderGeneratorSystem = game.OrderGenerator;
        }

        private void RegisterInputHandler(InputMessageHandler handler, InputMessageBuffer inputMessageBuffer)
        {
            inputMessageBuffer.Handlers.Add(handler);
            AddDisposeAction(() => inputMessageBuffer.Handlers.Remove(handler));
        }

        public void SetPlayers(IEnumerable<Player> players, Player localPlayer)
        {
            _players = players.ToList();

            if (!_players.Contains(localPlayer))
            {
                throw new ArgumentException(
                    $"Argument {nameof(localPlayer)} should be included in {nameof(players)}",
                    nameof(localPlayer));
            }

            LocalPlayer = localPlayer;

            if (LocalPlayer.SelectedUnits.Count > 0)
            {
                var mainUnit = LocalPlayer.SelectedUnits.First();
                CameraController.GoToObject(mainUnit);
            }

            // TODO: What to do with teams?
            // Teams refer to old Players and therefore they will not be collected by GC
            // (+ objects will have invalid owners)
        }

        // TODO: Move this over to a player collection?
        public int GetPlayerIndex(Player player)
        {
            return _players.IndexOf(player);
        }

        internal void LogicTick(ulong frame, in TimeInterval time)
        {
            var currentCount = GameObjects.Items.Count;
            for (var index = 0; index < currentCount; index++)
            {
                var gameObject = GameObjects.Items[index];
                gameObject.LogicTick(frame, time);
            }
        }

        internal void LocalLogicTick(in TimeInterval gameTime, float tickT)
        {
            _orderGeneratorInputHandler?.Update();

            for (int i = 0; i < GameObjects.Items.Count; i++)
            {
                var gameObject = GameObjects.Items[i];
                gameObject.LocalLogicTick(gameTime, tickT, Terrain?.HeightMap);
            }

            _cameraInputMessageHandler?.UpdateInputState(ref _cameraInputState);
            CameraController.UpdateCamera(Camera, _cameraInputState, gameTime);

            DebugOverlay.Update(gameTime);
        }

        internal void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime)
        {
            if (ShowTerrain)
            {
                Terrain?.BuildRenderList(renderList);
            }

            if (ShowWater)
            {
                WaterAreas.BuildRenderList(renderList);
            }

            if (ShowRoads)
            {
                Roads.BuildRenderList(renderList);
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
                foreach (var gameObject in GameObjects.Items)
                {
                    gameObject.BuildRenderList(renderList, camera, gameTime);
                }
            }

            _particleSystemManager.BuildRenderList(renderList);

            _orderGeneratorSystem.BuildRenderList(renderList, camera, gameTime);
        }

        // This is for drawing 2D elements which depend on the Scene3D, e.g tooltips and health bars.
        internal void Render(DrawingContext2D drawingContext)
        {
            DrawHealthBoxes(drawingContext);

            SelectionGui?.Draw(drawingContext);
            DebugOverlay?.Draw(drawingContext, Camera);
        }

        private void DrawHealthBoxes(DrawingContext2D drawingContext)
        {
            void DrawHealthBox(GameObject gameObject)
            {
                var geometrySize = gameObject.Definition.Geometry.MajorRadius;

                // Not sure if this is what IsSmall is actually for.
                if (gameObject.Definition.Geometry.IsSmall)
                {
                    geometrySize = Math.Max(geometrySize, 15);
                }

                var boundingSphere = new BoundingSphere(gameObject.Transform.Translation, geometrySize);
                var healthBoxSize = Camera.GetScreenSize(boundingSphere);

                var healthBoxWorldSpacePos = gameObject.Transform.Translation.WithZ(gameObject.Transform.Translation.Z + gameObject.Definition.Geometry.Height);
                var healthBoxRect = Camera.WorldToScreenRectangle(
                    healthBoxWorldSpacePos,
                    new SizeF(healthBoxSize, 3));

                if (healthBoxRect == null)
                {
                    return;
                }

                void DrawBar(in RectangleF rect, in ColorRgbaF color, float value)
                {
                    var actualRect = rect.WithWidth(rect.Width * value);
                    drawingContext.FillRectangle(actualRect, color);

                    var borderColor = color.WithRGB(color.R / 2.0f, color.G / 2.0f, color.B / 2.0f);
                    drawingContext.DrawRectangle(rect, borderColor, 1);
                }

                // TODO: Not sure what to draw for InactiveBody?
                if (gameObject.Body is ActiveBody)
                {
                    DrawBar(
                        healthBoxRect.Value,
                        new ColorRgbaF(0, 1, 0, 1),
                        (float) (gameObject.Body.Health / gameObject.Body.MaxHealth));
                }

                if (gameObject.ProductionUpdate != null)
                {
                    var productionBoxRect = healthBoxRect.Value.WithY(healthBoxRect.Value.Y + 4);
                    var productionBoxValue = gameObject.ProductionUpdate.IsProducing
                        ? gameObject.ProductionUpdate.ProductionQueue[0].Progress
                        : 0;

                    DrawBar(
                        productionBoxRect,
                        new ColorRgba(172, 255, 254, 255).ToColorRgbaF(),
                        productionBoxValue);
                }
            }

            // The AssetViewer has no LocalPlayer
            if (LocalPlayer != null)
            {
                foreach (var selectedUnit in LocalPlayer.SelectedUnits)
                {
                    DrawHealthBox(selectedUnit);
                }

                if (LocalPlayer.HoveredUnit != null)
                {
                    DrawHealthBox(LocalPlayer.HoveredUnit);
                }
            }
        }
    }
}
