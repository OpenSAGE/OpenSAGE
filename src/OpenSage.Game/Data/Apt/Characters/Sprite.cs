using System.Collections.Generic;
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

        public static int Create(AptFile container, List<Frame> frames)
        {
            var characters = container.Movie.Characters;
            var spriteIndex = characters.Count;
            characters.Add(new Sprite
            {
                Container = container,
                Frames = frames
            });
            return spriteIndex;
        }
    }
}
