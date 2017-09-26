using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage
{
    public sealed class GameContext : GraphicsObject
    {
        public GraphicsDevice GraphicsDevice { get; }
        public ContentManager ContentManager { get; }
        public IniDataContext IniDataContext { get; }
        public ParticleSystemManager ParticleSystemManager { get; }
        public MeshEffect MeshEffect { get; }

        public GameContext(FileSystem fileSystem, GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;

            ContentManager = AddDisposable(new ContentManager(fileSystem, graphicsDevice));

            IniDataContext = new IniDataContext();
            IniDataContext.LoadIniFile(fileSystem.GetFile(@"Data\INI\Terrain.ini"));
            IniDataContext.LoadIniFile(fileSystem.GetFile(@"Data\INI\ParticleSystem.ini"));
            foreach (var iniFile in fileSystem.GetFiles(@"Data\INI\Object"))
            {
                IniDataContext.LoadIniFile(iniFile);
            }

            ParticleSystemManager = AddDisposable(new ParticleSystemManager(graphicsDevice));

            MeshEffect = AddDisposable(new MeshEffect(graphicsDevice));
        }
    }
}
