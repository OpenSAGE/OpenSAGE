using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript;
using Veldrid;

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

        // Most general one
        internal AptContext(AssetStore assetStore, AptFile apt, VM avm, ImageMap imageMap, string movieName)
        {
            _assetStore = assetStore;

            AptFile = apt;

            if (avm == null) avm = new VM();
                Avm = avm;
            _imageMap = imageMap;
            _movieName = movieName;

            
        }
        //constructor to be used without an apt file
        internal AptContext(ImageMap imageMap, string movieName, AssetStore assetStore) : this(assetStore, null, null, imageMap, movieName) { }

        public AptContext(AssetStore assetStore, AptFile apt, VM avm) : this(assetStore, apt, avm, null, null) { }
        public AptContext(AptWindow window) : this(window, null) { }
        public AptContext(AptWindow window, VM avm): this(window.AssetStore, window.AptFile, avm) { Window = window; }
        
        // TODO resolve dependencies?
        public AptContext LoadContext()
        {
            var movie = AptFile.Movie;

            // Data.Apt should be only containers with no calculations
            // resolve imports

            // attach initactions properly to sprites
            foreach (Character c in movie.Characters)
            {

            }

            // resolve exports

            return this;
        }
        

        //need this to handle import/export correctly
        public Character GetCharacter(int id, Character callee)
        {
            //the movie where this character is in
            var movie = callee.Container.Movie;

            return movie.Characters[id];
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
