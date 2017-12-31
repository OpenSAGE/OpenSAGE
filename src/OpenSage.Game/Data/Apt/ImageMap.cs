using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Mathematics;

namespace OpenSage.Data.Apt
{

    public interface ImageAssignment
    {
        bool HasBounds { get; }
        int TextureId { get; }
    }

    public struct DirectAssignment : ImageAssignment
    {
        private int _textureId;

        public bool HasBounds
        {
            get
            {
                return false;
            }
        }

        public int TextureId
        {
            get
            {
                return _textureId;
            }
        }

        public DirectAssignment(int texture)
        {
            _textureId = texture;
        }
    }

    public struct RectangleAssignment : ImageAssignment
    {
        private int _textureId;
        private Rectangle _textureRect;

        public bool HasBounds
        {
            get
            {
                return true;
            }
        }

        public int TextureId
        {
            get
            {
                return _textureId;
            }
        }

        public Rectangle TextureRectangle
        {
            get
            {
                return _textureRect;
            }
        }

        public RectangleAssignment(int texture,Rectangle textureRect)
        {
            _textureId = texture;
            _textureRect = textureRect;
        }
    }

    public sealed class ImageMap
    {
        public Dictionary<int, ImageAssignment> Mapping;

        public ImageMap()
        {
            Mapping = new Dictionary<int, ImageAssignment>();
        }

        public static ImageMap FromFileSystemEntry(FileSystemEntry entry)
        {
            var map = new ImageMap();

            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //process line

                    //skip comments
                    line = line.Split(';').First();

                    if (line.Length == 0)
                        continue;

                    var seperators = new string[] { "->", "=" };

                    var list = line.Split(seperators, StringSplitOptions.None);

                    if (list.Length > 2)
                        throw new InvalidDataException();

                    //first entry in list is the internal image id
                    var image = Convert.ToInt32(list.First());

                    //second entry is the texture id and optionally bounds
                    var assigment = list.Last().Split(' ');

                    if (assigment.Length < 1)
                        throw new InvalidDataException();

                    var texture = Convert.ToInt32(assigment.First());

                    switch (assigment.Length)
                    {
                        case 1:
                            map.Mapping.Add(image,new DirectAssignment(texture));
                            break;
                        case 5:
                            Rectangle rect = new Rectangle(
                                Convert.ToInt32(assigment[1]),
                                Convert.ToInt32(assigment[2]),
                                Convert.ToInt32(assigment[3]),
                                Convert.ToInt32(assigment[4])
                                );

                            map.Mapping.Add(image, new RectangleAssignment(texture,rect));
                            break;
                        default:
                            throw new InvalidDataException();
                    }
                }
            }
            return map;
        }
    }
}
