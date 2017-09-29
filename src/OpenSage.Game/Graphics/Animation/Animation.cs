using System;

namespace OpenSage.Graphics.Animation
{
    public sealed class Animation
    {
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
