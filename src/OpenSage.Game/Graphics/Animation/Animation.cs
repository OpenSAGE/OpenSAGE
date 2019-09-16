using System;
using OpenSage.Content;
using OpenSage.FileFormats.W3d;

namespace OpenSage.Graphics.Animation
{
    public sealed partial class Animation : IHasName
    {
        internal static Animation FromW3dFile(W3dFile w3dFile)
        {
            var w3dAnimations = w3dFile.GetAnimations();
            var w3dCompressedAnimations = w3dFile.GetCompressedAnimations();

            var animations = new Animation[w3dAnimations.Count + w3dCompressedAnimations.Count];

            if (animations.Length != 1)
            {
                throw new NotSupportedException();
            }

            for (var i = 0; i < w3dAnimations.Count; i++)
            {
                animations[i] = new Animation(w3dAnimations[i]);
            }
            for (var i = 0; i < w3dCompressedAnimations.Count; i++)
            {
                animations[w3dAnimations.Count + i] = new Animation(w3dCompressedAnimations[i]);
            }

            return animations[0];
        }

        public string Name { get; }
        public TimeSpan Duration { get; }
        public AnimationClip[] Clips { get; }

        internal Animation(string name, TimeSpan duration, AnimationClip[] clips)
        {
            Name = name;
            Duration = duration;
            Clips = clips;
        }
    }
}
