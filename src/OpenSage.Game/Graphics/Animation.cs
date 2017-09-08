using System;
using OpenSage.Data.W3d;

namespace OpenSage.Graphics
{
    public sealed class Animation
    {
        public string Name { get; }
        public TimeSpan Duration { get; }
        public AnimationClip[] Clips { get; }

        public Animation(W3dAnimation w3dAnimation)
        {
            Name = w3dAnimation.Header.Name;
            Duration = TimeSpan.FromSeconds(w3dAnimation.Header.NumFrames / (double) w3dAnimation.Header.FrameRate);

            Clips = new AnimationClip[w3dAnimation.Channels.Count];

            for (var i = 0; i < w3dAnimation.Channels.Count; i++)
            {
                Clips[i] = new AnimationClip(w3dAnimation, w3dAnimation.Channels[i]);
            }
        }
    }
}
