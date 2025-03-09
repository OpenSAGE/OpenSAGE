using System.Collections.Generic;
using Veldrid;

namespace OpenSage.Graphics.Cameras;

public struct EditorCameraInputState
{
    public bool LeftMouseDown;
    public bool MiddleMouseDown;
    public bool RightMouseDown;

    public int DeltaX;
    public int DeltaY;

    public int ScrollWheelValue;

    public List<Key> PressedKeys;
}
