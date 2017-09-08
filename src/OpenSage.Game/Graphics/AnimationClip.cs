using System;
using System.Numerics;
using OpenSage.Data.W3d;

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

                // Switch y and z to account for z being up in .w3d
                switch (w3dChannel.ChannelType)
                {
                    case W3dAnimationChannelType.Quaternion:
                        Keyframes[i] = new QuaternionKeyframe(
                            time,
                            new Quaternion(data[i, 0], data[i, 2], -data[i, 1], data[i, 3]));
                        break;

                    case W3dAnimationChannelType.TranslationX:
                        Keyframes[i] = new TranslationXKeyframe(time, data[i, 0]);
                        break;

                    case W3dAnimationChannelType.TranslationY:
                        Keyframes[i] = new TranslationZKeyframe(time, -data[i, 0]);
                        break;

                    case W3dAnimationChannelType.TranslationZ:
                        Keyframes[i] = new TranslationYKeyframe(time, data[i, 0]);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
