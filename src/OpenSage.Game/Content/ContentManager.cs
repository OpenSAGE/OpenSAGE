using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenSage.Data;
using OpenSage.Data.Ini;
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

        public ISubsystemLoader SubsystemLoader { get; }

        private readonly Dictionary<Type, ContentLoader> _contentLoaders;

        private readonly Dictionary<string, object> _cachedObjects;

        private readonly FileSystem _fileSystem;

        private readonly Dictionary<FontKey, Font> _cachedFonts;

        private readonly string _fallbackSystemFont = "Arial";
        private readonly string _fallbackEmbeddedFont = "Roboto";

        private readonly Dictionary<uint, DeviceBuffer> _cachedNullStructuredBuffers;

        private FontCollection _fallbackFonts;

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

        public WndImageLoader WndImageLoader { get; }

        public string Language { get; }

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

            Language = LanguageUtility.ReadCurrentLanguage(game.Definition, fileSystem.RootDirectory);

            IniDataContext = new IniDataContext(fileSystem, sageGame);

            SubsystemLoader = Content.SubsystemLoader.Create(game.Definition, _fileSystem, IniDataContext);

            switch (sageGame)
            {
                // Only load these INI files for a subset of games, because we can't parse them for others yet.
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                case SageGame.Bfme:
                case SageGame.Bfme2:
                case SageGame.Bfme2Rotwk:
                    SubsystemLoader.Load(Subsystem.Core);

                    // TODO: Move this somewhere else.
                    // Subsystem.Core should load mouse config, but that isn't the case with at least BFME2.
                    IniDataContext.LoadIniFile(@"Data\INI\Mouse.ini");

                    break;
                default:
                    break;
            }

            // TODO: Defer subsystem loading until necessary
            switch (sageGame)
            {
                // Only load these INI files for a subset of games, because we can't parse them for others yet.
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                case SageGame.Bfme:
                    SubsystemLoader.Load(Subsystem.Players);
                    SubsystemLoader.Load(Subsystem.ParticleSystems);
                    SubsystemLoader.Load(Subsystem.ObjectCreation);
                    SubsystemLoader.Load(Subsystem.Multiplayer);
                    break;
                default:
                    break;
            }

            _contentLoaders = new Dictionary<Type, ContentLoader>
            {
                { typeof(Model), AddDisposable(new ModelLoader()) },
                { typeof(Scene3D), AddDisposable(new MapLoader()) },
                { typeof(Texture), AddDisposable(new TextureLoader(graphicsDevice)) },
                { typeof(Window), AddDisposable(new WindowLoader(this, wndCallbackResolver, Language)) },
                { typeof(AptWindow), AddDisposable(new AptLoader()) },
            };

            _cachedObjects = new Dictionary<string, object>();

            EffectLibrary = AddDisposable(new EffectLibrary(graphicsDevice));

            TranslationManager = new TranslationManager(fileSystem, sageGame, Language);

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
                1, 1, 1,
                new TextureMipMapData(
                    new byte[] { 255, 255, 255, 255 },
                    4, 4, 1, 1),
                PixelFormat.R8_G8_B8_A8_UNorm));

            WndImageLoader = AddDisposable(new WndImageLoader(this, new MappedImageLoader(this)));

            _fallbackFonts = new FontCollection();
            var assembly = Assembly.GetExecutingAssembly();
            var fontStream = assembly.GetManifestResourceStream($"OpenSage.Content.Fonts.{_fallbackEmbeddedFont}-Regular.ttf");
            _fallbackFonts.Install(fontStream);
            fontStream = assembly.GetManifestResourceStream($"OpenSage.Content.Fonts.{_fallbackEmbeddedFont}-Bold.ttf");
            _fallbackFonts.Install(fontStream);
        }

        internal DeviceBuffer GetNullStructuredBuffer(uint size)
        {
            if (!_cachedNullStructuredBuffers.TryGetValue(size, out var result))
            {
                _cachedNullStructuredBuffers.Add(size, result = AddDisposable(GraphicsDevice.ResourceFactory.CreateBuffer(
                    new BufferDescription(
                        size,
                        BufferUsage.StructuredBufferReadOnly,
                        size,
                        true))));
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
                bool embeddedFallback = false;

                if (!SystemFonts.TryFind(fontName, out var fontFamily))
                {
                    //First try to load a fallback system font (Arial)
                    if (SystemFonts.TryFind(_fallbackSystemFont, out fontFamily))
                    {
                        fontName = _fallbackSystemFont;
                    }
                    //If this fails use an embedded fallback font (Roboto)
                    else
                    {
                        embeddedFallback = true;
                    }
                }

                var fontStyle = fontWeight == FontWeight.Bold
                    ? FontStyle.Bold
                    : FontStyle.Regular;

                if (!embeddedFallback)
                {
                    font = SystemFonts.CreateFont(fontName,
                                                fontSize,
                                                fontStyle);
                }
                else
                {
                    font = _fallbackFonts.CreateFont(_fallbackEmbeddedFont,
                                                    fontSize,
                                                    fontStyle);
                }
                _cachedFonts.Add(key, font);
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
