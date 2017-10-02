using System.Linq;
using OpenSage.Graphics.Cameras;
using OpenSage.Settings;

namespace OpenSage
{
    public sealed class Scene
    {
        public Game Game { get; internal set; }

        public SceneSettings Settings { get; } = new SceneSettings();

        public SceneEntitiesCollection Entities { get; }

        public CameraComponent MainCamera => Game.Graphics.Cameras.FirstOrDefault();

        public Scene()
        {
            Entities = new SceneEntitiesCollection(this);
        }
    }
}
