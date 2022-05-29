using System.IO;
using System;

namespace OpenSage.FileFormats.Apt.Characters
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

        public override void Write(BinaryWriter writer, BinaryMemoryChain pool)
        {
            writer.Write((UInt32) CharacterType.Image);
            writer.Write((UInt32) Character.SIGNATURE);
            writer.Write((UInt32) TextureID);
        }
    }
}
