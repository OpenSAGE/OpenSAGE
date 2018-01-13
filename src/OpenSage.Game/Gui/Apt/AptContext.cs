using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Content;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.Gui.Apt
{
    public sealed class AptContext
    {
        private ContentManager _contentManager;
        private ImageMap _imageMap;
        private string _movieName;

        public ConstantData Constants { get; set; }
        //Time per frame in milliseconds
        public uint MillisecondsPerFrame { get; set; }

        public AptContext(AptFile apt, ContentManager contentManager)
        {
            MillisecondsPerFrame = apt.Movie.MillisecondsPerFrame;
            Constants = apt.Constants;

            _contentManager = contentManager;
        }

        //constructor to be used without an apt file
        public AptContext(ImageMap imageMap, string movieName,ContentManager contentManager)
        {
            _contentManager = contentManager;
            _imageMap = imageMap;
            _movieName = movieName;
        }

        //need this to handle import/export correctly
        public Character GetCharacter(int id,Character callee)
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
            return _contentManager.Load<Texture>(texturePath, loadOptions);
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
            return _contentManager.Load<Texture>(texturePath, loadOptions);
        }
    }
}
