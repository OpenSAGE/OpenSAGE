using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.IO;
using OpenSage.Mathematics;

namespace OpenSage.Data.Apt
{

    public interface IImageAssignment
    {
        bool HasBounds { get; }
        int TextureId { get; }
    }

    public class DirectAssignment : IImageAssignment
    {
        private int _textureId;

        public bool HasBounds => false;
        public int TextureId => _textureId;

        public DirectAssignment(int texture)
        {
            _textureId = texture;
        }
    }

    public class RectangleAssignment : IImageAssignment
    {
        private int _textureId;
        private Rectangle _textureRect;

        public bool HasBounds => true;
        public int TextureId => _textureId;

        public Rectangle TextureRectangle
        {
            get
            {
                return _textureRect;
            }
        }

        public RectangleAssignment(int texture, Rectangle textureRect)
        {
            _textureId = texture;
            _textureRect = textureRect;
        }
    }

    public sealed class ImageMap
    {
        public Dictionary<int, IImageAssignment> Mapping;

        public ImageMap()
        {
            Mapping = new Dictionary<int, IImageAssignment>();
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
                    var assigment = list.Last().Split(new[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

                    if (assigment.Length < 1)
                        throw new InvalidDataException();

                    var texture = Convert.ToInt32(assigment.First());

                    switch (assigment.Length)
                    {
                        case 1:
                            map.Mapping.Add(image, new DirectAssignment(texture));
                            break;
                        case 4:
                            Rectangle rect = new Rectangle(
                                Convert.ToInt32(assigment[0]),
                                Convert.ToInt32(assigment[1]),
                                Convert.ToInt32(assigment[2]),
                                Convert.ToInt32(assigment[3])
                                );

                            map.Mapping.Add(image, new RectangleAssignment(image, rect));
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
