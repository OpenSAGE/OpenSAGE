using System.IO;

namespace OpenSage.Data.Apt.Characters
{
    public sealed class Image : Character
    {
        public uint TextureID { get; private set; }

        public static Image Parse(BinaryReader reader)
        {
            var image = new Image();
            image.TextureID = reader.ReadUInt32();
            return image;
        }
    }
}
