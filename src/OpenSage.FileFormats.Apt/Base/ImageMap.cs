using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.Apt
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

    public sealed class ImageMap　: IMemoryStorage
    {
        public Dictionary<int, IImageAssignment> Mapping;

        public ImageMap()
        {
            Mapping = new Dictionary<int, IImageAssignment>();
        }

        public static ImageMap Parse(StreamReader reader)
        {
            var map = new ImageMap();
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

            return map;
        }

        public void Write(BinaryWriter writer, BinaryMemoryChain pool)
        {
            var stream = new MemoryStream();
            using (var writer2 = new StreamWriter(stream))
            {
                writer2.WriteLine($"; {Constants.OpenSageAptEditorCredits}");
                foreach (var (imageID, assigment) in Mapping)
                {
                    writer2.Write(imageID);
                    if (!(assigment.HasBounds)) // DirectAssignment
                    {
                        writer2.WriteLine($"->{assigment.TextureId}");
                    }
                    else // RectangleAssignment
                    {
                        var textureRectangle = ((RectangleAssignment) assigment).TextureRectangle;
                        writer2.WriteLine($"={textureRectangle.X} {textureRectangle.Y} {textureRectangle.Width} {textureRectangle.Height}");
                    }
                }
            }
            writer.Write(stream.ToArray());
        }
    }
}
