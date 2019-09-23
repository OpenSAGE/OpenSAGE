using System;
using System.IO;
using System.Text;
using OpenSage.Data.Ini;
using OpenSage.Data.Wnd.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Data.Wnd
{
    public sealed class WndFile
    {
        public int FileVersion { get; internal set; }
        public WndLayoutBlock LayoutBlock { get; internal set; }
        public WndWindowDefinition RootWindow { get; internal set; }

        public static WndFile FromFileSystemEntry(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream, Encoding.ASCII))
            {
                var source = reader.ReadToEnd();
                var parser = new WndParser(source);
                return parser.ParseFile();
            }
        }
    }

    public sealed class WndLayoutBlock
    {
        public string LayoutInit { get; internal set; }
        public string LayoutUpdate { get; internal set; }
        public string LayoutShutdown { get; internal set; }
    }

    public sealed class WndWindowDefinition
    {
        public WndWindowType WindowType { get; internal set; }
        public WndScreenRect ScreenRect { get; internal set; }
        public string Name { get; internal set; }
        public WndWindowStatusFlags Status { get; internal set; }
        public WndWindowStyle Style { get; internal set; }
        public string SystemCallback { get; internal set; }
        public string InputCallback { get; internal set; }
        public string TooltipCallback { get; internal set; }
        public string DrawCallback { get; internal set; }
        public WndFont Font { get; internal set; }
        public string HeaderTemplate { get; internal set; }
        public string TooltipText { get; internal set; }
        public int TooltipDelay { get; internal set; }
        public WndTextColor TextColor { get; internal set; }
        public WndDrawData EnabledDrawData { get; internal set; }
        public WndDrawData DisabledDrawData { get; internal set; }
        public WndDrawData HiliteDrawData { get; internal set; }

        // Following are not applicable for all window types.
        public string Text { get; internal set; }
        public WndTextEntryData TextEntryData { get; internal set; }
        public WndStaticTextData StaticTextData { get; internal set; }
        public WndListBoxData ListBoxData { get; internal set; }
        public WndDrawData ListBoxEnabledUpButtonDrawData { get; internal set; }
        public WndDrawData ListBoxDisabledUpButtonDrawData { get; internal set; }
        public WndDrawData ListBoxHiliteUpButtonDrawData { get; internal set; }
        public WndDrawData ListBoxEnabledDownButtonDrawData { get; internal set; }
        public WndDrawData ListBoxDisabledDownButtonDrawData { get; internal set; }
        public WndDrawData ListBoxHiliteDownButtonDrawData { get; internal set; }
        public WndDrawData ListBoxEnabledSliderDrawData { get; internal set; }
        public WndDrawData ListBoxDisabledSliderDrawData { get; internal set; }
        public WndDrawData ListBoxHiliteSliderDrawData { get; internal set; }
        public WndDrawData SliderThumbEnabledDrawData { get; internal set; }
        public WndDrawData SliderThumbDisabledDrawData { get; internal set; }
        public WndDrawData SliderThumbHiliteDrawData { get; internal set; }
        public WndRadioButtonData RadioButtonData { get; internal set; }
        public WndComboBoxData ComboBoxData { get; internal set; }
        public WndDrawData ComboBoxDropDownButtonEnabledDrawData { get; internal set; }
        public WndDrawData ComboBoxDropDownButtonDisabledDrawData { get; internal set; }
        public WndDrawData ComboBoxDropDownButtonHiliteDrawData { get; internal set; }
        public WndDrawData ComboBoxEditBoxEnabledDrawData { get; internal set; }
        public WndDrawData ComboBoxEditBoxDisabledDrawData { get; internal set; }
        public WndDrawData ComboBoxEditBoxHiliteDrawData { get; internal set; }
        public WndDrawData ComboBoxListBoxEnabledDrawData { get; internal set; }
        public WndDrawData ComboBoxListBoxDisabledDrawData { get; internal set; }
        public WndDrawData ComboBoxListBoxHiliteDrawData { get; internal set; }
        public WndSliderData SliderData { get; internal set; }

        public WndWindowDefinition[] ChildWindows { get; internal set; }

        public bool HasHeaderTemplate => HeaderTemplate != null && !string.Equals(HeaderTemplate, "[NONE]", StringComparison.InvariantCultureIgnoreCase);
    }

    public sealed class WndTextEntryData
    {
        public int MaxLen { get; internal set; }
        public bool SecretText { get; internal set; }
        public bool NumericalOnly { get; internal set; }
        public bool AlphaNumericalOnly { get; internal set; }
        public bool AsciiOnly { get; internal set; }
    }

    public sealed class WndStaticTextData
    {
        public bool Centered { get; internal set; }
    }

    public struct WndWindowStyle
    {
        public WndWindowType WindowType { get; internal set; }
        public bool TrackMouse { get; internal set; }
    }

    public enum WndWindowType
    {
        GenericWindow,

        PushButton,
        CheckBox,
        RadioButton,

        ListBox,
        ComboBox,

        HorizontalSlider,
        VerticalSlider,
        ProgressBar,

        EntryField,
        StaticText,

        CommandButton
    }

    public struct WndScreenRect
    {
        public Point2D UpperLeft;
        public Point2D BottomRight;
        public Size CreationResolution;
    }

    [Flags]
    public enum WndWindowStatusFlags
    {
        None         = 0,

        Draggable    = 0x0001,
        Enabled      = 0x0002,
        NoInput      = 0x0004,
        NoFocus      = 0x0008,
        RightClick   = 0x0010,
        WrapCentered = 0x0020,
        Hidden       = 0x0040,
        Border       = 0x0080,
        Image        = 0x0100,
        SeeThru      = 0x0200,
        HotkeyText   = 0x0400,
        TabStop      = 0x0800,
        OneLine      = 0x1000,
        OnMouseDown  = 0x2000,
        CheckLike    = 0x4000,
        MouseTrack   = 0x8000
    }

    public sealed class WndFont
    {
        internal static WndFont Parse(IniParser parser)
        {
            return new WndFont
            {
                Name = parser.ParseQuotedString(),
                Size = parser.ParseInteger(),
                Bold = parser.ParseBoolean()
            };
        }

        public string Name { get; internal set; }
        public int Size { get; internal set; }
        public bool Bold { get; internal set; }
    }

    public sealed class WndTextColor
    {
        public ColorRgba Enabled { get; internal set; }
        public ColorRgba EnabledBorder { get; internal set; }
        public ColorRgba Disabled { get; internal set; }
        public ColorRgba DisabledBorder { get; internal set; }
        public ColorRgba Hilite { get; internal set; }
        public ColorRgba HiliteBorder { get; internal set; }
    }

    public sealed class WndDrawData
    {
        public WndDrawDataItem[] Items { get; internal set; }
    }

    public sealed class WndDrawDataItem
    {
        public string Image { get; internal set; }
        public ColorRgba Color { get; internal set; }
        public ColorRgba BorderColor { get; internal set; }
    }

    public sealed class WndListBoxData
    {
        public int Length { get; internal set; }
        public bool AutoScroll { get; internal set; }
        public bool ScrollIfAtEnd { get; internal set; }
        public bool AutoPurge { get; internal set; }
        public bool ScrollBar { get; internal set; }
        public bool MultiSelect { get; internal set; }
        public int Columns { get; internal set; }
        public int[] ColumnWidths { get; internal set; }
        public bool ForceSelect { get; internal set; }
    }

    public sealed class WndRadioButtonData
    {
        public int Group { get; internal set; }
    }

    public sealed class WndComboBoxData
    {
        public bool IsEditable { get; internal set; }
        public int MaxChars { get; internal set; }
        public int MaxDisplay { get; internal set; }
        public bool AsciiOnly { get; internal set; }
        public bool LettersAndNumbers { get; internal set; }
    }

    public sealed class WndSliderData
    {
        public int MinValue { get; internal set; }
        public int MaxValue { get; internal set; }
    }
}
