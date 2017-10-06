using System;
using System.Collections.Generic;
using LLGfx;
using LLGfx.Effects;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.Content
{
    public sealed class ContentManager : GraphicsObject
    {
        private readonly Dictionary<Type, ContentLoader> _contentLoaders;

        private readonly Dictionary<string, object> _cachedObjects;

        private readonly FileSystem _fileSystem;

        private readonly Dictionary<Type, Effect> _effects;

        public GraphicsDevice GraphicsDevice { get; }

        public FileSystem FileSystem => _fileSystem;

        public IniDataContext IniDataContext { get; }

        public ContentManager(FileSystem fileSystem, GraphicsDevice graphicsDevice)
        {
            _fileSystem = fileSystem;
            GraphicsDevice = graphicsDevice;

            _contentLoaders = new Dictionary<Type, ContentLoader>
            {
                { typeof(Model), AddDisposable(new ModelLoader()) },
                { typeof(Scene), AddDisposable(new MapLoader()) },
                { typeof(Texture), AddDisposable(new TextureLoader(graphicsDevice)) }
            };

            _cachedObjects = new Dictionary<string, object>();

            _effects = new Dictionary<Type, Effect>
            {
                { typeof(MeshEffect), AddDisposable(new MeshEffect(graphicsDevice)) },
                { typeof(ParticleEffect), AddDisposable(new ParticleEffect(graphicsDevice)) },
                { typeof(SpriteEffect), AddDisposable(new SpriteEffect(graphicsDevice)) },
            };

            IniDataContext = new IniDataContext();
            IniDataContext.LoadIniFile(fileSystem.GetFile(@"Data\INI\GameData.ini"));
            IniDataContext.LoadIniFile(fileSystem.GetFile(@"Data\INI\Terrain.ini"));
            IniDataContext.LoadIniFile(fileSystem.GetFile(@"Data\INI\ParticleSystem.ini"));
            foreach (var iniFile in fileSystem.GetFiles(@"Data\INI\Object"))
            {
                IniDataContext.LoadIniFile(iniFile);
            }
        }

        public void Unload()
        {
            foreach (var cachedObject in _cachedObjects.Values)
            {
                if (cachedObject is IDisposable d)
                {
                    RemoveAndDispose(d);
                }
            }
            _cachedObjects.Clear();
        }

        public T GetEffect<T>()
            where T : Effect
        {
            return (T) _effects[typeof(T)];
        }

        public T Load<T>(
            string filePath, 
            ResourceUploadBatch uploadBatch,
            LoadOptions options = null)
            where T : class
        {
            if (_cachedObjects.TryGetValue(filePath, out var asset))
            {
                return (T) asset;
            }

            var type = typeof(T);
            
            if (!_contentLoaders.TryGetValue(type, out var contentLoader))
            {
                throw new Exception($"Could not finder content loader for type '{type.FullName}'");
            }

            FileSystemEntry entry = null;
            foreach (var testFilePath in contentLoader.GetPossibleFilePaths(filePath))
            {
                entry = _fileSystem.GetFile(testFilePath);
                if (entry != null)
                {
                    break;
                }
            }

            if (entry != null)
            {
                var createdUploadBatch = false;
                if (uploadBatch == null)
                {
                    uploadBatch = new ResourceUploadBatch(GraphicsDevice);
                    uploadBatch.Begin();
                    createdUploadBatch = true;
                }

                asset = contentLoader.Load(entry, this, uploadBatch);
                if (asset is IDisposable d)
                {
                    AddDisposable(d);
                }

                if (createdUploadBatch)
                {
                    uploadBatch.End();
                }
            }
            else
            {
                asset = contentLoader.PlaceholderValue;
            }

            _cachedObjects.Add(filePath, asset);

            return (T) asset;
        }
    }
}
