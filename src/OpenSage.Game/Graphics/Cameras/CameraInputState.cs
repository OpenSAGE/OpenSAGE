using System.Collections.Generic;
using OpenSage.Input;

namespace OpenSage.Graphics.Cameras
{
    public struct CameraInputState
    {
        public bool LeftMouseDown;
        public bool MiddleMouseDown;
        public bool RightMouseDown;

        public int DeltaX;
        public int DeltaY;

        public int ScrollWheelValue;

        public List<Key> PressedKeys;
    }
}
