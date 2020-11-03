using System.IO;
using OpenSage.Data.Apt;

using System;
using System.Text;
using System.Linq;

namespace OpenSage.Tools.AptEditor.Apt.Writer
{
    public static class ImageMapWriter
    {
        public static byte[] Write(ImageMap imageMap)
        {
            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine($"; {Constants.OpenSageAptEditorCredits}");
                foreach(var (imageID, assigment) in imageMap.Mapping)
                {
                    writer.Write(imageID);
                    if(!(assigment.HasBounds)) // DirectAssignment
                    {
                        writer.WriteLine($"->{assigment.TextureId}");
                    }
                    else // RectangleAssignment
                    {
                        var textureRectangle = ((RectangleAssignment)assigment).TextureRectangle;
                        writer.WriteLine($"={textureRectangle.X} {textureRectangle.Y} {textureRectangle.Width} {textureRectangle.Height}");
                    }
                }
            }
            return stream.ToArray();
        }
    }
}