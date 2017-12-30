using OpenSage.Graphics.Cameras;
using OpenSage.Logic;
using OpenSage.Settings;
using OpenSage.Terrain;
using System.Collections.Generic;

namespace OpenSage
{
    public sealed class Scene
    {
        public Game Game { get; set; }

        public HeightMap HeightMap { get; set; }

        public SceneSettings Settings { get; } = new SceneSettings();

        public SceneEntitiesCollection Entities { get; }

        public CameraComponent Camera { get; }

        public ICameraController CameraController { get; set; }

        public Dictionary<string, Team> Teams { get; }

        public OpenSage.Data.Map.MapFile MapFile { get; set; }

        public Scene()
        {
            Entities = new SceneEntitiesCollection(this);

            Camera = new CameraComponent();

            Teams = new Dictionary<string, Team>();
        }
    }
}
