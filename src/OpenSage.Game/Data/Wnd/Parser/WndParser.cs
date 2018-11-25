using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Mathematics;

namespace OpenSage.Data.Wnd.Parser
{
    internal sealed class WndParser
    {
        private readonly List<WndToken> _tokens;
        private int _tokenIndex;

        public WndParser(string source)
        {
            var lexer = new WndLexer(source);

            _tokens = new List<WndToken>();

            WndToken token;
            do
            {
                _tokens.Add(token = lexer.Lex());
            } while (token.TokenType != WndTokenType.EndOfFile);
        }

        private WndToken Current => _tokens[_tokenIndex];

        private WndToken NextToken(WndTokenType? tokenType = null)
        {
            var token = Current;
            if (tokenType != null && token.TokenType != tokenType.Value)
            {
                throw new InvalidDataException($"Expected token of type '{tokenType.Value}', but got token of type '{token.TokenType}'.");
            }
            _tokenIndex++;
            return token;
        }

        private WndToken NextIdentifierToken(string expectedStringValue)
        {
            var token = NextToken(WndTokenType.Identifier);
            if (token.StringValue != expectedStringValue)
            {
                throw new InvalidDataException($"Expected an identifier with name '{expectedStringValue}', but got '{token.StringValue}'.");
            }
            return token;
        }

        private int NextIntegerLiteralTokenValue()
        {
            return NextToken(WndTokenType.IntegerLiteral).IntegerValue;
        }

        private string NextStringLiteralTokenValue()
        {
            return NextToken(WndTokenType.StringLiteral).StringValue;
        }

        private void UnexpectedToken(WndToken token)
        {
            throw new InvalidDataException($"Unexpected token: {token}");
        }

        private T ParseProperty<T>(Func<T> parsePropertyValue)
        {
            NextToken();
            NextToken(WndTokenType.Equals);

            var result = parsePropertyValue();

            NextToken(WndTokenType.Semicolon);

            return result;
        }

        private T ParseAttribute<T>(string name, Func<T> parseAttributeValue)
        {
            NextIdentifierToken(name);
            NextToken(WndTokenType.Colon);

            return parseAttributeValue();
        }

        private void ParseAttributes(Dictionary<string, Action> parseAttributeValues)
        {
            do
            {
                var name = NextToken(WndTokenType.Identifier).StringValue;

                if (!parseAttributeValues.TryGetValue(name, out var attributeValueParser))
                {
                    throw new InvalidDataException($"Unexpected attribute name: '{name}'");
                }

                NextToken(WndTokenType.Colon);

                attributeValueParser();

                if (Current.TokenType != WndTokenType.Semicolon)
                {
                    NextToken(WndTokenType.Comma);
                }
            } while (Current.TokenType != WndTokenType.Semicolon);
        }

        public WndFile ParseFile()
        {
            var result = new WndFile();

            while (Current.TokenType != WndTokenType.EndOfFile)
            {
                switch (Current.TokenType)
                {
                    case WndTokenType.Identifier:
                        switch (Current.StringValue)
                        {
                            case "FILE_VERSION":
                                result.FileVersion = ParseProperty(NextIntegerLiteralTokenValue);
                                break;

                            case "STARTLAYOUTBLOCK":
                                result.LayoutBlock = ParseLayoutBlock();
                                break;

                            case "WINDOW":
                                result.RootWindow = ParseWindow();
                                break;

                            default:
                                UnexpectedToken(Current);
                                break;
                        }
                        break;

                    default:
                        UnexpectedToken(Current);
                        break;
                }
            }

            return result;
        }

        private WndLayoutBlock ParseLayoutBlock()
        {
            var result = new WndLayoutBlock();

            NextToken();

            while (Current.TokenType == WndTokenType.Identifier)
            {
                if (Current.StringValue == "ENDLAYOUTBLOCK")
                {
                    NextToken();
                    break;
                }

                switch (Current.StringValue)
                {
                    case "LAYOUTINIT":
                        result.LayoutInit = ParseLayoutBlockValue();
                        break;

                    case "LAYOUTUPDATE":
                        result.LayoutUpdate = ParseLayoutBlockValue();
                        break;

                    case "LAYOUTSHUTDOWN":
                        result.LayoutShutdown = ParseLayoutBlockValue();
                        break;

                    default:
                        UnexpectedToken(Current);
                        break;
                }
            }

            return result;
        }

        private string ParseLayoutBlockValue()
        {
            return ParseProperty(() =>
            {
                switch (Current.TokenType)
                {
                    case WndTokenType.OpenSquareBracket:
                        NextToken();
                        var none = NextIdentifierToken("None");
                        NextToken(WndTokenType.CloseSquareBracket);
                        return "[None]";

                    case WndTokenType.Identifier:
                        return NextToken().StringValue;

                    default:
                        UnexpectedToken(Current);
                        return null;
                }
            });
        }

        private WndWindowDefinition ParseWindow()
        {
            NextToken();

            var result = new WndWindowDefinition();

            var childWindows = new List<WndWindowDefinition>();

            while (Current.TokenType == WndTokenType.Identifier)
            {
                if (Current.StringValue == "END")
                {
                    NextToken();
                    break;
                }

                switch (Current.StringValue)
                {
                    case "WINDOWTYPE":
                        result.WindowType = ParseProperty(ParseWindowType);
                        break;

                    case "SCREENRECT":
                        result.ScreenRect = ParseProperty(ParseScreenRectValue);
                        break;

                    case "NAME":
                        result.Name = ParseProperty(NextStringLiteralTokenValue);
                        break;

                    case "STATUS":
                        result.Status = ParseProperty(ParseStatus);
                        break;

                    case "STYLE":
                        result.Style = ParseProperty(ParseStyle);
                        break;

                    case "SYSTEMCALLBACK":
                        result.SystemCallback = ParseProperty(NextStringLiteralTokenValue);
                        break;

                    case "INPUTCALLBACK":
                        result.InputCallback = ParseProperty(NextStringLiteralTokenValue);
                        break;

                    case "TOOLTIPCALLBACK":
                        result.TooltipCallback = ParseProperty(NextStringLiteralTokenValue);
                        break;

                    case "DRAWCALLBACK":
                        result.DrawCallback = ParseProperty(NextStringLiteralTokenValue);
                        break;

                    case "FONT":
                        result.Font = ParseProperty(ParseFont);
                        break;

                    case "HEADERTEMPLATE":
                        result.HeaderTemplate = ParseProperty(NextStringLiteralTokenValue);
                        break;

                    case "TOOLTIPTEXT":
                        result.TooltipText = ParseProperty(NextStringLiteralTokenValue);
                        break;


                    case "TOOLTIPDELAY":
                        result.TooltipDelay = ParseProperty(NextIntegerLiteralTokenValue);
                        break;

                    case "TEXTCOLOR":
                        result.TextColor = ParseProperty(ParseTextColor);
                        break;

                    case "ENABLEDDRAWDATA":
                        result.EnabledDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "DISABLEDDRAWDATA":
                        result.DisabledDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "HILITEDRAWDATA":
                        result.HiliteDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "CHILD":
                        NextToken();
                        childWindows.Add(ParseWindow());
                        break;

                    case "ENDALLCHILDREN":
                        NextToken();
                        break;

                    case "TEXT":
                        result.Text = ParseProperty(NextStringLiteralTokenValue);
                        break;

                    case "TEXTENTRYDATA":
                        result.TextEntryData = ParseProperty(ParseTextEntryData);
                        break;

                    case "STATICTEXTDATA":
                        result.StaticTextData = ParseProperty(ParseStaticTextData);
                        break;

                    case "LISTBOXDATA":
                        result.ListBoxData = ParseProperty(ParseListBoxData);
                        break;

                    case "LISTBOXENABLEDUPBUTTONDRAWDATA":
                        result.ListBoxEnabledUpButtonDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "LISTBOXDISABLEDUPBUTTONDRAWDATA":
                        result.ListBoxDisabledUpButtonDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "LISTBOXHILITEUPBUTTONDRAWDATA":
                        result.ListBoxHiliteUpButtonDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "LISTBOXENABLEDDOWNBUTTONDRAWDATA":
                        result.ListBoxEnabledDownButtonDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "LISTBOXDISABLEDDOWNBUTTONDRAWDATA":
                        result.ListBoxDisabledDownButtonDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "LISTBOXHILITEDOWNBUTTONDRAWDATA":
                        result.ListBoxHiliteDownButtonDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "LISTBOXENABLEDSLIDERDRAWDATA":
                        result.ListBoxEnabledSliderDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "LISTBOXDISABLEDSLIDERDRAWDATA":
                        result.ListBoxDisabledSliderDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "LISTBOXHILITESLIDERDRAWDATA":
                        result.ListBoxHiliteSliderDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "SLIDERTHUMBENABLEDDRAWDATA":
                        result.SliderThumbEnabledDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "SLIDERTHUMBDISABLEDDRAWDATA":
                        result.SliderThumbDisabledDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "SLIDERTHUMBHILITEDRAWDATA":
                        result.SliderThumbHiliteDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "RADIOBUTTONDATA":
                        result.RadioButtonData = ParseProperty(ParseRadioButtonData);
                        break;

                    case "COMBOBOXDATA":
                        result.ComboBoxData = ParseProperty(ParseComboBoxData);
                        break;

                    case "COMBOBOXDROPDOWNBUTTONENABLEDDRAWDATA":
                        result.ComboBoxDropDownButtonEnabledDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "COMBOBOXDROPDOWNBUTTONDISABLEDDRAWDATA":
                        result.ComboBoxDropDownButtonDisabledDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "COMBOBOXDROPDOWNBUTTONHILITEDRAWDATA":
                        result.ComboBoxDropDownButtonHiliteDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "COMBOBOXEDITBOXENABLEDDRAWDATA":
                        result.ComboBoxEditBoxEnabledDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "COMBOBOXEDITBOXDISABLEDDRAWDATA":
                        result.ComboBoxEditBoxDisabledDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "COMBOBOXEDITBOXHILITEDRAWDATA":
                        result.ComboBoxEditBoxHiliteDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "COMBOBOXLISTBOXENABLEDDRAWDATA":
                        result.ComboBoxListBoxEnabledDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "COMBOBOXLISTBOXDISABLEDDRAWDATA":
                        result.ComboBoxListBoxDisabledDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "COMBOBOXLISTBOXHILITEDRAWDATA":
                        result.ComboBoxListBoxHiliteDrawData = ParseProperty(ParseDrawData);
                        break;

                    case "SLIDERDATA":
                        result.SliderData = ParseProperty(ParseSliderData);
                        break;

                    default:
                        UnexpectedToken(Current);
                        break;
                }
            }

            result.ChildWindows = childWindows.ToArray();

            return result;
        }

        private WndWindowType ParseWindowType()
        {
            var windowType = NextToken(WndTokenType.Identifier).StringValue;

            switch (windowType)
            {
                case "USER":
                    return WndWindowType.GenericWindow;

                case "PUSHBUTTON":
                    return WndWindowType.PushButton;

                case "ENTRYFIELD":
                    return WndWindowType.EntryField;

                case "STATICTEXT":
                    return WndWindowType.StaticText;

                case "PROGRESSBAR":
                    return WndWindowType.ProgressBar;

                case "SCROLLLISTBOX":
                    return WndWindowType.ListBox;

                case "RADIOBUTTON":
                    return WndWindowType.RadioButton;

                case "COMBOBOX":
                    return WndWindowType.ComboBox;

                case "CHECKBOX":
                    return WndWindowType.CheckBox;

                case "HORZSLIDER":
                    return WndWindowType.HorizontalSlider;

                case "VERTSLIDER":
                    return WndWindowType.VerticalSlider;

                case "COMMANDBUTTON":
                    return WndWindowType.CommandButton;

                default:
                    throw new ArgumentOutOfRangeException(nameof(windowType), windowType, "Unexpected window type.");
            }
        }

        private WndScreenRect ParseScreenRectValue()
        {
            var result = new WndScreenRect();

            ParseAttributes(new Dictionary<string, Action>
            {
                { "UPPERLEFT", () => result.UpperLeft = ParsePoint() },
                { "BOTTOMRIGHT", () => result.BottomRight = ParsePoint() },
                { "CREATIONRESOLUTION", () => result.CreationResolution = ParseSize() },
            });

            return result;
        }

        private Point2D ParsePoint()
        {
            return new Point2D(
                NextIntegerLiteralTokenValue(),
                NextIntegerLiteralTokenValue());
        }

        private Size ParseSize()
        {
            return new Size(
                NextIntegerLiteralTokenValue(),
                NextIntegerLiteralTokenValue());
        }

        private WndWindowStatusFlags ParseStatus()
        {
            var result = WndWindowStatusFlags.None;

            while (Current.TokenType != WndTokenType.Semicolon)
            {
                var identifier = NextToken(WndTokenType.Identifier);
                switch (identifier.StringValue)
                {
                    case "ENABLED":
                        result |= WndWindowStatusFlags.Enabled;
                        break;

                    case "BORDER":
                        result |= WndWindowStatusFlags.Border;
                        break;

                    case "SEE_THRU":
                        result |= WndWindowStatusFlags.SeeThru;
                        break;

                    case "IMAGE":
                        result |= WndWindowStatusFlags.Image;
                        break;

                    case "NOINPUT":
                        result |= WndWindowStatusFlags.NoInput;
                        break;

                    case "NOFOCUS":
                        result |= WndWindowStatusFlags.NoFocus;
                        break;

                    case "HIDDEN":
                        result |= WndWindowStatusFlags.Hidden;
                        break;

                    case "WRAP_CENTERED":
                        result |= WndWindowStatusFlags.WrapCentered;
                        break;

                    case "HOTKEY_TEXT":
                        result |= WndWindowStatusFlags.HotkeyText;
                        break;

                    case "DRAGABLE":
                        result |= WndWindowStatusFlags.Draggable;
                        break;

                    case "RIGHT_CLICK":
                        result |= WndWindowStatusFlags.RightClick;
                        break;

                    case "TABSTOP":
                        result |= WndWindowStatusFlags.TabStop;
                        break;

                    case "ONE_LINE":
                        result |= WndWindowStatusFlags.OneLine;
                        break;

                    case "ON_MOUSE_DOWN":
                        result |= WndWindowStatusFlags.OnMouseDown;
                        break;

                    case "CHECK_LIKE":
                        result |= WndWindowStatusFlags.CheckLike;
                        break;

                    case "MOUSETRACK":
                        result |= WndWindowStatusFlags.MouseTrack;
                        break;

                    default:
                        throw new InvalidDataException($"Unexpected window status flag: {identifier.StringValue}");
                }

                if (Current.TokenType != WndTokenType.Semicolon)
                {
                    NextToken(WndTokenType.Plus);
                }
            }

            return result;
        }

        private WndWindowStyle ParseStyle()
        {
            var result = new WndWindowStyle();

            do
            {
                if (Current.TokenType == WndTokenType.Identifier)
                {
                    if (Current.StringValue == "MOUSETRACK")
                    {
                        NextToken();
                        result.TrackMouse = true;
                    }
                    else
                    {
                        result.WindowType = ParseWindowType();
                    }

                    if (Current.TokenType == WndTokenType.Plus)
                    {
                        NextToken();
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    UnexpectedToken(Current);
                }
            }
            while (Current.TokenType != WndTokenType.Semicolon);

            return result;
        }

        private WndFont ParseFont()
        {
            var result = new WndFont();

            ParseAttributes(new Dictionary<string, Action>
            {
                { "NAME", () => result.Name = NextStringLiteralTokenValue() },
                { "SIZE", () => result.Size = NextIntegerLiteralTokenValue() },
                { "BOLD", () => result.Bold = ParseBoolean() }
            });

            return result;
        }

        private bool ParseBoolean()
        {
            var result = NextIntegerLiteralTokenValue();
            if (result != 0 && result != 1)
            {
                throw new InvalidDataException();
            }
            return result == 1;
        }

        private WndTextColor ParseTextColor()
        {
            var result = new WndTextColor();

            ParseAttributes(new Dictionary<string, Action>
            {
                { "ENABLED", () => result.Enabled = ParseColor() },
                { "ENABLEDBORDER", () => result.EnabledBorder = ParseColor() },
                { "DISABLED", () => result.Disabled = ParseColor() },
                { "DISABLEDBORDER", () => result.DisabledBorder = ParseColor() },
                { "HILITE", () => result.Hilite = ParseColor() },
                { "HILITEBORDER", () => result.HiliteBorder = ParseColor() },
            });

            return result;
        }

        private ColorRgba ParseColor()
        {
            return new ColorRgba(
                (byte) NextIntegerLiteralTokenValue(),
                (byte) NextIntegerLiteralTokenValue(),
                (byte) NextIntegerLiteralTokenValue(),
                (byte) NextIntegerLiteralTokenValue());
        }

        private WndDrawData ParseDrawData()
        {
            var items = new List<WndDrawDataItem>();
            while (Current.TokenType != WndTokenType.Semicolon)
            {
                items.Add(ParseDrawDataItem());

                if (Current.TokenType != WndTokenType.Semicolon)
                {
                    NextToken(WndTokenType.Comma);
                }
            }

            return new WndDrawData
            {
                Items = items.ToArray()
            };
        }

        private WndDrawDataItem ParseDrawDataItem()
        {
            var image = ParseAttribute("IMAGE", () => NextToken(WndTokenType.Identifier).StringValue);

            // BFME II has some .wnd files with missing commas.
            if (Current.TokenType == WndTokenType.Comma)
            {
                NextToken(WndTokenType.Comma);
            }

            var color = ParseAttribute("COLOR", ParseColor);

            NextToken(WndTokenType.Comma);

            var borderColor = ParseAttribute("BORDERCOLOR", ParseColor);

            return new WndDrawDataItem
            {
                Image = image,
                Color = color,
                BorderColor = borderColor
            };
        }

        private WndTextEntryData ParseTextEntryData()
        {
            var result = new WndTextEntryData();

            ParseAttributes(new Dictionary<string, Action>
            {
                { "MAXLEN", () => result.MaxLen = NextIntegerLiteralTokenValue() },
                { "SECRETTEXT", () => result.SecretText = ParseBoolean() },
                { "NUMERICALONLY", () => result.NumericalOnly = ParseBoolean() },
                { "ALPHANUMERICALONLY", () => result.AlphaNumericalOnly = ParseBoolean() },
                { "ASCIIONLY", () => result.AsciiOnly = ParseBoolean() }
            });

            return result;
        }

        private WndStaticTextData ParseStaticTextData()
        {
            var result = new WndStaticTextData();

            ParseAttributes(new Dictionary<string, Action>
            {
                { "CENTERED", () => result.Centered = ParseBoolean() }
            });

            return result;
        }

        private WndListBoxData ParseListBoxData()
        {
            var result = new WndListBoxData();

            var columnWidths = new List<int>();

            ParseAttributes(new Dictionary<string, Action>
            {
                { "LENGTH", () => result.Length = NextIntegerLiteralTokenValue() },
                { "AUTOSCROLL", () => result.AutoScroll = ParseBoolean() },
                { "SCROLLIFATEND", () => result.ScrollIfAtEnd = ParseBoolean() },
                { "AUTOPURGE", () => result.AutoPurge = ParseBoolean() },
                { "SCROLLBAR", () => result.ScrollBar = ParseBoolean() },
                { "MULTISELECT", () => result.MultiSelect = ParseBoolean() },
                { "COLUMNS", () => result.Columns = NextIntegerLiteralTokenValue() },
                { "COLUMNSWIDTH", () => columnWidths.Add(NextIntegerLiteralTokenValue()) },
                { "FORCESELECT", () => result.ForceSelect = ParseBoolean() }
            });

            result.ColumnWidths = columnWidths.ToArray();

            return result;
        }

        private WndRadioButtonData ParseRadioButtonData()
        {
            var result = new WndRadioButtonData();

            ParseAttributes(new Dictionary<string, Action>
            {
                { "GROUP", () => result.Group = NextIntegerLiteralTokenValue() }
            });

            return result;
        }

        private WndComboBoxData ParseComboBoxData()
        {
            var result = new WndComboBoxData();

            ParseAttributes(new Dictionary<string, Action>
            {
                { "ISEDITABLE", () => result.IsEditable = ParseBoolean() },
                { "MAXCHARS", () => result.MaxChars = NextIntegerLiteralTokenValue() },
                { "MAXDISPLAY", () => result.MaxDisplay = NextIntegerLiteralTokenValue() },
                { "ASCIIONLY", () => result.AsciiOnly = ParseBoolean() },
                { "LETTERSANDNUMBERS", () => result.LettersAndNumbers = ParseBoolean() }
            });

            return result;
        }

        private WndSliderData ParseSliderData()
        {
            var result = new WndSliderData();

            ParseAttributes(new Dictionary<string, Action>
            {
                { "MINVALUE", () => result.MinValue = NextIntegerLiteralTokenValue() },
                { "MAXVALUE", () => result.MaxValue = NextIntegerLiteralTokenValue() }
            });

            return result;
        }
    }
}
