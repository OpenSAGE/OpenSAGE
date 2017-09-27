using OpenSage.Settings;

namespace OpenSage
{
    public sealed class Scene
    {
        public Game Game { get; internal set; }

        public SceneSettings Settings { get; }

        public SceneEntitiesCollection Entities { get; }

        public Scene()
        {
            Entities = new SceneEntitiesCollection(this);
        }
    }
}
