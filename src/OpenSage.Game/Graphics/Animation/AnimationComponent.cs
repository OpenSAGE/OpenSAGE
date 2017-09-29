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

        private int[] _currentKeyframes;
        private Transform[] _boneTransforms;

        private Animation _animation;

        public Animation Animation
        {
            get { return _animation; }
            set
            {
                _animation = value;

                _currentTimeValue = TimeSpan.Zero;
                _currentKeyframes = new int[value.Clips.Length];
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
            Array.Clear(_currentKeyframes, 0, _currentKeyframes.Length);

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
                    keyframe.Apply(
                        ref _boneTransforms[clip.Bone], 
                        x => _bones[clip.Bone].Entity.Visible = x);

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
}
