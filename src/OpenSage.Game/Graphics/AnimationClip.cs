using System;
using System.Numerics;

namespace OpenSage.Graphics
{
    public sealed class AnimationPlayer
    {
        private readonly Animation _animation;
        private readonly Model _model;

        private TimeSpan _currentTimeValue;

        private int[] _currentKeyframes;
        private Transform[] _boneTransforms;

        public AnimationPlayer(Animation animation, Model model)
        {
            _animation = animation;
            _model = model;

            _boneTransforms = new Transform[model.BindPose.Length];
        }

        public void Start()
        {
            _currentTimeValue = TimeSpan.Zero;
            _currentKeyframes = new int[_animation.Clips.Length];

            ResetBoneTransforms();
        }

        private void ResetBoneTransforms()
        {
            for (var i = 0; i < _boneTransforms.Length; i++)
            {
                _boneTransforms[i].Translation = Vector3.Zero;
                _boneTransforms[i].Rotation = Quaternion.Identity;
            }
        }

        public void Update(TimeSpan deltaTime)
        {
            UpdateBoneTransforms(deltaTime);

            for (var i = 0; i < _boneTransforms.Length; i++)
            {
                _model.AnimatedBoneTransforms[i] = 
                    Matrix4x4.CreateFromQuaternion(_boneTransforms[i].Rotation) * 
                    Matrix4x4.CreateTranslation(_boneTransforms[i].Translation);
            }
        }

        private void UpdateBoneTransforms(TimeSpan deltaTime)
        {
            var time = _currentTimeValue + deltaTime;

            // If we reached the end, loop back to the start.
            while (time >= _animation.Duration)
            {
                time -= _animation.Duration;
            }

            // If we've just moved backwards, reset the keyframe indices.
            if (time < _currentTimeValue)
            {
                Array.Clear(_currentKeyframes, 0, _currentKeyframes.Length);
                ResetBoneTransforms();
            }

            _currentTimeValue = time;

            for (var i = 0; i < _animation.Clips.Length; i++)
            {
                var clip = _animation.Clips[i];
                while (_currentKeyframes[i] < clip.Keyframes.Length)
                {
                    var keyframe = clip.Keyframes[_currentKeyframes[i]];

                    // Stop when we've read up to current time position.
                    if (keyframe.Time > _currentTimeValue)
                    {
                        break;
                    }

                    // Use this keyframe.
                    keyframe.Apply(ref _model.BindPose[clip.Bone], ref _boneTransforms[clip.Bone]);

                    _currentKeyframes[i] += 1;
                }
            }
        }
    }

    public struct Transform
    {
        public Vector3 Translation;
        public Quaternion Rotation;
    }

    public sealed class Animation
    {
        public string Name { get; }
        public TimeSpan Duration { get; }
        public AnimationClip[] Clips { get; }

        public Animation(string name, TimeSpan duration, AnimationClip[] clips)
        {
            Name = name;
            Duration = duration;
            Clips = clips;
        }
    }

    public sealed class AnimationClip
    {
        public int Bone { get; }
        public Keyframe[] Keyframes { get; }

        public AnimationClip(int bone, Keyframe[] keyframes)
        {
            Bone = bone;
            Keyframes = keyframes;
        }
    }

    public abstract class Keyframe
    {
        public TimeSpan Time { get; }

        protected Keyframe(TimeSpan time)
        {
            Time = time;
        }

        public abstract void Apply(ref Transform bindPoseTransform, ref Transform transform);
    }

    public sealed class QuaternionKeyframe : Keyframe
    {
        public Quaternion Rotation { get; }

        public QuaternionKeyframe(TimeSpan time, Quaternion rotation) 
            : base(time)
        {
            Rotation = rotation;
        }

        public override void Apply(ref Transform bindPoseTransform, ref Transform transform)
        {
            transform.Rotation = Rotation;
        }
    }

    public sealed class TranslationXKeyframe : Keyframe
    {
        public float Value { get; }

        public TranslationXKeyframe(TimeSpan time, float value)
            : base(time)
        {
            Value = value;
        }

        public override void Apply(ref Transform bindPoseTransform, ref Transform transform)
        {
            transform.Translation.X = Value;
        }
    }

    public sealed class TranslationYKeyframe : Keyframe
    {
        public float Value { get; }

        public TranslationYKeyframe(TimeSpan time, float value)
            : base(time)
        {
            Value = value;
        }

        public override void Apply(ref Transform bindPoseTransform, ref Transform transform)
        {
            transform.Translation.Y = Value;
        }
    }

    public sealed class TranslationZKeyframe : Keyframe
    {
        public float Value { get; }

        public TranslationZKeyframe(TimeSpan time, float value)
            : base(time)
        {
            Value = value;
        }

        public override void Apply(ref Transform bindPoseTransform, ref Transform transform)
        {
            transform.Translation.Z = Value;
        }
    }
}
