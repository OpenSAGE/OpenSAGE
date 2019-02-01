using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript;
using Veldrid;

namespace OpenSage.Gui.Apt
{
    public sealed class AptContext
    {
        private readonly ImageMap _imageMap;
        private readonly string _movieName;

        public VM Avm { get; }

        public AptWindow Window { get; }
        public ConstantData Constants => Window.AptFile.Constants;
        //Time per frame in milliseconds
        public uint MillisecondsPerFrame => Window.AptFile.Movie.MillisecondsPerFrame;
        public ContentManager ContentManager { get; }
        public SpriteItem Root { get; set; }

        public AptContext(AptWindow window)
        {
            Window = window;
            ContentManager = window.ContentManager;

            Avm = new VM();
        }

        //constructor to be used without an apt file
        public AptContext(ImageMap imageMap, string movieName, ContentManager contentManager)
        {
            ContentManager = contentManager;
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
            var texturePath = "art/Textures/apt_" + movieName + "_" + texId.ToString() + ".tga";
            var loadOptions = new TextureLoadOptions() { GenerateMipMaps = false };
            return ContentManager.Load<Texture>(texturePath, loadOptions);
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
            var texturePath = "art/Textures/apt_" + movieName + "_" + texId.ToString() + ".tga";
            var loadOptions = new TextureLoadOptions() { GenerateMipMaps = false };
            return ContentManager.Load<Texture>(texturePath, loadOptions);
        }
    }
}
