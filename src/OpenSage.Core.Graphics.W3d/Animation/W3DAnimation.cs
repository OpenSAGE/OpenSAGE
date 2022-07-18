using System;
using OpenSage.FileFormats.W3d;

namespace OpenSage.Graphics.Animation
{
    public sealed partial class W3DAnimation : BaseAsset
    {
        public static W3DAnimation FromW3dFile(W3dFile w3dFile)
        {
            var w3dAnimations = w3dFile.GetAnimations();
            var w3dCompressedAnimations = w3dFile.GetCompressedAnimations();

            var animations = new W3DAnimation[w3dAnimations.Count + w3dCompressedAnimations.Count];

            if (animations.Length == 0)
            {
                // sometimes w3d files are referenced inside animation states that do not contain any animation chunks
                return null;
            }

            if (animations.Length != 1)
            {
                throw new NotSupportedException();
            }

            for (var i = 0; i < w3dAnimations.Count; i++)
            {
                animations[i] = new W3DAnimation(w3dAnimations[i]);
            }
            for (var i = 0; i < w3dCompressedAnimations.Count; i++)
            {
                animations[w3dAnimations.Count + i] = new W3DAnimation(w3dCompressedAnimations[i]);
            }

            return animations[0];
        }

        public TimeSpan Duration { get; }
        public AnimationClip[] Clips { get; }

        public W3DAnimation(string name, TimeSpan duration, AnimationClip[] clips)
        {
            SetNameAndInstanceId("W3DAnimation", name);
            Duration = duration;
            Clips = clips;
        }
    }
}
