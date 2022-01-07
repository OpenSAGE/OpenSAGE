namespace OpenAS2.Runtime
{
    /// <summary>
    /// See SWF specification https://www.mobilefish.com/download/flash/swf_file_format_spec_v9.pdf
    /// page 103
    /// </summary>
    public enum PropertyType
    {
        X = 0,
        Y,
        XScale,
        YScale,
        CurrentFrame,
        TotalFrames,
        Alpha,
        Visible,
        Width,
        Height,
        Rotation,
        Target,
        FramesLoaded,
        Name,
        DropTarget,
        Url,
        HighQuality,
        FocusRect,
        SoundBufTime,
        Quality,
        XMouse,
        YMouse,
        Size
        //TODO add rest
    }


    /// <summary>
    /// properties exclusive to textfields
    /// </summary>
    public enum TextPropertyType
    {
        TextColor
    }
}
