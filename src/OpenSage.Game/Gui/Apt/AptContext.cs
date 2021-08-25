using OpenSage.Content;
using OpenSage.FileFormats.Apt;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Data;
using Veldrid;
using System.Collections.Generic;
using OpenSage.FileFormats.Apt.FrameItems;
using System;
using System.IO;

namespace OpenSage.Gui.Apt
{
    public sealed class AptContext
    {
        private readonly AssetStore _assetStore;
        private readonly ImageMap _imageMap;
        private readonly string _movieName;

        public VM Avm { get; }

        // TODO: Should be deprecated or implemented in other way. This one causes nothing but mess.
        public AptWindow Window { get; }
        public AptFile AptFile { get; }
        public ConstantData Constants => AptFile.Constants;
        public uint MsPerFrame => AptFile != null ? AptFile.Movie.MillisecondsPerFrame : MsPerFrameDefault; // Java style. Any fancier implementations?
        public static readonly uint MsPerFrameDefault = 30;
        public SpriteItem Root { get; set; }

        public Dictionary<int, (string, string)> ImportDict { get; private set; }
        public Dictionary<string, int> ExportDict { get; private set; }
        public Dictionary<string, AptContext> ImportContextDict { get; private set; }
        public Dictionary<int, InstructionCollection> InitActionsDict { get; private set; }

        // The general constructor
        public AptContext(AptWindow window, AptFile file = null, VM avm = null) {
            Window = window;
            _assetStore = window.AssetStore;
            if (file == null) file = window == null ? null : window.AptFile;
            AptFile = file;
            if (avm == null) avm = new VM(this);
            Avm = avm;
        }

        // Constructor to be used without an apt file
        internal AptContext(ImageMap imageMap, string movieName, AssetStore assetStore) {
            _assetStore = assetStore;
            _imageMap = imageMap;
            _movieName = movieName;
        }

        public AptContext LoadContext()
        {
            var movie = AptFile.Movie;

            // Data.Apt should be only containers with no calculations
            // resolve imports

            ImportContextDict = new Dictionary<string, AptContext>();

            foreach (var import in AptFile.Movie.Imports)
            {
                //open the apt file where our character is located
                var importPath = Path.Combine(AptFile.RootDirectory, Path.ChangeExtension(import.Movie, ".apt"));
                var importFile = AptFile.FromPath(importPath, AptFile.StreamGetter);
                var importContext = new AptContext(Window, importFile, Avm)
                {
                    Root = Root,
                };
                importContext.LoadContext();
                ImportContextDict[import.Movie] = importContext;
            }

            // resolve initactions

            InitActionsDict = new Dictionary<int, InstructionCollection>();

            // find all initactions
            // cover the old one if sprite id is repeated
            // this follows the file spec
            foreach (var v in movie.Frames)
                foreach (var act in v.FrameItems)
                    if (act is InitAction iact)
                    {
                        var sid = (int) iact.Sprite;
                        if (movie.Characters.Count <= sid)
                            throw new InvalidDataException("Initactions should be attached to existing Sprites.");
                        else if (InitActionsDict.TryGetValue(sid, out var _))
                            throw new InvalidDataException("Initactions should onle be attached to Sprites.");
                        else
                            InitActionsDict[sid] = InstructionCollection.Parse(iact.Instructions);
                    }
            /*
            foreach (var ia in InitActionsDict)
            {
                var spr = movie.Characters[ia.Key];
                if (spr is Sprite sprite)
                    sprite.InitActions = ia.Value;
                else
                    throw new InvalidDataException("Initactions should onle be attached to Sprites.");
            }
            */

            // resolve imports and exports

            ImportDict = new Dictionary<int, (string, string)>();
            ExportDict = new Dictionary<string, int>();
            foreach (var import in movie.Imports)
                ImportDict[(int) import.Character] = (import.Name, import.Movie);
            foreach (var export in movie.Exports)
                ExportDict[export.Name] = (int) export.Character;
            
            // execute initactions

            foreach (var a in InitActionsDict)
            {
                Avm.EnqueueContext(a.Value, this, null, $"Initaction #{a.Key}");
            }

            return this;
        }
        

        //need this to handle import/export correctly
        public Character GetCharacter(int id, Character callee)
        {
            //the movie where this character is in
            var movie = callee.Container.Movie;

            return movie.Characters[id];
        }

        public DisplayItem GetInstantiatedCharacter(int id, ItemTransform initState, SpriteItem parent = null)
        {
            if (ImportDict.ContainsKey(id))
            {
                var import_context = ImportContextDict[ImportDict[id].Item2];
                return import_context.GetInstantiatedCharacter(import_context.ExportDict[ImportDict[id].Item1], initState, parent);
            }
            var chr = AptFile.Movie.Characters[id];
            DisplayItem displayItem = chr switch
            {
                Playable _ => new SpriteItem(),
                Button _ => new ButtonItem(),
                _ => new RenderItem(),
            };
            displayItem.Transform = initState;
            displayItem.Create(chr, this, parent);
            return displayItem;
        }

        public Geometry GetGeometry(uint id, Character callee)
        {
            var apt = callee.Container;

            return apt.GeometryMap[id];
        }

        public Texture GetTexture(int id, Character callee)
        {
            var aptFile = callee.Container;
            //TODO: properly implement rectangle assignment
            var texId = aptFile.ImageMap.Mapping[id].TextureId;
            var movieName = aptFile.MovieName;
            var textureFileName = "apt_" + movieName + "_" + texId.ToString() + ".tga";
            return _assetStore.GuiTextures.GetByName(textureFileName);
        }

        public Texture GetTexture(int id, Geometry geom)
        {
            //TODO: properly implement rectangle assignment
            ImageMap map;
            string movieName;
            if (geom.Container != null)
            {
                map = geom.Container.ImageMap;
                movieName = geom.Container.MovieName;
            }
            else
            {
                map = _imageMap;
                movieName = _movieName;
            }

            var texId = map.Mapping[id].TextureId;
            var textureFileName = "apt_" + movieName + "_" + texId.ToString() + ".tga";
            return _assetStore.GuiTextures.GetByName(textureFileName);
        }
    }
}
