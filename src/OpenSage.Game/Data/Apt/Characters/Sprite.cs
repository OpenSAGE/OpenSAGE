using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Data.Apt.Characters
{
    public sealed class Sprite : Playable
    {    
        public static Sprite Parse(BinaryReader reader)
        {
            var sprite = new Sprite();
            sprite.Frames = reader.ReadListAtOffset<Frame>(() => Frame.Parse(reader));
            return sprite;
        }
    }
}
