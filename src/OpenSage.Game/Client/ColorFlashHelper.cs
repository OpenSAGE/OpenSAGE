using System.Numerics;

namespace OpenSage.Client
{
    internal sealed class ColorFlashHelper
    {
        private Vector3 _increasingColorDelta;
        private Vector3 _decreasingColorDelta;
        private Vector3 _targetColor;
        private Vector3 _currentColor;
        private uint _holdFrames;
        private bool _isActive;
        private ColorFlashState _state;

        // TODO: Actual implementation

        internal void Load(StatePersister reader)
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
