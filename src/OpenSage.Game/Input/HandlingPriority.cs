namespace OpenSage.Input;

public enum HandlingPriority
{
    Disabled,
    CameraPriority,
    BoxSelectionPriority,
    MoveCameraPriority,
    SelectionPriority,
    OrderGeneratorPriority,
    UIPriority,
    DebugPriority,
    Engine,
    Window,
}
