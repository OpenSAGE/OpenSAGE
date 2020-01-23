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

        public AptWindow Window { get; }
        public ConstantData Constants => Window.AptFile.Constants;
        //Time per frame in milliseconds
        public uint MillisecondsPerFrame => Window.AptFile.Movie.MillisecondsPerFrame;
        public SpriteItem Root { get; set; }

        public AptContext(AptWindow window)
        {
            Window = window;
            _assetStore = window.AssetStore;

            Avm = new VM();
        }

        //constructor to be used without an apt file
        internal AptContext(ImageMap imageMap, string movieName, AssetStore assetStore)
        {
            _assetStore = assetStore;
            _imageMap = imageMap;
            _movieName = movieName;
            Avm = new VM();
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
