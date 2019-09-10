using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenSage.Content.Translation;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Diagnostics;
using OpenSage.Graphics;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Logic.Object;
using OpenSage.Utilities;
using SixLabors.Fonts;
using Veldrid;

namespace OpenSage.Content
{
    public sealed class ContentManager : DisposableBase
    {
        private readonly Game _game;
        private readonly Stack<ContentScope> _contentScopes;

        public ISubsystemLoader SubsystemLoader { get; }

        private readonly Dictionary<Type, ContentLoader> _contentLoaders;

        private readonly Dictionary<string, object> _cachedObjects;

        private readonly FileSystem _fileSystem;

        private readonly Dictionary<FontKey, Font> _cachedFonts;

        private readonly string _fallbackSystemFont = "Arial";
        private readonly string _fallbackEmbeddedFont = "Roboto";

        private FontCollection _fallbackFonts;

        internal IEnumerable<object> CachedObjects => _cachedObjects.Values;

        public GraphicsDevice GraphicsDevice { get; }

        public SageGame SageGame { get; }

        internal readonly StandardGraphicsResources StandardGraphicsResources;
        internal readonly ShaderResourceManager ShaderResources;

        public FileSystem FileSystem => _fileSystem;

        public IniDataContext IniDataContext { get; }

        /// <summary>
        /// Eventually all game data will live here, and be scoped to global or map-specific.
        /// </summary>
        public DataContext DataContext { get; }

        public ITranslationManager TranslationManager { get; }

        public WndImageLoader WndImageLoader { get; }

        public string Language { get; }

        public ContentManager(
            Game game,
            FileSystem fileSystem,
            GraphicsDevice graphicsDevice,
            SageGame sageGame,
            WndCallbackResolver wndCallbackResolver)
        {
            using (GameTrace.TraceDurationEvent("ContentManager()"))
            {
                _game = game;
                _fileSystem = fileSystem;

                GraphicsDevice = graphicsDevice;

                SageGame = sageGame;

                Language = LanguageUtility.ReadCurrentLanguage(game.Definition, fileSystem.RootDirectory);

                IniDataContext = new IniDataContext(fileSystem, sageGame);

                DataContext = new DataContext();

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
                        // Subsystem.Core should load mouse and water config, but that isn't the case with at least BFME2.
                        IniDataContext.LoadIniFile(@"Data\INI\Mouse.ini");
                        IniDataContext.LoadIniFile(@"Data\INI\Water.ini");
                        IniDataContext.LoadIniFile(@"Data\INI\AudioSettings.ini");

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
                    case SageGame.Bfme2:
                    case SageGame.Bfme2Rotwk:
                        SubsystemLoader.Load(Subsystem.Players);
                        SubsystemLoader.Load(Subsystem.ParticleSystems);
                        SubsystemLoader.Load(Subsystem.ObjectCreation);
                        SubsystemLoader.Load(Subsystem.Multiplayer);
                        SubsystemLoader.Load(Subsystem.LinearCampaign);
                        break;
                    default:
                        break;
                }

                _contentLoaders = new Dictionary<Type, ContentLoader>
                {
                    { typeof(Model), AddDisposable(new ModelLoader()) },
                    { typeof(Window), AddDisposable(new WindowLoader(this, wndCallbackResolver, Language)) },
                    { typeof(AptWindow), AddDisposable(new AptLoader()) },
                };

                _cachedObjects = new Dictionary<string, object>();

                TranslationManager = Translation.TranslationManager.Instance;
                Translation.TranslationManager.LoadGameStrings(fileSystem, Language, sageGame);

                _cachedFonts = new Dictionary<FontKey, Font>();

                StandardGraphicsResources = AddDisposable(new StandardGraphicsResources(graphicsDevice));

                ShaderResources = AddDisposable(new ShaderResourceManager(graphicsDevice, StandardGraphicsResources.SolidWhiteTexture));

                WndImageLoader = AddDisposable(new WndImageLoader(this, new MappedImageLoader(this)));

                _fallbackFonts = new FontCollection();
                var assembly = Assembly.GetExecutingAssembly();
                var fontStream = assembly.GetManifestResourceStream($"OpenSage.Content.Fonts.{_fallbackEmbeddedFont}-Regular.ttf");
                _fallbackFonts.Install(fontStream);
                fontStream = assembly.GetManifestResourceStream($"OpenSage.Content.Fonts.{_fallbackEmbeddedFont}-Bold.ttf");
                _fallbackFonts.Install(fontStream);

                _contentScopes = new Stack<ContentScope>();
                PushScope();
            }
        }

        public void PushScope()
        {
            _contentScopes.Push(AddDisposable(new ContentScope()));
        }

        public void PopScope()
        {
            var contentScope = _contentScopes.Pop();
            RemoveAndDispose(ref contentScope);
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

        public Texture GetTexture(string fileName)
        {
            return GetTexture(
                fileName,
                true,
                normalizedFileName =>
                {
                    // TODO: Move this to IGameDefinition.
                    switch (SageGame)
                    {
                        case SageGame.CncGenerals:
                        case SageGame.CncGeneralsZeroHour:
                        case SageGame.Bfme:
                            return new[] { Path.Combine("art", "textures", normalizedFileName) };

                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            return new[] { Path.Combine("art", "compiledtextures", normalizedFileName.Substring(0, 2), normalizedFileName) };

                        default:
                            throw new NotImplementedException();
                    }
                });
        }

        public Texture GetGuiTexture(string fileName)
        {
            return GetTexture(
                fileName,
                false,
                // TODO: Figure out which games need which paths and move this to IGameDefinition.
                normalizedFileName => new[]
                {
                    Path.Combine("data", Language.ToLowerInvariant(), "art", "textures", normalizedFileName),
                    Path.Combine("lang", Language.ToLowerInvariant(), "art", "textures", normalizedFileName),
                    Path.Combine("art", "textures", normalizedFileName),
                    Path.Combine("art", "compiledtextures",  normalizedFileName.Substring(0,2), normalizedFileName)
                });
        }

        public Texture GetGuiTextureFromPath(string filePath)
        {
            return GetTexture(
                filePath,
                false,
                normalizedFilePath => new[] { normalizedFilePath });
        }

        public Texture GetAptTexture(string fileName)
        {
            return GetTexture(
                fileName,
                false,
                normalizedFileName => new[] { Path.Combine("art", "textures", normalizedFileName) });
        }

        private Texture GetTexture(string fileName, bool generateMipMaps, Func<string, string[]> getPaths)
        {
            var normalizedFileName = FileSystem.NormalizeFilePath(fileName);

            // See if it's already cached.
            foreach (var contentScope in _contentScopes)
            {
                if (contentScope.Textures.TryGetResource(normalizedFileName, out var result))
                {
                    return result;
                }
            }

            // Find it in the file system.
            FileSystemEntry entry = null;
            foreach (var path in getPaths(normalizedFileName))
            {
                foreach (var possibleFilePath in TextureLoader.GetPossibleFilePaths(path))
                {
                    entry = _fileSystem.GetFile(possibleFilePath);
                    if (entry != null)
                    {
                        break;
                    }
                }

                if (entry != null)
                {
                    break;
                }
            }

            var foundTexture = entry != null;

            // Load texture.
            var texture = foundTexture
                ? TextureLoader.Load(entry, GraphicsDevice, generateMipMaps)
                : StandardGraphicsResources.PlaceholderTexture;

            // Add it to current content scope.
            _contentScopes.Peek().Textures.AddResource(
                normalizedFileName,
                texture,
                foundTexture);

            return texture;
        }

        public T Load<T>(
            string filePath,
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

            GraphicsDevice.WaitForIdle();

            return (T) asset;
        }

        public GameObject InstantiateObject(string typeName, GameObjectCollection parent)
        {
            var objectDefinition = IniDataContext.Objects.FirstOrDefault(x => x.Name == typeName);
            if (objectDefinition != null)
            {
                return new GameObject(objectDefinition, this, _game.CivilianPlayer, parent);
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
                var embeddedFallback = false;

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
