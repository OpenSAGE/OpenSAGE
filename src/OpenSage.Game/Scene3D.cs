using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Map;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Scripting;
using OpenSage.Settings;

using Player = OpenSage.Logic.Player;
using Team = OpenSage.Logic.Team;

namespace OpenSage
{
    public sealed class Scene3D : DisposableBase
    {
        private readonly CameraInputMessageHandler _cameraInputMessageHandler;
        private CameraInputState _cameraInputState;

        private readonly UnitSelectionInputHandler _unitSelectionInputHandler;

        private readonly ParticleSystemManager _particleSystemManager;

        public CameraComponent Camera { get; }

        public ICameraController CameraController { get; set; }

        public MapFile MapFile { get; set; }

        public Terrain.Terrain Terrain { get; }

        public MapScriptCollection Scripts { get; }

        public GameObjectCollection GameObjects { get; }

        public WaypointCollection Waypoints { get; set; }
        public WaypointPathCollection WaypointPaths { get; set; }

        public WorldLighting Lighting { get; }

        public IReadOnlyList<Team> Teams => _teams;
        private List<Team> _teams;

        // TODO: Move these to a World class?
        // TODO: Encapsulate this into a custom collection?
        public IReadOnlyList<Player> Players => _players;
        private List<Player> _players;
        public Player LocalPlayer { get; private set; }

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
            ICameraController cameraController,
            MapFile mapFile,
            Terrain.Terrain terrain,
            MapScriptCollection scripts,
            GameObjectCollection gameObjects,
            WaypointCollection waypoints,
            WaypointPathCollection waypointPaths,
            WorldLighting lighting,
            Player[] players,
            Team[] teams)
        {
            Camera = new CameraComponent(game);
            CameraController = cameraController;

            MapFile = mapFile;
            Terrain = terrain;
            Scripts = scripts;
            GameObjects = AddDisposable(gameObjects);
            Waypoints = waypoints;
            WaypointPaths = waypointPaths;
            Lighting = lighting;

            _unitSelectionInputHandler = new UnitSelectionInputHandler(game.UnitSelection);
            game.InputMessageBuffer.Handlers.Insert(0, _unitSelectionInputHandler);
            AddDisposeAction(() => game.InputMessageBuffer.Handlers.Remove(_unitSelectionInputHandler));

            _cameraInputMessageHandler = new CameraInputMessageHandler();
            game.InputMessageBuffer.Handlers.Add(_cameraInputMessageHandler);
            AddDisposeAction(() => game.InputMessageBuffer.Handlers.Remove(_cameraInputMessageHandler));

            _particleSystemManager = AddDisposable(new ParticleSystemManager(game, this));

            _players = players.ToList();
            _teams = teams.ToList();
            // TODO: This is completely wrong.
            LocalPlayer = _players[0];
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

        internal void Update(GameTime gameTime)
        {
            foreach (var gameObject in GameObjects.Items)
            {
                gameObject.Update(gameTime);
            }

            _particleSystemManager.Update(gameTime);

            _cameraInputMessageHandler.UpdateInputState(ref _cameraInputState);
            CameraController.UpdateCamera(Camera, _cameraInputState, gameTime);
        }

        internal void BuildRenderList(RenderList renderList, CameraComponent camera)
        {
            Terrain?.BuildRenderList(renderList);

            foreach (var gameObject in GameObjects.Items)
            {
                gameObject.BuildRenderList(renderList, camera);
            }

            _particleSystemManager.BuildRenderList(renderList);
        }
    }
}
