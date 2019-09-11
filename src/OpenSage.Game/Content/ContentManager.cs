using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Content.Translation;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Diagnostics;
using OpenSage.FileFormats.W3d;
using OpenSage.Graphics;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Logic.Object;
using OpenSage.Utilities;
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

        // TODO: Remove this once we can load all INI files upfront.
        private readonly List<string> _alreadyLoadedIniFiles = new List<string>();

        internal IEnumerable<object> CachedObjects => _cachedObjects.Values;

        public GraphicsDevice GraphicsDevice { get; }

        public SageGame SageGame { get; }

        internal readonly StandardGraphicsResources StandardGraphicsResources;
        internal readonly ShaderResourceManager ShaderResources;

        public FileSystem FileSystem => _fileSystem;

        public IniDataContext IniDataContext { get; }

        public ITranslationManager TranslationManager { get; }

        public WndImageLoader WndImageLoader { get; }

        public FontManager FontManager { get; }

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

                IniDataContext = new IniDataContext();

                _contentScopes = new Stack<ContentScope>();
                PushScope();

                SubsystemLoader = Content.SubsystemLoader.Create(game.Definition, _fileSystem, this);

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
                        LoadIniFile(@"Data\INI\Mouse.ini");
                        LoadIniFile(@"Data\INI\Water.ini");
                        LoadIniFile(@"Data\INI\AudioSettings.ini");

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
                    { typeof(Window), AddDisposable(new WindowLoader(this, wndCallbackResolver, Language)) },
                    { typeof(AptWindow), AddDisposable(new AptLoader()) },
                };

                _cachedObjects = new Dictionary<string, object>();

                TranslationManager = Translation.TranslationManager.Instance;
                Translation.TranslationManager.LoadGameStrings(fileSystem, Language, sageGame);

                FontManager = new FontManager();

                StandardGraphicsResources = AddDisposable(new StandardGraphicsResources(graphicsDevice));

                ShaderResources = AddDisposable(new ShaderResourceManager(graphicsDevice, StandardGraphicsResources.SolidWhiteTexture));

                WndImageLoader = AddDisposable(new WndImageLoader(this));
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

        public void LoadIniFiles(string folder)
        {
            foreach (var iniFile in _fileSystem.GetFiles(folder))
            {
                LoadIniFile(iniFile);
            }
        }

        public void LoadIniFile(string filePath)
        {
            LoadIniFile(_fileSystem.GetFile(filePath));
        }

        public void LoadIniFile(FileSystemEntry entry)
        {
            using (GameTrace.TraceDurationEvent($"LoadIniFile('{entry.FilePath}'"))
            {
                if (!entry.FilePath.ToLowerInvariant().EndsWith(".ini"))
                {
                    return;
                }

                if (_alreadyLoadedIniFiles.Contains(entry.FilePath))
                {
                    return;
                }

                var parser = new IniParser(entry, this);
                parser.ParseFile();

                _alreadyLoadedIniFiles.Add(entry.FilePath);
            }
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

        public MappedImage GetMappedImage(string name)
        {
            foreach (var contentScope in _contentScopes)
            {
                if (contentScope.MappedImages.TryGetValue(name, out var result))
                {
                    return result;
                }
            }

            return null;
        }

        internal void AddMappedImage(MappedImage mappedImage)
        {
            var currentContentScope = _contentScopes.Peek();
            currentContentScope.MappedImages.TryAdd(mappedImage.Name, mappedImage);
        }

        public Model GetModel(string name)
        {
            var normalizedName = FileSystem.NormalizeFilePath(name);

            // See if it's already cached.
            foreach (var contentScope in _contentScopes)
            {
                if (contentScope.Models.TryGetResource(normalizedName, out var result))
                {
                    return result;
                }
            }

            // Find it in the file system.
            var entry = _fileSystem.GetFile(Path.Combine("art", "w3d", name + ".w3d"));

            // Load model.
            var model = ModelLoader.Load(entry, this);

            // Add it to current content scope.
            _contentScopes.Peek().Models.AddResource(
                normalizedName,
                model);

            return model;
        }

        public ModelBoneHierarchy GetModelBoneHierarchy(string name)
        {
            var normalizedName = FileSystem.NormalizeFilePath(name);

            // See if it's already cached.
            foreach (var contentScope in _contentScopes)
            {
                if (contentScope.ModelBoneHierarchies.TryGetValue(normalizedName, out var result))
                {
                    return result;
                }
            }

            // Find it in the file system.
            var entry = _fileSystem.GetFile(Path.Combine("art", "w3d", name + ".w3d"));

            // Load hierarchy.
            W3dFile hierarchyFile;
            using (var entryStream = entry.Open())
            {
                hierarchyFile = W3dFile.FromStream(entryStream, entry.FilePath);
            }
            var w3dHierarchy = hierarchyFile.GetHierarchy();
            var hierarchy = w3dHierarchy != null
                ? new ModelBoneHierarchy(w3dHierarchy)
                : ModelBoneHierarchy.CreateDefault();

            // Add it to current content scope.
            _contentScopes.Peek().ModelBoneHierarchies.Add(
                normalizedName,
                hierarchy);

            return hierarchy;
        }

        public Graphics.Animation.Animation GetAnimation(string name)
        {
            var normalizedName = FileSystem.NormalizeFilePath(name);

            // See if it's already cached.
            foreach (var contentScope in _contentScopes)
            {
                if (contentScope.Animations.TryGetValue(normalizedName, out var result))
                {
                    return result;
                }
            }

            var splitName = normalizedName.Split('.');

            if (splitName.Length <= 1)
            {
                return null;
            }

            // Find it in the file system.
            var entry = _fileSystem.GetFile(Path.Combine("art", "w3d", splitName[1] + ".w3d"));

            // Load animation.
            W3dFile w3dFile;
            using (var entryStream = entry.Open())
            {
                w3dFile = W3dFile.FromStream(entryStream, entry.FilePath);
            }
            var animation = Graphics.Animation.Animation.FromW3dFile(w3dFile);
            if (!string.Equals(animation.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException();
            }

            // Add it to current content scope.
            _contentScopes.Peek().Animations.Add(
                normalizedName,
                animation);

            return animation;
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
    }
}
