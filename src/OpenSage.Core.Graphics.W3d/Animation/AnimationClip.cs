namespace OpenSage.Graphics.Animation
{
    public sealed class AnimationClip
    {
        public AnimationClipType ClipType { get; }
        public int Bone { get; }
        public Keyframe[] Keyframes { get; }

        public AnimationClip(AnimationClipType clipType, int bone, Keyframe[] keyframes)
        {
            ClipType = clipType;
            Bone = bone;
            Keyframes = keyframes;
        }
    }
}
