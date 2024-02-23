using System;
using System.Numerics;

namespace OpenSage.Client
{
    internal sealed class ColorFlashHelper : IPersistableObject
    {
        public Vector3 CurrentColor => _currentColor;
        public bool IsActive => _isActive;

        private Vector3 _increasingColorDelta;
        private Vector3 _decreasingColorDelta;
        private Vector3 _targetColor;
        private Vector3 _currentColor;
        private uint _holdFrames;
        private bool _isActive;
        private ColorFlashState _state;

        public void StepFrame()
        {
            if (!_isActive)
            {
                return;
            }

            switch (_state)
            {
                case ColorFlashState.None:
                    _isActive = false;
                    return;
                case ColorFlashState.Increasing:
                    _currentColor += _increasingColorDelta;
                    if (_currentColor == _targetColor || _currentColor.X > _targetColor.X || _currentColor.Y > _targetColor.Y || _currentColor.Z > _targetColor.Z)
                    {
                        _currentColor = _targetColor;
                        _state = ColorFlashState.Holding;
                    }

                    return;
                case ColorFlashState.Holding:
                    if (_holdFrames <= 0)
                    {
                        _state = ColorFlashState.Decreasing;
                    }
                    else
                    {
                        _holdFrames--;
                    }

                    return;
                case ColorFlashState.Decreasing:
                    _currentColor += _decreasingColorDelta;

                    if (_currentColor == Vector3.Zero || _currentColor.X < 0 || _currentColor.Y < 0 || _currentColor.Z < 0)
                    {
                        _currentColor = Vector3.Zero;
                        _state = ColorFlashState.None;
                        _isActive = false;
                    }

                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private const uint SelectionHoldFrames = 1;

        public void StartSelection(Vector3 targetColor)
        {
            _targetColor = targetColor;
            _increasingColorDelta = targetColor;
            _decreasingColorDelta = new Vector3(-targetColor.X / 4, -targetColor.Y / 4, -targetColor.Z / 4);
            Start(SelectionHoldFrames);
        }

        private void Start(uint holdFrames)
        {
            if (_isActive)
            {
                return;
            }

            _isActive = true;
            _state = ColorFlashState.Increasing;
            _holdFrames = holdFrames;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistVector3(ref _increasingColorDelta);
            reader.PersistVector3(ref _decreasingColorDelta);
            reader.PersistVector3(ref _targetColor);
            reader.PersistVector3(ref _currentColor);
            reader.PersistUInt32(ref _holdFrames);
            reader.PersistBoolean(ref _isActive);
            reader.PersistEnumByte(ref _state);
        }

        private enum ColorFlashState : byte
        {
            None = 0,
            Increasing = 1,
            Holding = 2,
            Decreasing = 3,
        }
    }
}
