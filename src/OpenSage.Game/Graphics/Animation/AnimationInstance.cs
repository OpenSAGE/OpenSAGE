using System;
using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Animation
{
    public sealed class AnimationInstance
    {
        private readonly int[] _keyframeIndices;
        private readonly ModelBoneInstance[] _boneInstances;
        private readonly W3DAnimation _animation;

        private TimeSpan _currentTimeValue;

        private bool _playing;
        private readonly AnimationMode _mode;
        private readonly AnimationFlags _flags;

        private bool Looping => _mode.HasFlag(AnimationMode.Loop) || _mode.HasFlag(AnimationMode.LoopBackwards);
        private bool Reverse => _mode.HasFlag(AnimationMode.OnceBackwards) || _mode.HasFlag(AnimationMode.LoopBackwards);
        private bool Manual => _mode.HasFlag(AnimationMode.Manual);

        public AnimationInstance(ModelInstance modelInstance, W3DAnimation animation,
            AnimationMode mode, AnimationFlags flags)
        {
            _animation = animation;
            _mode = mode;
            _flags = flags;
            _boneInstances = modelInstance.ModelBoneInstances;

            _keyframeIndices = new int[animation.Clips.Length];

            if (_flags.HasFlag(AnimationFlags.StartFrameFirst) ||
                _flags == AnimationFlags.None)
            {
                _currentTimeValue = TimeSpan.Zero;
            }
            else if (_flags.HasFlag(AnimationFlags.StartFrameLast))
            {
                _currentTimeValue = _animation.Duration;
            }
            else
            {
                //TODO: implement other flags
                //throw new NotImplementedException();
            }
        }

        public void Play()
        {
            if (_playing)
            {
                return;
            }

            ResetBoneTransforms();

            _playing = true;
        }

        public void Stop()
        {
            if (!_playing)
            {
                return;
            }

            // TODO: Reset to original transforms and visibilities?

            _playing = false;
        }

        private void ResetBoneTransforms()
        {
            Array.Clear(_keyframeIndices, 0, _keyframeIndices.Length);

            for (var i = 0; i < _boneInstances.Length; i++)
            {
                _boneInstances[i].AnimatedOffset.Translation = Vector3.Zero;
                _boneInstances[i].AnimatedOffset.Rotation = Quaternion.Identity;

                _boneInstances[i].Visible = true;
            }
        }

        internal void Update(in TimeInterval gameTime)
        {
            if (!_playing)
            {
                return;
            }

            UpdateBoneTransforms(gameTime);
        }

        private void UpdateBoneTransforms(in TimeInterval gameTime)
        {
            //TODO: implement reverse and ping pong
            var time = _currentTimeValue;

            if (!Manual)
            {
                time += gameTime.DeltaTime;
            }

            if (time >= _animation.Duration)
            {
                // If we reached the end, loop back to the start or stay at the last frame
                if (Looping)
                {
                    while (time >= _animation.Duration)
                    {
                        time -= _animation.Duration;
                    }

                    // If we've just moved backwards, reset the keyframe indices.
                    if (time < _currentTimeValue)
                    {
                        ResetBoneTransforms();
                    }
                }
                else
                {
                    time = _animation.Duration;
                }
            }

            _currentTimeValue = time;

            Keyframe? previous;
            Keyframe? next;

            for (var i = 0; i < _animation.Clips.Length; i++)
            {
                previous = null;
                next = null;

                var clip = _animation.Clips[i];

                if (clip.Bone >= _boneInstances.Length)
                {
                    continue;
                }

                for (var j = _keyframeIndices[i]; j < clip.Keyframes.Length; j++)
                {
                    var keyframe = clip.Keyframes[j];

                    if (keyframe.Time > _currentTimeValue)
                    {
                        next = keyframe;
                        break;
                    }

                    previous = keyframe;
                    _keyframeIndices[i] = j;
                }

                if (previous != null)
                {
                    Evaluate(
                        clip,
                        previous.Value,
                        next ?? previous.Value,
                        _currentTimeValue);
                }
            }
        }

        private void Evaluate(
            AnimationClip clip,
            in Keyframe previous,
            in Keyframe next,
            TimeSpan currentTime)
        {
            var progress = (float) (currentTime - previous.Time).TotalMilliseconds;
            var duration = (float) (next.Time - previous.Time).TotalMilliseconds;

            var amount = (duration != 0)
                ? progress / duration
                : 0;

            var animatedOffset = _boneInstances[clip.Bone].AnimatedOffset;

            switch (clip.ClipType)
            {
                case AnimationClipType.TranslationX:
                    animatedOffset.Translation = animatedOffset.Translation.WithX(MathUtility.Lerp(
                        previous.Value.FloatValue,
                        next.Value.FloatValue,
                        amount));
                    break;

                case AnimationClipType.TranslationY:
                    animatedOffset.Translation = animatedOffset.Translation.WithY(MathUtility.Lerp(
                        previous.Value.FloatValue,
                        next.Value.FloatValue,
                        amount));
                    break;

                case AnimationClipType.TranslationZ:
                    animatedOffset.Translation = animatedOffset.Translation.WithZ(MathUtility.Lerp(
                        previous.Value.FloatValue,
                        next.Value.FloatValue,
                        amount));
                    break;

                case AnimationClipType.Quaternion:
                    _boneInstances[clip.Bone].AnimatedOffset.Rotation = Quaternion.Slerp(
                        previous.Value.Quaternion,
                        next.Value.Quaternion,
                        amount);
                    break;

                case AnimationClipType.Visibility:
                    _boneInstances[clip.Bone].Visible = previous.Value.BoolValue;
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
