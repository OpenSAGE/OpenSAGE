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

            _boneTransforms = new Transform[model.Bones.Length];
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

        public void Update(GameTime gameTime)
        {
            UpdateBoneTransforms(gameTime);

            for (var i = 0; i < _boneTransforms.Length; i++)
            {
                _model.AnimatedBoneTransforms[i] = 
                    Matrix4x4.CreateFromQuaternion(_boneTransforms[i].Rotation) * 
                    Matrix4x4.CreateTranslation(_boneTransforms[i].Translation);
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
                    keyframe.Apply(ref _boneTransforms[clip.Bone], ref _model.AnimatedBoneVisibilities[clip.Bone]);

                    _currentKeyframes[i] += 1;
                }
            }
        }
    }
}
