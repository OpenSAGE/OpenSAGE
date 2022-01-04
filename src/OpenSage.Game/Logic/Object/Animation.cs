namespace OpenSage.Logic.Object
{
    public sealed class Animation : IPersistableObject
    {
        public readonly AnimationTemplate Template;

        private ushort _currentImageIndex;
        private uint _lastUpdatedFrame;

        private ushort _unknown;

        private ushort _lastImageIndex;
        private uint _animationDelayFrames;

        public Animation(AnimationTemplate template)
        {
            Template = template;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistUInt16(ref _currentImageIndex);
            reader.PersistFrame("LastUpdatedFrame", ref _lastUpdatedFrame);
            reader.PersistUInt16(ref _unknown);

            reader.SkipUnknownBytes(1);

            reader.PersistUInt16(ref _lastImageIndex);
            reader.PersistUInt32("AnimationDelayFrames", ref _animationDelayFrames);

            var unknownFloat = 1.0f;
            reader.PersistSingle("UnknownFloat", ref unknownFloat);
            if (unknownFloat != 1.0f)
            {
                throw new InvalidStateException();
            }
        }
    }
}
