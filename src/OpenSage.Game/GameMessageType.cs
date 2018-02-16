namespace OpenSage
{
    public enum GameMessageType
    {
        Unknown27 = 27,
        SetSelection = 1001,
        ClearSelection = 1003,
        SetRallyPoint = 1043,
        CreateUnit = 1047,
        BuildObject = 1049,
        MoveTo = 1068,
        SetCameraPosition = 1092,
        Checksum = 1095,
        Unknown1097 = 1097,

        // Input messages
        KeyDown = 5000,
        KeyUp,
        MouseLeftButtonDown,
        MouseLeftButtonUp,
        MouseMiddleButtonDown,
        MouseMiddleButtonUp,
        MouseRightButtonDown,
        MouseRightButtonUp,
        MouseMove,
        MouseWheel
    }
}
