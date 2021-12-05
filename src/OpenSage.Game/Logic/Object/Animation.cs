namespace OpenSage.Logic.Object
{
    public sealed class Animation
    {
        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var currentImageIndex = reader.ReadUInt16();
            var lastUpdatedFrame = reader.ReadUInt32();

            var unknown = reader.ReadUInt16();

            var unknown2 = reader.ReadByte();
            if (unknown2 != 0)
            {
                throw new InvalidStateException();
            }

            var lastImageIndex = reader.ReadUInt16();
            var animationDelayFrames = reader.ReadUInt32();

            var unknownFloat = reader.ReadSingle();
            if (unknownFloat != 1.0f)
            {
                throw new InvalidStateException();
            }
        }
    }
}
