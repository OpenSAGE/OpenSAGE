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

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            reader.ReadVector3(ref _increasingColorDelta);
            reader.ReadVector3(ref _decreasingColorDelta);
            reader.ReadVector3(ref _targetColor);
            reader.ReadVector3(ref _currentColor);
            _holdFrames = reader.ReadUInt32();
            reader.ReadBoolean(ref _isActive);
            _state = reader.ReadEnumByte<ColorFlashState>();
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
