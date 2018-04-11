using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Audio;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Data.Wav;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Logic.Object;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;
using SixLabors.Fonts;
using Veldrid;

namespace OpenSage.Content
{
    public sealed class ContentManager : DisposableBase
    {
        private readonly Game _game;

        private readonly Dictionary<Type, ContentLoader> _contentLoaders;

        private readonly Dictionary<string, object> _cachedObjects;

        private readonly FileSystem _fileSystem;

        private readonly Dictionary<FontKey, Font> _cachedFonts;

        private readonly Dictionary<uint, DeviceBuffer> _cachedNullStructuredBuffers;

        public GraphicsDevice GraphicsDevice { get; }

        public SageGame SageGame { get; }

        public EffectLibrary EffectLibrary { get; }

        public Sampler LinearClampSampler { get; }
        public Sampler PointClampSampler { get; }

        public Texture NullTexture { get; }

        public Texture SolidWhiteTexture { get; }

        public FileSystem FileSystem => _fileSystem;

        public IniDataContext IniDataContext { get; }

        public TranslationManager TranslationManager { get; }

        internal WndImageLoader WndImageLoader { get; }

        public ContentManager(
            Game game,
            FileSystem fileSystem,
            GraphicsDevice graphicsDevice,
            SageGame sageGame,
            WndCallbackResolver wndCallbackResolver)
        {
            _game = game;
            _fileSystem = fileSystem;

            GraphicsDevice = graphicsDevice;

            SageGame = sageGame;

            IniDataContext = new IniDataContext(fileSystem);

            IniDataContext.LoadIniFiles(@"Data\INI\Default\Object.ini");
            IniDataContext.LoadIniFiles(@"Data\INI\Object");

            _contentLoaders = new Dictionary<Type, ContentLoader>
            {
                { typeof(Model), AddDisposable(new ModelLoader()) },
                { typeof(Scene3D), AddDisposable(new MapLoader()) },
                { typeof(Texture), AddDisposable(new TextureLoader(graphicsDevice)) },
                { typeof(Window), AddDisposable(new WindowLoader(this, wndCallbackResolver)) },
                { typeof(AptWindow), AddDisposable(new AptLoader()) },
                { typeof(WavFile), AddDisposable(new WavLoader()) },
            };

            _cachedObjects = new Dictionary<string, object>();

            EffectLibrary = AddDisposable(new EffectLibrary(graphicsDevice));

            TranslationManager = new TranslationManager(fileSystem, sageGame);

            _cachedFonts = new Dictionary<FontKey, Font>();

            var linearClampSamplerDescription = SamplerDescription.Linear;
            linearClampSamplerDescription.AddressModeU = SamplerAddressMode.Clamp;
            linearClampSamplerDescription.AddressModeV = SamplerAddressMode.Clamp;
            linearClampSamplerDescription.AddressModeW = SamplerAddressMode.Clamp;
            LinearClampSampler = AddDisposable(
                graphicsDevice.ResourceFactory.CreateSampler(ref linearClampSamplerDescription));

            var pointClampSamplerDescription = SamplerDescription.Point;
            pointClampSamplerDescription.AddressModeU = SamplerAddressMode.Clamp;
            pointClampSamplerDescription.AddressModeV = SamplerAddressMode.Clamp;
            pointClampSamplerDescription.AddressModeW = SamplerAddressMode.Clamp;
            PointClampSampler = AddDisposable(
                graphicsDevice.ResourceFactory.CreateSampler(ref pointClampSamplerDescription));

            NullTexture = AddDisposable(graphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D(1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled)));

            _cachedNullStructuredBuffers = new Dictionary<uint, DeviceBuffer>();

            SolidWhiteTexture = AddDisposable(graphicsDevice.CreateStaticTexture2D(
                1,
                1,
                new TextureMipMapData(
                    new byte[] { 255, 255, 255, 255 },
                    4, 4, 1, 1),
                PixelFormat.R8_G8_B8_A8_UNorm));

            WndImageLoader = AddDisposable(new WndImageLoader(this, new MappedImageLoader(this)));
        }

        internal DeviceBuffer GetNullStructuredBuffer(uint size)
        {
            if (!_cachedNullStructuredBuffers.TryGetValue(size, out var result))
            {
                _cachedNullStructuredBuffers.Add(size, result = AddDisposable(GraphicsDevice.ResourceFactory.CreateBuffer(
                    new BufferDescription(
                        size,
                        BufferUsage.StructuredBufferReadOnly,
                        size))));
            }
            return result;
        }

        public void Unload()
        {
            foreach (var cachedObject in _cachedObjects.Values)
            {
                if (cachedObject is IDisposable d)
                {
                    RemoveAndDispose(ref d);
                }
            }
            _cachedObjects.Clear();
        }

        public T Load<T>(
            string[] filePaths,
            LoadOptions options = null,
            bool fallbackToPlaceholder = true)
            where T : class
        {
            for (var i = 0; i < filePaths.Length; i++)
            {
                var actuallyFallbackToPlaceholder = fallbackToPlaceholder && i == filePaths.Length - 1;

                var result = Load<T>(filePaths[i], options, actuallyFallbackToPlaceholder);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public T Load<T>(
            string filePath,
            LoadOptions options = null,
            bool fallbackToPlaceholder = true)
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
                asset = contentLoader.Load(entry, this, _game, options);

                if (asset is IDisposable d)
                {
                    AddDisposable(d);
                }

                var shouldCacheAsset = options?.CacheAsset ?? true;
                if (shouldCacheAsset)
                {
                    _cachedObjects.Add(filePath, asset);
                }
            }
            else if (fallbackToPlaceholder)
            {
                asset = contentLoader.PlaceholderValue;
            }

            GraphicsDevice.WaitForIdle();

            return (T) asset;
        }

        public GameObject InstantiateObject(string typeName)
        {
            var objectDefinition = IniDataContext.Objects.FirstOrDefault(x => x.Name == typeName);
            if (objectDefinition != null)
            {
                return new GameObject(objectDefinition, this);
            }
            else
            {
                // TODO
                return null;
            }
        }

        public Font GetOrCreateFont(string fontName, float fontSize, FontWeight fontWeight)
        {
            var key = new FontKey
            {
                FontName = fontName,
                FontSize = fontSize,
                FontWeight = fontWeight
            };

            if (!_cachedFonts.TryGetValue(key, out var font))
            {
                if (!SystemFonts.TryFind(fontName, out var fontFamily))
                {
                    fontName = "Arial";
                }

                var fontStyle = fontWeight == FontWeight.Bold
                    ? FontStyle.Bold
                    : FontStyle.Regular;

                _cachedFonts.Add(key, font = SystemFonts.CreateFont(
                    fontName,
                    fontSize,
                    fontStyle));
            }

            return font;
        }

        private struct FontKey
        {
            public string FontName;
            public float FontSize;
            public FontWeight FontWeight;
        }
    }
}
