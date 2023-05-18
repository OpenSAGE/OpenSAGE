using System;
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

        internal readonly ParticleSystemManager ParticleSystemManager;

        private readonly OrderGeneratorSystem _orderGeneratorSystem;

        public readonly Camera Camera;

        public readonly ICameraController CameraController;

        public readonly MapFile MapFile;

        public readonly Terrain.Terrain Terrain;

        public readonly Quadtree<GameObject> Quadtree;
        public bool ShowTerrain
        {
            get => RenderScene.GetRenderBucket("Terrain").Visible;
            set => RenderScene.GetRenderBucket("Terrain").Visible = value;
        }

        public readonly WaterAreaCollection WaterAreas;
        public bool ShowWater { get; set; } = true;

        public readonly RoadCollection Roads;
        public bool ShowRoads
        {
            get => RenderScene.GetRenderBucket("Roads").Visible;
            set => RenderScene.GetRenderBucket("Roads").Visible = value;
        }

        public readonly Bridge[] Bridges;
        public bool ShowBridges { get; set; } = true;

        public bool FrustumCulling { get; set; } = false;

        public PlayerScriptsList PlayerScripts { get; private set; }

        public readonly GameObjectCollection GameObjects;
        public bool ShowObjects { get; set; } = true;
        public readonly CameraCollection Cameras;
        public readonly WaypointCollection Waypoints;

        public readonly WorldLighting Lighting;

        public readonly ShadowSettings Shadows = new ShadowSettings();

        public WaterSettings Waters { get; } = new WaterSettings();

        public IReadOnlyList<Player> Players => Game.PlayerManager.Players;
        public Player LocalPlayer => Game.PlayerManager.LocalPlayer;
        public readonly Navigation.Navigation Navigation;

        internal AudioSystem Audio => Game.Audio;
        internal AssetLoadContext AssetLoadContext => Game.AssetStore.LoadContext;

        public readonly Random Random;

        private readonly OrderGeneratorInputHandler _orderGeneratorInputHandler;

        public readonly Radar Radar;

        public readonly Game Game;

        public GameObject BuildPreviewObject;

        public readonly RenderScene RenderScene;

        internal Scene3D(
            Game game,
            MapFile mapFile,
            string mapPath,
            int randomSeed,
            Data.Map.Player[] mapPlayers,
            Data.Map.Team[] mapTeams,
            ScriptList[] mapScriptLists,
            GameType gameType)
            : this(game, () => game.Viewport, game.InputMessageBuffer, randomSeed, false, mapFile, mapPath)
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

            CameraController = new RtsCameraController(game.AssetStore.GameData.Current, Camera, Terrain.HeightMap)
            {
                TerrainPosition = Terrain.HeightMap.GetPosition(
                    Terrain.HeightMap.Width / 2,
                    Terrain.HeightMap.Height / 2)
            };

            game.ContentManager.GraphicsDevice.WaitForIdle();
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
                                GameObject.FromMapObject(mapObject, GameContext, overwriteAngle: null);
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
                            GameContext,
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
            Game game,
            InputMessageBuffer inputMessageBuffer,
            Func<Viewport> getViewport,
            ICameraController cameraController,
            WorldLighting lighting,
            int randomSeed,
            bool isDiagnosticScene = false)
            : this(game, getViewport, inputMessageBuffer, randomSeed, isDiagnosticScene, null, null)
        {
            WaterAreas = AddDisposable(new WaterAreaCollection());
            Lighting = lighting;

            Roads = AddDisposable(new RoadCollection());
            Bridges = Array.Empty<Bridge>();
            Waypoints = new WaypointCollection();
            Cameras = new CameraCollection();

            CameraController = cameraController;
        }

        private Scene3D(Game game, Func<Viewport> getViewport, InputMessageBuffer inputMessageBuffer, int randomSeed, bool isDiagnosticScene, MapFile mapFile, string mapPath)
        {
            Game = game;

            Camera = new Camera(getViewport);

            SelectionGui = new SelectionGui();

            DebugOverlay = new DebugOverlay(this, game.ContentManager);

            Random = new Random(randomSeed);

            RenderScene = new RenderScene();

            if (mapFile != null)
            {
                MapFile = mapFile;
                Terrain = AddDisposable(new Terrain.Terrain(mapFile, game.AssetStore.LoadContext, RenderScene));
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

            ParticleSystemManager = AddDisposable(new ParticleSystemManager(this, game.AssetStore.LoadContext));

            Radar = new Radar(this, game.AssetStore, mapPath);

            if (mapFile != null)
            {
                var borderWidth = mapFile.HeightMapData.BorderWidth * HeightMap.HorizontalScale;
                var width = mapFile.HeightMapData.Width * HeightMap.HorizontalScale;
                var height = mapFile.HeightMapData.Height * HeightMap.HorizontalScale;
                Quadtree = new Quadtree<GameObject>(new RectangleF(-borderWidth, -borderWidth, width, height));
            }

            GameContext = new GameContext(
                game.AssetStore.LoadContext,
                game.Audio,
                ParticleSystemManager,
                new ObjectCreationListManager(),
                Terrain,
                Navigation,
                Radar,
                Quadtree,
                this);

            Game.GameLogic.Reset();
            Game.GameClient.Reset();

            GameObjects = AddDisposable(new GameObjectCollection(GameContext));

            GameContext.GameObjects = GameObjects;

            _orderGeneratorSystem = game.OrderGenerator;
        }

        private void RegisterInputHandler(InputMessageHandler handler, InputMessageBuffer inputMessageBuffer)
        {
            inputMessageBuffer.Handlers.Add(handler);
            AddDisposeAction(() => inputMessageBuffer.Handlers.Remove(handler));
        }

        // TODO: Move this over to a player collection?
        public int GetPlayerIndex(Player player) => Game.PlayerManager.GetPlayerIndex(player);

        internal void LogicTick(in TimeInterval time)
        {
            Game.PlayerManager.LogicTick();

            foreach (var gameObject in GameObjects.Items)
            {
                gameObject.LogicTick(time);
            }

            GameObjects.DeleteDestroyed();
        }

        internal void CursorLogicTick(in TimeInterval gameTime)
        {
            if (_orderGeneratorInputHandler != null)
            {
                _orderGeneratorSystem.Update(gameTime, _orderGeneratorInputHandler.KeyModifiers);
            }
        }
        internal void LocalLogicTick(in TimeInterval gameTime, float tickT)
        {
            _orderGeneratorInputHandler?.Update();

            foreach (var gameObject in GameObjects.Items)
            {
                gameObject.LocalLogicTick(gameTime, tickT, Terrain?.HeightMap);
            }

            _cameraInputMessageHandler?.UpdateInputState(ref _cameraInputState);
            CameraController.UpdateCamera(Camera, _cameraInputState, gameTime);

            DebugOverlay.Update(gameTime);

            Terrain?.Update(Waters, gameTime);

            ParticleSystemManager?.Update(gameTime);
        }

        internal void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime)
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
                foreach (var gameObject in GameObjects.Items)
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
                if (gameObject.Definition.KindOf.Get(ObjectKinds.Horde))
                {
                    return;
                }

                var geometrySize = gameObject.Definition.Geometry.Shapes[0].MajorRadius;

                // Not sure if this is what IsSmall is actually for.
                if (gameObject.Definition.Geometry.IsSmall)
                {
                    geometrySize = Math.Max(geometrySize, 15);
                }

                var boundingSphere = new BoundingSphere(gameObject.Translation, geometrySize);
                var healthBoxSize = Camera.GetScreenSize(boundingSphere);

                var healthBoxWorldSpacePos = gameObject.Translation.WithZ(gameObject.Translation.Z + gameObject.Definition.Geometry.Shapes[0].Height);
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
                if (gameObject.HasActiveBody())
                {
                    DrawBar(
                        healthBoxRect.Value,
                        new ColorRgbaF(0, 1, 0, 1),
                        (float)gameObject.HealthPercentage);
                }

                var yOffset = 0;
                if (gameObject.Definition.KindOf.Get(ObjectKinds.Structure) && gameObject.IsBeingConstructed())
                {
                    yOffset += 4;
                    var constructionProgressBoxRect = healthBoxRect.Value.WithY(healthBoxRect.Value.Y + yOffset);
                    DrawBar(
                        constructionProgressBoxRect,
                        new ColorRgba(172, 255, 254, 255).ToColorRgbaF(),
                        gameObject.BuildProgress);
                }
                else if (gameObject.ProductionUpdate != null)
                {
                    yOffset += 4;
                    var productionBoxRect = healthBoxRect.Value.WithY(healthBoxRect.Value.Y + yOffset);
                    var productionBoxValue = gameObject.ProductionUpdate.IsProducing
                        ? gameObject.ProductionUpdate.ProductionQueue[0].Progress
                        : 0;

                    DrawBar(
                        productionBoxRect,
                        new ColorRgba(172, 255, 254, 255).ToColorRgbaF(),
                        productionBoxValue);
                }

                var gainsExperience = gameObject.FindBehavior<ExperienceUpdate>().ObjectGainsExperience;
                if (gainsExperience)
                {
                    yOffset += 4;
                    var experienceBoxRect = healthBoxRect.Value.WithY(healthBoxRect.Value.Y + yOffset);
                    DrawBar(
                        experienceBoxRect,
                        new ColorRgba(255, 255, 0, 255).ToColorRgbaF(),
                        gameObject.ExperienceValue / (float)gameObject.ExperienceRequiredForNextLevel);
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

        internal void CreateSkirmishPlayerStartingBuilding(in PlayerSetting playerSetting, Player player)
        {
            // TODO: Not sure what the OG does here.
            var playerStartPosition = new Vector3(80, 80, 0);
            if (Waypoints.TryGetByName($"Player_{playerSetting.StartPosition}_Start", out var startWaypoint))
            {
                playerStartPosition = startWaypoint.Position;
            }
            playerStartPosition.Z += Terrain.HeightMap.GetHeight(playerStartPosition.X, playerStartPosition.Y);

            if (player.Template.StartingBuilding != null)
            {
                var startingBuilding = GameObjects.Add(player.Template.StartingBuilding.Value, player);
                var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathUtility.ToRadians(startingBuilding.Definition.PlacementViewAngle));
                startingBuilding.UpdateTransform(playerStartPosition, rotation);

                Navigation.UpdateAreaPassability(startingBuilding, false);

                var startingUnit0 = GameObjects.Add(player.Template.StartingUnits[0].Unit.Value, player);
                var startingUnit0Position = playerStartPosition;
                startingUnit0Position += Vector3.Transform(Vector3.UnitX, startingBuilding.Rotation) * startingBuilding.Definition.Geometry.Shapes[0].MajorRadius;
                startingUnit0.SetTranslation(startingUnit0Position);

                Game.Selection.SetSelectedObjects(player, new[] { startingBuilding }, playAudio: false);
            }
            else
            {
                var castleBehaviors = new List<(CastleBehavior, TeamTemplate)>();
                foreach (var gameObject in GameObjects.Items)
                {
                    var team = gameObject.TeamTemplate;
                    if (team?.Name == $"Player_{playerSetting.StartPosition}_Inherit")
                    {
                        var castleBehavior = gameObject.FindBehavior<CastleBehavior>();
                        if (castleBehavior != null)
                        {
                            castleBehaviors.Add((castleBehavior, team));
                        }
                    }
                }
                foreach (var (castleBehavior, _) in castleBehaviors)
                {
                    castleBehavior.Unpack(player, instant: true);
                }
            }
        }
    }
}
