namespace OpenSage.Logic.Object
{
    public sealed class Animation
    {
        private readonly AnimationTemplate _template;

        private ushort _currentImageIndex;
        private uint _lastUpdatedFrame;

        private ushort _unknown;

        private ushort _lastImageIndex;
        private uint _animationDelayFrames;

        public Animation(AnimationTemplate template)
        {
            _template = template;
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            _currentImageIndex = reader.ReadUInt16();
            _lastUpdatedFrame = reader.ReadUInt32();

            _unknown = reader.ReadUInt16();

            reader.SkipUnknownBytes(1);

            _lastImageIndex = reader.ReadUInt16();
            _animationDelayFrames = reader.ReadUInt32();

            var unknownFloat = reader.ReadSingle();
            if (unknownFloat != 1.0f)
            {
                throw new InvalidStateException();
            }
        }
    }
}
