using System.Collections.Generic;
using OpenSage.Data.Map;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic.Object;
using OpenSage.Scripting;
using OpenSage.Settings;

namespace OpenSage
{
    public sealed class Scene3D : DisposableBase
    {
        private readonly CameraInputMessageHandler _cameraInputMessageHandler;
        private CameraInputState _cameraInputState;

        public CameraComponent Camera { get; }

        public ICameraController CameraController { get; set; }

        public MapFile MapFile { get; set; }

        public Terrain.Terrain Terrain { get; }

        public MapScriptCollection Scripts { get; }

        public GameObjectCollection GameObjects { get; }

        public WaypointCollection Waypoints { get; set; }
        public WaypointPathCollection WaypointPaths { get; set; }

        public WorldLighting Lighting { get; }

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
            WorldLighting lighting)
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

            _cameraInputMessageHandler = new CameraInputMessageHandler();
            game.Input.MessageBuffer.Handlers.Add(_cameraInputMessageHandler);
            AddDisposeAction(() => game.Input.MessageBuffer.Handlers.Remove(_cameraInputMessageHandler));
        }

        internal void Update(GameTime gameTime)
        {
            foreach (var gameObject in GameObjects.Items)
            {
                gameObject.Update(gameTime);
            }

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
        }
    }
}
