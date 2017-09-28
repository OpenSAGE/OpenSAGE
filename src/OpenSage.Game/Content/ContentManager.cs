using System;
using System.Collections.Generic;
using LLGfx;
using OpenSage.Data;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;

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

        public ContentManager(FileSystem fileSystem, GraphicsDevice graphicsDevice)
        {
            _fileSystem = fileSystem;
            GraphicsDevice = graphicsDevice;

            _contentLoaders = new Dictionary<Type, ContentLoader>
            {
                { typeof(Entity), AddDisposable(new ModelLoader()) },
                { typeof(Texture), AddDisposable(new TextureLoader(graphicsDevice)) }
            };

            _cachedObjects = new Dictionary<string, object>();

            _effects = new Dictionary<Type, Effect>
            {
                { typeof(MeshEffect), AddDisposable(new MeshEffect(graphicsDevice)) },
                { typeof(SpriteEffect), AddDisposable(new SpriteEffect(graphicsDevice)) }
            };
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
