using System;
using System.Numerics;
using OpenSage.Data.W3d;
using OpenSage.Graphics.Util;

namespace OpenSage.Graphics
{
    public sealed class AnimationClip
    {
        public int Bone { get; }
        public Keyframe[] Keyframes { get; }

        internal AnimationClip(W3dAnimation w3dAnimation, W3dAnimationChannel w3dChannel)
        {
            Bone = w3dChannel.Pivot;

            var data = w3dChannel.Data;
            var numKeyframes = data.GetLength(0);
            Keyframes = new Keyframe[numKeyframes];

            for (var i = 0; i < numKeyframes; i++)
            {
                var time = TimeSpan.FromSeconds((w3dChannel.FirstFrame + i) / (double) w3dAnimation.Header.FrameRate);
                Keyframes[i] = CreateKeyframe(w3dChannel.ChannelType, time, ref data[i]);
            }
        }

        private static Keyframe CreateKeyframe(W3dAnimationChannelType channelType, TimeSpan time, ref W3dAnimationChannelDatum datum)
        {
            // Switch y and z to account for z being up in W3D.
            switch (channelType)
            {
                case W3dAnimationChannelType.Quaternion:
                    return new QuaternionKeyframe(time, datum.Quaternion.ToQuaternion());

                case W3dAnimationChannelType.TranslationX:
                    return new TranslationXKeyframe(time, datum.FloatValue);

                case W3dAnimationChannelType.TranslationY:
                    return new TranslationZKeyframe(time, -datum.FloatValue);

                case W3dAnimationChannelType.TranslationZ:
                    return new TranslationYKeyframe(time, datum.FloatValue);

                default:
                    throw new NotImplementedException();
            }
        }

        internal AnimationClip(W3dAnimation w3dAnimation, W3dBitChannel w3dChannel)
        {
            Bone = w3dChannel.Pivot;

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

            Keyframes = new Keyframe[totalKeyframes];

            var keyframeIndex = 0;
            if (w3dChannel.FirstFrame != 0)
            {
                Keyframes[keyframeIndex++] = new VisibilityKeyframe(TimeSpan.Zero, w3dChannel.DefaultValue);
            }

            for (var i = 0; i < numKeyframes; i++)
            {
                var time = TimeSpan.FromSeconds((w3dChannel.FirstFrame + i) / (double) w3dAnimation.Header.FrameRate);

                switch (w3dChannel.ChannelType)
                {
                    case W3dBitChannelType.Visibility:
                        Keyframes[keyframeIndex++] = new VisibilityKeyframe(time, data[i]);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            if (w3dChannel.LastFrame != w3dAnimation.Header.NumFrames - 1)
            {
                var time = TimeSpan.FromSeconds((w3dChannel.LastFrame + 1) / (double) w3dAnimation.Header.FrameRate);
                Keyframes[keyframeIndex++] = new VisibilityKeyframe(time, w3dChannel.DefaultValue);
            }
        }

        internal AnimationClip(W3dCompressedAnimation w3dAnimation, W3dTimeCodedAnimationChannel w3dChannel)
        {
            Bone = w3dChannel.Pivot;

            Keyframes = new Keyframe[w3dChannel.NumTimeCodes];

            for (var i = 0; i < w3dChannel.NumTimeCodes; i++)
            {
                var timeCodedDatum = w3dChannel.Data[i];
                var time = TimeSpan.FromSeconds(timeCodedDatum.TimeCode / (double) w3dAnimation.Header.FrameRate);
                Keyframes[i] = CreateKeyframe(w3dChannel.ChannelType, time, ref timeCodedDatum.Value);
            }
        }
    }
}
