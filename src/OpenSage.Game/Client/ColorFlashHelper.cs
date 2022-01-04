using System.Numerics;

namespace OpenSage.Client
{
    internal sealed class ColorFlashHelper : IPersistableObject
    {
        private Vector3 _increasingColorDelta;
        private Vector3 _decreasingColorDelta;
        private Vector3 _targetColor;
        private Vector3 _currentColor;
        private uint _holdFrames;
        private bool _isActive;
        private ColorFlashState _state;

        // TODO: Actual implementation

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistVector3("IncreasingColorDelta", ref _increasingColorDelta);
            reader.PersistVector3("DecreasingColorDelta", ref _decreasingColorDelta);
            reader.PersistVector3("TargetColor", ref _targetColor);
            reader.PersistVector3("CurrentColor", ref _currentColor);
            reader.PersistUInt32("HoldFrames", ref _holdFrames);
            reader.PersistBoolean("IsActive", ref _isActive);
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
