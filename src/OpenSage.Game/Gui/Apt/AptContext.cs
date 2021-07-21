using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript;
using Veldrid;
using System.Collections.Generic;
using OpenSage.Data.Apt.FrameItems;
using System;

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

        // The general constructor
        public AptContext(AptWindow window, VM avm = null) {
            Window = window;
            _assetStore = window.AssetStore;
            AptFile = window.AptFile;

            if (avm == null) avm = new VM();
            Avm = avm;
        }

        // Constructor to be used without an apt file
        internal AptContext(ImageMap imageMap, string movieName, AssetStore assetStore) {
            _assetStore = assetStore;
            _imageMap = imageMap;
            _movieName = movieName;
        }
        
        // TODO resolve dependencies?
        public AptContext LoadContext()
        {
            var movie = AptFile.Movie;
            var global = Avm.GlobalObject;
            var extobj = Avm.ExternObject;

            var initactions = new Dictionary<uint, InstructionCollection>();

            // Data.Apt should be only containers with no calculations
            // resolve imports
            foreach (Import import in movie.Imports)
            {
                AptFile af = null;
            }

            // find all initactions
            // cover the old one if sprite id is repeated
            // this follows the file spec
            foreach (var v in movie.Frames)
                foreach (var act in v.FrameItems)
                    if (act is InitAction iact)
                        initactions[iact.Sprite] = iact.Instructions;

            foreach (var iact in initactions)
            {

            }

            // resolve exports
            foreach (Export export in movie.Exports)
            {
                var chrname = export.Name;
                var character = movie.Characters[(int)export.Character];
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

        public DisplayItem GetInstantiatedCharacter(int id)
        {
            throw new NotImplementedException();
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
