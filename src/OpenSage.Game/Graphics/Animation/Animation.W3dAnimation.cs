using System;
using System.Linq;
using OpenSage.Content.Util;
using OpenSage.FileFormats.W3d;

namespace OpenSage.Graphics.Animation
{
    partial class Animation
    {
        internal Animation(W3dAnimation w3dAnimation)
        {
            Name = w3dAnimation.Header.HierarchyName + "." + w3dAnimation.Header.Name;
            Duration = TimeSpan.FromSeconds(w3dAnimation.Header.NumFrames / (double) w3dAnimation.Header.FrameRate);

            var channels = w3dAnimation.Channels
                .OfType<W3dAnimationChannel>()
                .Where(x => x.ChannelType != W3dAnimationChannelType.UnknownBfme) // Don't know what this channel means.
                .ToList();

            var bitChannels = w3dAnimation.Channels
                .OfType<W3dBitChannel>()
                .ToList();

            Clips = new AnimationClip[channels.Count + bitChannels.Count];

            for (var i = 0; i < channels.Count; i++)
            {
                Clips[i] = CreateAnimationClip(w3dAnimation, channels[i]);
            }

            for (var i = 0; i < bitChannels.Count; i++)
            {
                Clips[channels.Count + i] = CreateAnimationClip(w3dAnimation, bitChannels[i]);
            }
        }

        private static AnimationClip CreateAnimationClip(W3dAnimation w3dAnimation, W3dAnimationChannel w3dChannel)
        {
            var bone = w3dChannel.Pivot;

            var data = w3dChannel.Data;
            var numKeyframes = data.GetLength(0);
            var keyframes = new Keyframe[numKeyframes];

            for (var i = 0; i < numKeyframes; i++)
            {
                var time = TimeSpan.FromSeconds((w3dChannel.FirstFrame + i) / (double) w3dAnimation.Header.FrameRate);
                keyframes[i] = CreateKeyframe(w3dChannel.ChannelType, time, data[i]);
            }

            return new AnimationClip(w3dChannel.ChannelType.ToAnimationClipType(), bone, keyframes);
        }

        private static Keyframe CreateKeyframe(W3dAnimationChannelType channelType, TimeSpan time, in W3dAnimationChannelDatum datum)
        {
            return new Keyframe(time, CreateKeyframeValue(channelType, datum));
        }

        private static KeyframeValue CreateKeyframeValue(W3dAnimationChannelType channelType, in W3dAnimationChannelDatum datum)
        {
            switch (channelType)
            {
                case W3dAnimationChannelType.Quaternion:
                    return new KeyframeValue { Quaternion = datum.Quaternion };

                case W3dAnimationChannelType.TranslationX:
                case W3dAnimationChannelType.TranslationY:
                case W3dAnimationChannelType.TranslationZ:
                    return new KeyframeValue { FloatValue = datum.FloatValue };

                default:
                    throw new NotImplementedException();
            }
        }

        private static AnimationClip CreateAnimationClip(W3dAnimation w3dAnimation, W3dBitChannel w3dChannel)
        {
            var bone = w3dChannel.Pivot;

            var data = w3dChannel.Data;

            var numKeyframes = data.GetLength(0);

            var totalKeyframes = numKeyframes;
            if (w3dChannel.FirstFrame != 0)
            {
                totalKeyframes++;
            }
            if (w3dChannel.LastFrame != w3dAnimation.Header.NumFrames - 1)
            {
                totalKeyframes++;
            }

            var keyframes = new Keyframe[totalKeyframes];

            var keyframeIndex = 0;
            if (w3dChannel.FirstFrame != 0)
            {
                keyframes[keyframeIndex++] = new Keyframe(TimeSpan.Zero, new KeyframeValue { BoolValue = w3dChannel.DefaultValue });
            }

            for (var i = 0; i < numKeyframes; i++)
            {
                var time = TimeSpan.FromSeconds((w3dChannel.FirstFrame + i) / (double) w3dAnimation.Header.FrameRate);

                switch (w3dChannel.ChannelType)
                {
                    case W3dBitChannelType.Visibility:
                        keyframes[keyframeIndex++] = new Keyframe(time, new KeyframeValue { BoolValue = data[i] });
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            if (w3dChannel.LastFrame != w3dAnimation.Header.NumFrames - 1)
            {
                var time = TimeSpan.FromSeconds((w3dChannel.LastFrame + 1) / (double) w3dAnimation.Header.FrameRate);
                keyframes[keyframeIndex++] = new Keyframe(time, new KeyframeValue { BoolValue = w3dChannel.DefaultValue });
            }

            return new AnimationClip(AnimationClipType.Visibility, bone, keyframes);
        }
    }
}
