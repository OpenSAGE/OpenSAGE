using OpenSage.Graphics.Cameras;
using OpenSage.Logic;
using OpenSage.Settings;
using OpenSage.Terrain;
using System.Collections.Generic;

namespace OpenSage
{
    public sealed class Scene
    {
        private Game _game;

        public Game Game
        {
            get => _game;
            set
            {
                _game = value;
                if (value != null)
                {
                    CameraController.Initialize(value);
                }
            }
        }

        public HeightMap HeightMap { get; set; }

        public SceneSettings Settings { get; } = new SceneSettings();

        public SceneEntitiesCollection Entities { get; }

        public CameraComponent Camera { get; }

        public RtsCameraController CameraController { get; }

        public Dictionary<string, Team> Teams { get; }

        public Scene()
        {
            Entities = new SceneEntitiesCollection(this);

            Camera = new CameraComponent();
            CameraController = new RtsCameraController(Camera);

            Teams = new Dictionary<string, Team>();
        }
    }
}
