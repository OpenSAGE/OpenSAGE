using System;
using System.IO;
using System.Text;
using OpenSage.Data.Ini.Parser;
using OpenSage.Data.Wnd.Parser;

namespace OpenSage.Data.Wnd
{
    public sealed class WndFile
    {
        public int FileVersion { get; internal set; }
        public WndLayoutBlock LayoutBlock { get; internal set; }
        public WndWindow RootWindow { get; internal set; }

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

    public sealed class WndWindow
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

        public WndWindow[] ChildWindows { get; internal set; }
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

        TextEntry,
        StaticText
    }

    public struct WndScreenRect
    {
        public WndPoint UpperLeft;
        public WndPoint BottomRight;
        public WndSize CreationResolution;
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
        CheckLike    = 0x4000
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
        public WndColor Enabled { get; internal set; }
        public WndColor EnabledBorder { get; internal set; }
        public WndColor Disabled { get; internal set; }
        public WndColor DisabledBorder { get; internal set; }
        public WndColor Hilite { get; internal set; }
        public WndColor HiliteBorder { get; internal set; }
    }

    public struct WndPoint
    {
        internal static WndPoint Parse(IniParser parser)
        {
            return new WndPoint
            {
                X = parser.ParseAttributeInteger("X"),
                Y = parser.ParseAttributeInteger("Y")
            };
        }

        public int X;
        public int Y;
    }

    public struct WndSize
    {
        public int Width;
        public int Height;
    }

    public struct WndColor
    {
        internal static WndColor Parse(IniParser parser)
        {
            var r = parser.ParseAttributeByte("R");
            var g = parser.ParseAttributeByte("G");
            var b = parser.ParseAttributeByte("B");

            var aToken = parser.GetNextTokenOptional(IniParser.SeparatorsColon);
            var a = (byte) 255;
            if (aToken != null)
            {
                if (aToken.Value.Text != "A")
                {
                    throw new IniParseException($"Expected attribute name 'A'", aToken.Value.Position);
                }
                a = parser.ScanByte(parser.GetNextToken(IniParser.SeparatorsColon));
            }

            return new WndColor
            {
                R = r,
                G = g,
                B = b,
                A = a
            };
        }

        public byte R;
        public byte G;
        public byte B;
        public byte A;
    }

    public sealed class WndDrawData
    {
        public WndDrawDataItem[] Items { get; internal set; }
    }

    public sealed class WndDrawDataItem
    {
        public string Image { get; internal set; }
        public WndColor Color { get; internal set; }
        public WndColor BorderColor { get; internal set; }
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
