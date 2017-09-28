namespace OpenSage.Graphics
{
    public sealed class AnimationClip
    {
        public int Bone { get; }
        public Keyframe[] Keyframes { get; }

        internal AnimationClip(int bone, Keyframe[] keyframes)
        {
            Bone = bone;
            Keyframes = keyframes;
        }
    }
}
