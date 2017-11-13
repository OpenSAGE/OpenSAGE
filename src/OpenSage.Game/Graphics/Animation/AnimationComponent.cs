using OpenSage.Mathematics;
using System;
using System.Numerics;

namespace OpenSage.Graphics.Animation
{
    public sealed class AnimationComponent : EntityComponent
    {
        private TransformComponent[] _bones;

        private bool _playing;

        private TimeSpan _currentTimeValue;

        private bool[] _originalVisibilities;

        private Transform[] _boneTransforms;

        private Animation _animation;

        public Animation Animation
        {
            get { return _animation; }
            set
            {
                _animation = value;

                _currentTimeValue = TimeSpan.Zero;
            }
        }

        public void Play()
        {
            if (_playing)
            {
                return;
            }

            var modelComponent = Entity.GetComponent<ModelComponent>();
            if (modelComponent.Bones != _bones)
            {
                _bones = modelComponent.Bones;
                _originalVisibilities = new bool[_bones.Length];
                _boneTransforms = new Transform[_bones.Length];
            }

            ResetBoneTransforms();

            for (var i = 0; i < _bones.Length; i++)
            {
                _originalVisibilities[i] = _bones[i].Entity.Visible;
            }

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
            for (var i = 0; i < _boneTransforms.Length; i++)
            {
                _boneTransforms[i].Translation = Vector3.Zero;
                _boneTransforms[i].Rotation = Quaternion.Identity;

                _bones[i].Entity.Visible = true;
            }
        }

        internal void Update(GameTime gameTime)
        {
            if (!_playing)
            {
                return;
            }

            UpdateBoneTransforms(gameTime);

            for (var i = 0; i < _boneTransforms.Length; i++)
            {
                _bones[i].LocalPosition = _boneTransforms[i].Translation;
                _bones[i].LocalRotation = _boneTransforms[i].Rotation;
            }
        }

        private void UpdateBoneTransforms(GameTime gameTime)
        {
            var time = _currentTimeValue + gameTime.ElapsedGameTime;

            // If we reached the end, loop back to the start.
            while (time >= _animation.Duration)
            {
                time -= _animation.Duration;
            }

            // If we've just moved backwards, reset the keyframe indices.
            if (time < _currentTimeValue)
            {
                ResetBoneTransforms();
            }

            _currentTimeValue = time;

            for (var i = 0; i < _animation.Clips.Length; i++)
            {
                Keyframe? previous = null;
                Keyframe? next = null;

                var clip = _animation.Clips[i];

                if (clip.Bone >= _boneTransforms.Length)
                {
                    continue;
                }

                for (var j = 0; j < clip.Keyframes.Length; j++)
                {
                    var keyframe = clip.Keyframes[j];

                    if (keyframe.Time > _currentTimeValue)
                    {
                        next = keyframe;
                        break;
                    }

                    previous = keyframe;
                }

                if (previous != null)
                {
                    Evaluate(clip, previous.Value, next ?? previous.Value, _currentTimeValue);
                }
            }
        }

        private void Evaluate(AnimationClip clip, Keyframe previous, Keyframe next, TimeSpan currentTime)
        {
            var progress = (float) (currentTime - previous.Time).TotalMilliseconds;
            var duration = (float) (next.Time - previous.Time).TotalMilliseconds;

            var amount = (duration != 0)
                ? progress / duration
                : 0;

            switch (clip.ClipType)
            {
                case AnimationClipType.TranslationX:
                    _boneTransforms[clip.Bone].Translation.X = MathUtility.Lerp(
                        previous.Value.FloatValue,
                        next.Value.FloatValue,
                        amount);
                    break;

                case AnimationClipType.TranslationY:
                    _boneTransforms[clip.Bone].Translation.Y = MathUtility.Lerp(
                        previous.Value.FloatValue,
                        next.Value.FloatValue,
                        amount);
                    break;

                case AnimationClipType.TranslationZ:
                    _boneTransforms[clip.Bone].Translation.Z = MathUtility.Lerp(
                        previous.Value.FloatValue,
                        next.Value.FloatValue,
                        amount);
                    break;

                case AnimationClipType.Quaternion:
                    _boneTransforms[clip.Bone].Rotation = Quaternion.Lerp(
                        previous.Value.Quaternion,
                        next.Value.Quaternion,
                        amount);
                    break;

                case AnimationClipType.Visibility:
                    _bones[clip.Bone].Entity.Visible = previous.Value.BoolValue;
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public struct Transform
    {
        public Vector3 Translation;
        public Quaternion Rotation;
    }
}
