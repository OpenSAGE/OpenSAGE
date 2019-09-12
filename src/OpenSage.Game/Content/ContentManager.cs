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

        private readonly Dictionary<string, object> _cachedObjects;

        private readonly FileSystem _fileSystem;

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
            SageGame sageGame)
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

                        // TODO: Defer subsystem loading until necessary
                        SubsystemLoader.Load(Subsystem.Audio);
                        SubsystemLoader.Load(Subsystem.Players);
                        SubsystemLoader.Load(Subsystem.ParticleSystems);
                        SubsystemLoader.Load(Subsystem.ObjectCreation);
                        SubsystemLoader.Load(Subsystem.Multiplayer);
                        SubsystemLoader.Load(Subsystem.LinearCampaign);
                        SubsystemLoader.Load(Subsystem.Wnd);
                        SubsystemLoader.Load(Subsystem.Terrain);
                        SubsystemLoader.Load(Subsystem.Credits);

                        break;

                    default:
                        break;
                }

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

        internal void LoadIniFiles(string folder)
        {
            foreach (var iniFile in _fileSystem.GetFiles(folder))
            {
                LoadIniFile(iniFile);
            }
        }

        internal void LoadIniFile(string filePath)
        {
            LoadIniFile(_fileSystem.GetFile(filePath));
        }

        internal void LoadIniFile(FileSystemEntry entry)
        {
            using (GameTrace.TraceDurationEvent($"LoadIniFile('{entry.FilePath}'"))
            {
                if (!entry.FilePath.ToLowerInvariant().EndsWith(".ini"))
                {
                    return;
                }

                var parser = new IniParser(entry, this);
                parser.ParseFile();
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
            var entry = _fileSystem.GetFile(GetW3dPath(name));

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
            var entry = _fileSystem.GetFile(GetW3dPath(name));

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
            var entry = _fileSystem.GetFile(GetW3dPath(splitName[1]));

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

        private string GetW3dPath(string name)
        {
            // TODO: Move this to IGameDefinition.
            switch (SageGame)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                case SageGame.Bfme:
                    return Path.Combine("art", "w3d", name + ".w3d");

                case SageGame.Bfme2:
                case SageGame.Bfme2Rotwk:
                    return Path.Combine("art", "w3d", name.Substring(0, 2), name + ".w3d");

                default:
                    throw new NotImplementedException();
            }
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
