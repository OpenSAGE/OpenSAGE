﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using OpenSage.Scripting;
using OpenSage.Settings;
using Veldrid;
using Player = OpenSage.Logic.Player;
using Team = OpenSage.Logic.Team;

namespace OpenSage
{
    public sealed class Scene3D : DisposableBase
    {
        private readonly CameraInputMessageHandler _cameraInputMessageHandler;
        private CameraInputState _cameraInputState;

        private readonly SelectionMessageHandler _selectionMessageHandler;
        public SelectionGui SelectionGui { get; }

        private readonly DebugMessageHandler _debugMessageHandler;
        public DebugOverlay DebugOverlay { get; private set; }

        private readonly ParticleSystemManager _particleSystemManager;

        public Camera Camera { get; }

        public ICameraController CameraController { get; set; }

        public MapFile MapFile { get; set; }

        public Terrain.Terrain Terrain { get; }
        public bool ShowTerrain { get; set; } = true;

        public Terrain.WaterArea[] WaterAreas { get; }
        public bool ShowWater { get; set; } = true;

        public Terrain.Road[] Roads { get; }
        public bool ShowRoads { get; set; } = true;

        public Terrain.Bridge[] Bridges { get; }
        public bool ShowBridges { get; set; } = true;

        public MapScriptCollection Scripts { get; }

        public GameObjectCollection GameObjects { get; }
        public bool ShowObjects { get; set; } = true;

        public WaypointCollection Waypoints { get; set; }
        public WaypointPathCollection WaypointPaths { get; set; }

        public WorldLighting Lighting { get; }

        public ShadowSettings Shadows { get; } = new ShadowSettings();

        private readonly List<Team> _teams;
        public IReadOnlyList<Team> Teams => _teams;

        // TODO: Move these to a World class?
        // TODO: Encapsulate this into a custom collection?
        public IReadOnlyList<Player> Players => _players;
        private List<Player> _players;
        public Player LocalPlayer { get; private set; }

        private readonly OrderGeneratorInputHandler _orderGeneratorInputHandler;

        internal IEnumerable<AttachedParticleSystem> GetAllAttachedParticleSystems()
        {
            foreach (var gameObject in GameObjects.Items)
            {
                foreach (var attachedParticleSystem in gameObject.GetAllAttachedParticleSystems())
                {
                    yield return attachedParticleSystem;
                }
            }
        }

        public Scene3D(
            Game game,
            InputMessageBuffer inputMessageBuffer,
            Func<Viewport> getViewport,
            ICameraController cameraController,
            MapFile mapFile,
            Terrain.Terrain terrain,
            Terrain.WaterArea[] waterAreas,
            Terrain.Road[] roads,
            Terrain.Bridge[] bridges,
            MapScriptCollection scripts,
            GameObjectCollection gameObjects,
            WaypointCollection waypoints,
            WaypointPathCollection waypointPaths,
            WorldLighting lighting,
            Player[] players,
            Team[] teams,
            bool isDiagnosticScene = false)
        {
            Camera = new Camera(getViewport);
            CameraController = cameraController;

            MapFile = mapFile;
            Terrain = terrain;
            WaterAreas = waterAreas;
            Roads = roads;
            Bridges = bridges;
            Scripts = scripts;
            GameObjects = AddDisposable(gameObjects);
            Waypoints = waypoints;
            WaypointPaths = waypointPaths;
            Lighting = lighting;

            SelectionGui = new SelectionGui();

            RegisterInputHandler(_cameraInputMessageHandler = new CameraInputMessageHandler(), inputMessageBuffer);

            DebugOverlay = new DebugOverlay(this, game.ContentManager);

            if (!isDiagnosticScene)
            {
                RegisterInputHandler(_selectionMessageHandler = new SelectionMessageHandler(game.Selection), inputMessageBuffer);
                RegisterInputHandler(_orderGeneratorInputHandler = new OrderGeneratorInputHandler(game.OrderGenerator), inputMessageBuffer);
                RegisterInputHandler(_debugMessageHandler = new DebugMessageHandler(DebugOverlay), inputMessageBuffer);
            }

            _particleSystemManager = AddDisposable(new ParticleSystemManager(this));

            _players = players.ToList();
            _teams = teams.ToList();
            // TODO: This is completely wrong.
            LocalPlayer = _players.FirstOrDefault();
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

            // TODO: What to do with teams?
            // Teams refer to old Players and therefore they will not be collected by GC
            // (+ objects will have invalid owners)
        }

        // TODO: Move this over to a player collection?
        public int GetPlayerIndex(Player player)
        {
            return _players.IndexOf(player);
        }

        internal void LogicTick(ulong frame)
        {
            foreach (var gameObject in GameObjects.Items)
            {
                gameObject.LogicTick(frame);
            }
        }

        internal void LocalLogicTick(in TimeInterval gameTime, float tickT)
        {
            _orderGeneratorInputHandler?.Update();

            foreach (var gameObject in GameObjects.Items)
            {
                gameObject.LocalLogicTick(gameTime, tickT, Terrain.HeightMap);
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
                foreach (var waterArea in WaterAreas)
                {
                    waterArea.BuildRenderList(renderList);
                }
            }

            if (ShowRoads)
            {
                foreach (var road in Roads)
                {
                    road.BuildRenderList(renderList);
                }
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
                    gameObject.BuildRenderList(renderList, camera);
                }
            }

            _particleSystemManager.BuildRenderList(renderList, gameTime);
        }

        // This is for drawing 2D elements which depend on the Scene3D, e.g tooltips and health bars.
        internal void Render(DrawingContext2D drawingContext)
        {
            SelectionGui?.Draw(drawingContext, Camera);
            DebugOverlay?.Draw(drawingContext, Camera);
        }
    }
}
