using System;
using System.Linq;
using OpenSage.Content.Util;
using OpenSage.FileFormats.W3d;

namespace OpenSage.Graphics.Animation
{
    partial class Animation
    {
        internal Animation(W3dCompressedAnimation w3dAnimation)
        {
            Name = w3dAnimation.Header.HierarchyName + "." + w3dAnimation.Header.Name;
            Duration = TimeSpan.FromSeconds(w3dAnimation.Header.NumFrames / (double) w3dAnimation.Header.FrameRate);

            var timeCodedChannels = w3dAnimation.TimeCodedChannels
                .Where(x => x.ChannelType != W3dAnimationChannelType.UnknownBfme) // Don't know what this channel means.
                .ToList();

            var adaptiveDeltaChannels = w3dAnimation.AdaptiveDeltaChannels
               .Where(x => x.ChannelType != W3dAnimationChannelType.UnknownBfme) // Don't know what this channel means.
               .ToList();

            var motionChannels = w3dAnimation.MotionChannels
                .Where(x => x.ChannelType != W3dAnimationChannelType.UnknownBfme) // Don't know what this channel means.
                .ToList();

            var channelCount = timeCodedChannels.Count + adaptiveDeltaChannels.Count + motionChannels.Count;
            Clips = new AnimationClip[channelCount];

            for (var i = 0; i < timeCodedChannels.Count; i++)
            {
                Clips[i] = CreateAnimationClip(w3dAnimation, timeCodedChannels[i]);
            }

            var timecodedAndAdaptive = timeCodedChannels.Count + adaptiveDeltaChannels.Count;

            for (var i = timeCodedChannels.Count; i < timecodedAndAdaptive; ++i)
            {
                Clips[i] = CreateAnimationClip(w3dAnimation, adaptiveDeltaChannels[i - timeCodedChannels.Count]);
            }

            for (var i = timecodedAndAdaptive; i < channelCount; ++i)
            {
                Clips[i] = CreateAnimationClip(w3dAnimation, motionChannels[i - timecodedAndAdaptive]);
            }
        }

        private static AnimationClip CreateAnimationClip(W3dCompressedAnimation w3dAnimation, W3dTimeCodedAnimationChannel w3dChannel)
        {
            var bone = w3dChannel.Pivot;

            var keyframes = new Keyframe[w3dChannel.NumTimeCodes];

            for (var i = 0; i < w3dChannel.NumTimeCodes; i++)
            {
                var timeCodedDatum = w3dChannel.Data[i];
                var time = TimeSpan.FromSeconds(timeCodedDatum.TimeCode / (double) w3dAnimation.Header.FrameRate);
                keyframes[i] = CreateKeyframe(w3dChannel.ChannelType, time, timeCodedDatum.Value);
            }

            return new AnimationClip(w3dChannel.ChannelType.ToAnimationClipType(), bone, keyframes);
        }

        private static AnimationClip CreateAnimationClip(W3dCompressedAnimation w3dAnimation, W3dAdaptiveDeltaAnimationChannel w3dChannel)
        {
            var bone = w3dChannel.Pivot;

            var keyframes = new Keyframe[w3dChannel.NumTimeCodes];

            var decodedData = W3dAdaptiveDeltaCodec.Decode(
                w3dChannel.Data,
                w3dChannel.NumTimeCodes,
                w3dChannel.Scale);

            for (var i = 0; i < w3dChannel.NumTimeCodes; i++)
            {
                var time = TimeSpan.FromSeconds(i / (double) w3dAnimation.Header.FrameRate);
                keyframes[i] = CreateKeyframe(w3dChannel.ChannelType, time, decodedData[i]);
            }

            return new AnimationClip(w3dChannel.ChannelType.ToAnimationClipType(), bone, keyframes);
        }

        private static AnimationClip CreateAnimationClip(W3dCompressedAnimation w3dAnimation, W3dMotionChannel w3dChannel)
        {
            var bone = w3dChannel.Pivot;

            var keyframes = new Keyframe[w3dChannel.NumTimeCodes];
            var i = 0;
            foreach (var keyframeWithValue in w3dChannel.Data.GetKeyframesWithValues(w3dChannel))
            {
                var time = TimeSpan.FromSeconds(keyframeWithValue.Keyframe / (double) w3dAnimation.Header.FrameRate);
                keyframes[i++] = CreateKeyframe(w3dChannel.ChannelType, time, keyframeWithValue.Datum);
            }

            return new AnimationClip(w3dChannel.ChannelType.ToAnimationClipType(), bone, keyframes);
        }
    }
}
