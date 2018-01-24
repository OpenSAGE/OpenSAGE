﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Wnd;
using SixLabors.Fonts;

namespace OpenSage.Content
{
    public sealed class ContentManager : GraphicsObject
    {
        private readonly Dictionary<Type, ContentLoader> _contentLoaders;

        private readonly Dictionary<string, object> _cachedObjects;

        private readonly FileSystem _fileSystem;

        private readonly Dictionary<FontKey, Font> _cachedFonts;

        public GraphicsDevice GraphicsDevice { get; }

        public SageGame SageGame { get; }

        public EffectLibrary EffectLibrary { get; }

        public FileSystem FileSystem => _fileSystem;

        public IniDataContext IniDataContext { get; }

        public TranslationManager TranslationManager { get; }

        public ContentManager(
            FileSystem fileSystem,
            GraphicsDevice graphicsDevice,
            SageGame sageGame,
            WndCallbackResolver wndCallbackResolver)
        {
            _fileSystem = fileSystem;

            GraphicsDevice = graphicsDevice;

            SageGame = sageGame;

            IniDataContext = new IniDataContext(fileSystem);

            _contentLoaders = new Dictionary<Type, ContentLoader>
            {
                { typeof(Model), AddDisposable(new ModelLoader()) },
                { typeof(Scene), AddDisposable(new MapLoader()) },
                { typeof(Texture), AddDisposable(new TextureLoader(graphicsDevice)) },
                { typeof(WndTopLevelWindow), AddDisposable(new WindowLoader(this, wndCallbackResolver)) },
                { typeof(ShapeComponent), AddDisposable(new ShapeLoader(this)) },
                { typeof(AptComponent), AddDisposable(new AptLoader(this)) }

            };

            _cachedObjects = new Dictionary<string, object>();

            EffectLibrary = AddDisposable(new EffectLibrary(graphicsDevice));

            TranslationManager = new TranslationManager(fileSystem);

            _cachedFonts = new Dictionary<FontKey, Font>();
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
                asset = contentLoader.Load(entry, this, options);

                if (asset is IDisposable d)
                {
                    AddDisposable(d);
                }

                _cachedObjects.Add(filePath, asset);
            }
            else if (fallbackToPlaceholder)
            {
                asset = contentLoader.PlaceholderValue;
            }

            return (T) asset;
        }

        public Entity InstantiateObject(string typeName)
        {
            // TODO: Don't do this every time.
            IniDataContext.LoadIniFiles(@"Data\INI\Object");

            var objectDefinition = IniDataContext.Objects.FirstOrDefault(x => x.Name == typeName);
            if (objectDefinition != null)
            {
                return Entity.FromObjectDefinition(objectDefinition);
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
