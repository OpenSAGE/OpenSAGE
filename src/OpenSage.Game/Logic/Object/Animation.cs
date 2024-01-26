using System;
using OpenSage.Gui;

namespace OpenSage.Logic.Object
{
    public sealed class Animation : IPersistableObject
    {
        public readonly AnimationTemplate Template;
        public AnimationType AnimationType { get; }
        public MappedImage Current => Template.Images[_currentImageIndex].Value;

        private ushort _currentImageIndex;
        private uint _lastUpdatedFrame;

        private AnimationDirection _animationDirection;

        private ushort _lastImageIndex;
        private uint _animationDelayFrames;

        private uint NextFrameUpdateAllowed => _lastUpdatedFrame + _animationDelayFrames;
        private AnimationMode AnimationMode => Template.AnimationMode;

        public Animation(AnimationTemplate template)
        {
            Template = template;
            _animationDelayFrames = (uint)Math.Round(template.AnimationDelay * (Game.LogicFramesPerSecond / 1000));
            _lastImageIndex = (ushort)(template.Images.Count - 1);
            AnimationType = AnimationNameToType(template.Name);
            if (template.AnimationMode is AnimationMode.LoopBackwards or AnimationMode.OnceBackwards)
            {
                _currentImageIndex = _lastImageIndex;
                _animationDirection = AnimationDirection.Backwards;
            }
        }

        /// <summary>
        /// Sets the current frame for the animation
        /// </summary>
        /// <param name="currentFrame"></param>
        /// <returns>Whether the animation is complete</returns>
        public bool SetFrame(uint currentFrame)
        {
            var isComplete = currentFrame >= NextFrameUpdateAllowed && AnimationMode switch
            {
                AnimationMode.Once => _currentImageIndex == _lastImageIndex,
                AnimationMode.OnceBackwards => _currentImageIndex == 0,
                _ => false,
            };

            if (isComplete)
            {
                return false;
            }

            if (_lastUpdatedFrame == 0)
            {
                // todo: randomize start frame (what does this do?)
                _lastUpdatedFrame = currentFrame;
                return true;
            }

            // update CurrentImageIndex to the next index
            if (currentFrame < NextFrameUpdateAllowed)
            {
                return true; // nothing to update
            }

            _lastUpdatedFrame = currentFrame;

            _currentImageIndex = AnimationMode switch
            {
                AnimationMode.Loop when _currentImageIndex >= _lastImageIndex => 0, // go back to the start
                AnimationMode.LoopBackwards when _currentImageIndex <= 0 => _lastImageIndex, // go back to the end
                _ => (ushort)ComputeNextImageIndex(),
            };

            return true;
        }

        private int ComputeNextImageIndex()
        {
            if (AnimationMode is AnimationMode.PingPong)
            {
                // direction can change for ping-pong
                if (_currentImageIndex == _lastImageIndex)
                {
                    _animationDirection = AnimationDirection.Backwards;
                }
                else if (_currentImageIndex == 0)
                {
                    _animationDirection = AnimationDirection.Forwards;
                }
            }

            return _currentImageIndex + _animationDirection switch
            {
                AnimationDirection.Forwards => 1,
                AnimationDirection.Backwards => -1,
                _ => throw new ArgumentOutOfRangeException(nameof(_animationDirection)),
            };
        }

        private static AnimationType AnimationNameToType(string animationName)
        {
            return animationName switch
            {
                "DefaultHeal" => AnimationType.DefaultHeal,
                "StructureHeal" => AnimationType.StructureHeal,
                "VehicleHeal" => AnimationType.VehicleHeal,
                "MoneyPickUp" => AnimationType.MoneyPickUp,
                "LevelGainedAnimation" => AnimationType.LevelGainedAnimation,
                "GetHealedAnimation" => AnimationType.GetHealedAnimation,
                "BombTimed" => AnimationType.BombTimed,
                "BombRemote" => AnimationType.BombRemote,
                "CarBomb" => AnimationType.CarBomb,
                "Disabled" => AnimationType.Disabled,
                "AmmoFull" => AnimationType.AmmoFull,
                "AmmoEmpty" => AnimationType.AmmoEmpty,
                "Enthusiastic" => AnimationType.Enthusiastic,
                "Subliminal" => AnimationType.Subliminal,
                _ => throw new ArgumentOutOfRangeException(nameof(animationName)),
            };
        }

        public static string AnimationTypeToName(AnimationType animationType)
        {
            return animationType switch
            {
                AnimationType.DefaultHeal => "DefaultHeal",
                AnimationType.StructureHeal => "StructureHeal",
                AnimationType.VehicleHeal => "VehicleHeal",
                AnimationType.MoneyPickUp => "MoneyPickUp",
                AnimationType.LevelGainedAnimation => "LevelGainedAnimation",
                AnimationType.GetHealedAnimation => "GetHealedAnimation",
                AnimationType.BombTimed => "BombTimed",
                AnimationType.BombRemote => "BombRemote",
                AnimationType.CarBomb => "CarBomb",
                AnimationType.Disabled => "Disabled",
                AnimationType.AmmoFull => "AmmoFull",
                AnimationType.AmmoEmpty => "AmmoEmpty",
                AnimationType.Enthusiastic => "Enthusiastic",
                AnimationType.Subliminal => "Subliminal",
                _ => throw new ArgumentOutOfRangeException(nameof(animationType)),
            };
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistUInt16(ref _currentImageIndex);
            reader.PersistFrame(ref _lastUpdatedFrame);
            var animationDirection = (ushort)_animationDirection;
            reader.PersistUInt16(ref animationDirection);
            _animationDirection = (AnimationDirection) animationDirection;

            reader.SkipUnknownBytes(1);

            reader.PersistUInt16(ref _lastImageIndex);
            reader.PersistUInt32(ref _animationDelayFrames);

            var unknownFloat = 1.0f;
            reader.PersistSingle(ref unknownFloat);
            if (unknownFloat != 1.0f)
            {
                throw new InvalidStateException();
            }
        }

        // todo: forwards and backwards may be reversed: verify
        private enum AnimationDirection: ushort
        {
            Forwards,
            Unknown1, // stopped?
            Backwards,
        }
    }

    public enum AnimationType
    {
        DefaultHeal,
        StructureHeal,
        VehicleHeal,
        MoneyPickUp,
        LevelGainedAnimation,
        GetHealedAnimation,
        BombTimed,
        BombRemote,
        CarBomb,
        Disabled,
        AmmoFull,
        AmmoEmpty,
        Enthusiastic,
        Subliminal,
    }
}
