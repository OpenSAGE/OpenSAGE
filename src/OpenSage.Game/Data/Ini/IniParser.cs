using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.FileFormats;
using OpenSage.FX;
using OpenSage.Graphics;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    internal sealed partial class IniParser
    {
        private static readonly Dictionary<string, Func<IniParser, IniToken>> MacroFunctions = new Dictionary<string, Func<IniParser, IniToken>>
        {
             { "#DIVIDE(", (parser) => { return parser.DivideFunc(); } },
             { "#ADD(", (parser) => { return parser.AddFunc(); } },
             { "#MULTIPLY(", (parser) => { return parser.MultiplyFunc(); } },
             { "#SUBTRACT(", (parser) => { return parser.SubtractFunc(); } },
        };

        private static readonly char[] Separators = { ' ', '\n', '\r', '\t', '=' };
        private static readonly char[] SeparatorsPercent = { ' ', '\n', '\r', '\t', '=', '%' };
        public static readonly char[] SeparatorsColon = { ' ', '\n', '\r', '\t', '=', ':' };
        private static readonly char[] SeparatorsQuote = { '"', '\n', '=' };

        public const string EndToken = "END";

        private TokenReader _tokenReader;

        private readonly string _directory;
        private readonly IniDataContext _dataContext;
        private readonly FileSystem _fileSystem;

        private readonly Stack<string> _currentBlockOrFieldStack;

        private readonly AssetStore _assetStore;

        // Used for some things that need temporary storage, like AliasConditionState.
        public object Temp { get; set; }

        public IniTokenPosition CurrentPosition => _tokenReader.CurrentPosition;

        public SageGame SageGame { get; }

        // Ini files with file name that ends with 9x use locale specific encoding.
        private readonly Encoding _encoding;

        public IniParser(FileSystemEntry entry, AssetStore assetStore, SageGame sageGame, IniDataContext dataContext, Encoding localeSpecificEncoding)
        {
            var iniEncoding = Encoding.ASCII;
            {
                // Use locale specific encoding if a "9x ini file" is present:
                // https://github.com/OpenSAGE/OpenSAGE/issues/405
                var localeSpecificFileName = Path.ChangeExtension(entry.FilePath, null) + "9x.ini";
                var localeSpecificEntry = entry.FileSystem.GetFile(localeSpecificFileName);
                if(localeSpecificEntry != null) 
                {
                    entry = localeSpecificEntry;
                    iniEncoding = localeSpecificEncoding;
                }
            }

            _directory = Path.GetDirectoryName(entry.FilePath);
            _dataContext = dataContext;
            _fileSystem = entry.FileSystem;
            _assetStore = assetStore;
            SageGame = sageGame;
            _encoding = iniEncoding;

            _tokenReader = CreateTokenReader(entry, _encoding);

            _currentBlockOrFieldStack = new Stack<string>();
        }

        private TokenReader CreateTokenReader(FileSystemEntry entry, Encoding encoding)
        {
            string source;

            if (entry != null)
            {
                using (var stream = entry.Open())
                using (var reader = new StreamReader(stream, encoding))
                {
                    source = reader.ReadToEnd();
                }
            }
            else
            {
                source = "";
            }

            return new TokenReader(source, entry.FullFilePath);
        }

        public void GoToNextLine() => _tokenReader.GoToNextLine();

        public string ParseQuotedString()
        {
            var token = GetNextTokenOptional();

            if (token == null)
            {
                return string.Empty;
            }

            if (!token.Value.Text.StartsWith("\""))
            {
                return token.Value.Text;
            }

            var firstPart = token.Value.Text.TrimStart('"');
            if (token.Value.Text.Length > 1 && firstPart.EndsWith("\""))
            {
                return firstPart.TrimEnd('"');
            }

            var restOfQuotedStringToken = GetNextToken(SeparatorsQuote);

            return firstPart + " " + restOfQuotedStringToken.Text;
        }

        public string ParseString()
        {
            var token = GetNextTokenOptional();

            if (token == null)
            {
                return string.Empty;
            }

            if (!token.Value.Text.StartsWith("\""))
            {
                return token.Value.Text;
            }

            if (token.Value.Text.Length > 1 && token.Value.Text.EndsWith("\""))
            {
                return token.Value.Text;
            }

            var restOfQuotedStringToken = GetNextToken(SeparatorsQuote);

            var restOfQuotedString = restOfQuotedStringToken.Text.TrimEnd('"');

            return token.Value.Text + " " + restOfQuotedString;
        }

        private static List<byte> ReadEncodedText(string encoded)
        {
            int ConvertHexNibble(char c)
            {
                if (c >= '0' && c <= '9')
                {
                    return c - '0';
                }
                return c - 'A' + 10;
            }

            var result = new List<byte>(encoded.Length / 2);
            var i = 0;

            while (i < encoded.Length)
            {
                if (encoded[i] == '_')
                {
                    var firstNibble = ConvertHexNibble(encoded[i + 1]);
                    var secondNibble = ConvertHexNibble(encoded[i + 2]);
                    var decodedByte = (byte) ((firstNibble << 4) | secondNibble);
                    result.Add(decodedByte);
                    i += 3;
                }
                else
                {
                    result.Add((byte) encoded[i]);
                    i += 1;
                }
            }

            return result;
        }

        public static string ToUnicodeString(string text)
        {
            var unicodeBytes = ReadEncodedText(text);

            return Encoding.Unicode.GetString(unicodeBytes.ToArray());
        }

        public static string ToAsciiString(string text)
        {
            var asciiBytes = ReadEncodedText(text);

            return Encoding.ASCII.GetString(asciiBytes.ToArray());
        }

        public string ParseUnicodeString()
        {
            var text = ParseString();

            return ToUnicodeString(text);
        }

        public string ScanAssetReference(in IniToken token) => token.Text;

        public string ParseAssetReference()
        {
            var token = GetNextTokenOptional();
            return token.HasValue ? token.Value.Text : "";
        }

        public string[] ParseAssetReferenceArray()
        {
            var result = new List<string>();

            IniToken? token;
            while ((token = GetNextTokenOptional()).HasValue)
            {
                result.Add(token.Value.Text);
            }

            return result.ToArray();
        }

        public T[] ParseAssetReferenceArray<T>(Dictionary<string, T> objects)
        {
            var result = new List<T>();

            IniToken? token;
            while ((token = GetNextTokenOptional()).HasValue)
            {
                var name = token.Value.Text;
                result.Add(objects[name]);
            }

            return result.ToArray();
        }

        public bool IsInteger(in IniToken token) => int.TryParse(token.Text, out _);

        public int GetIntegerOptional()
        {
            var token = GetNextTokenOptional();
            if (!token.HasValue)
            {
                return 0;
            }
            return ScanInteger(token.Value);
        }

        public int ScanInteger(in IniToken token) => ParseUtility.ParseInteger(token.Text);

        public int ParseInteger() => ScanInteger(GetNextToken());

        public int[] ParseIntegerArray()
        {
            var result = new List<int>();

            IniToken? token;
            while ((token = GetNextTokenOptional()) != null)
            {
                result.Add(ScanInteger(token.Value));
            }

            return result.ToArray();
        }

        public int[] ParseInLineIntegerArray()
        {
            var result = new List<int>();

            while (PeekInteger() != null)
            {
                result.Add(ParseInteger());
            }

            return result.ToArray();
        }

        public uint ScanUnsignedInteger(in IniToken token) => Convert.ToUInt32(token.Text);

        public uint ParseUnsignedInteger() => ScanUnsignedInteger(GetNextToken());

        private long ScanLong(in IniToken token)
        {
            try
            {
                return ParseUtility.ParseLong(token.Text);
            }
            catch (OverflowException)
            {
                return token.Text[0] == '-'
                    ? long.MinValue
                    : long.MaxValue;
            }
        }

        public long ParseLong() => ScanLong(GetNextToken());

        public byte ScanByte(in IniToken token) => (byte) ScanInteger(token);

        public byte ParseByte() => ScanByte(GetNextToken());

        private static string GetFloatText(in IniToken token)
        {
            var floatText = string.Empty;
            var seenDot = false;
            foreach (var c in token.Text)
            {
                if (!char.IsDigit(c) && c != '.' && c != '-' && c != '+')
                {
                    break;
                }
                if (c == '.')
                {
                    if (seenDot)
                    {
                        break;
                    }
                    else
                    {
                        seenDot = true;
                    }
                }
                floatText += c;
            }
            return floatText;
        }

        public bool IsFloat(in IniToken token)
        {
            var floatText = GetFloatText(token);
            return !string.IsNullOrEmpty(floatText) && ParseUtility.TryParseFloat(floatText, out _);
        }

        public float ScanFloat(in IniToken token)
        {
            return ParseUtility.ParseFloat(GetFloatText(token));
        }

        public float ParseFloat() => ScanFloat(GetNextToken());

        public float GetFloatOptional()
        {
            var token = GetNextTokenOptional();
            if (!token.HasValue)
            {
                return 0.0f;
            }

            return ScanFloat(token.Value);
        }

        public float[] ParseFloatArray()
        {
            var result = new List<float>();

            IniToken? token;
            while ((token = GetNextTokenOptional()) != null)
            {
                result.Add(ScanFloat(token.Value));
            }

            return result.ToArray();
        }

        private float ScanPercentage(in IniToken token) => ScanFloat(token);

        public Percentage ParsePercentage() => new Percentage(ScanPercentage(GetNextToken(SeparatorsPercent)) / 100.0f);

        private bool ScanBoolean(in IniToken token)
        {
            switch (token.Text.ToUpperInvariant())
            {
                case "YES":
                case "1":
                    return true;

                case "NO":
                case "0":
                    return false;

                default:
                    throw new IniParseException($"Invalid value for boolean: '{token.Text}'", token.Position);
            }
        }

        public bool ParseBoolean() => ScanBoolean(GetNextToken());

        public string ParseIdentifier()
        {
            return GetNextToken().Text;
        }

        public string ParseLocalizedStringKey()
        {
            return ParseIdentifier();
        }

        public string ParseOptionalLocalizedStringKey()
        {
            var token = GetNextTokenOptional();
            if (token.HasValue) return token.Value.Text;
            return "";
        }

        public string ParseFileName() => ParseIdentifier();

        public TimeSpan ParseTimeMilliseconds() => TimeSpan.FromMilliseconds(ParseInteger());
        public TimeSpan ParseTimeSeconds() => TimeSpan.FromSeconds(ParseInteger());

        public LazyAssetReference<CommandButton> ParseCommandButtonReference()
        {
            var name = ParseAssetReference();
            return _assetStore.CommandButtons.GetLazyAssetReferenceByName(name);
        }

        public LazyAssetReference<CommandSet> ParseCommandSetReference()
        {
            var name = ParseAssetReference();
            return _assetStore.CommandSets.GetLazyAssetReferenceByName(name);
        }

        public LazyAssetReference<UpgradeTemplate> ParseUpgradeReference()
        {
            var name = ParseAssetReference();
            return _assetStore.Upgrades.GetLazyAssetReferenceByName(name);
        }

        public LazyAssetReference<MappedImage> ParseMappedImageReference()
        {
            var name = ParseAssetReference();
            return _assetStore.MappedImages.GetLazyAssetReferenceByName(name);
        }

        public LazyAssetReference<FXParticleSystemTemplate> ParseFXParticleSystemTemplateReference()
        {
            var name = ParseAssetReference();
            return _assetStore.FXParticleSystemTemplates.GetLazyAssetReferenceByName(name);
        }

        public LazyAssetReference<LocomotorTemplate> ParseLocomotorTemplateReference()
        {
            var name = ParseAssetReference();
            return _assetStore.LocomotorTemplates.GetLazyAssetReferenceByName(name);
        }

        public LazyAssetReference<WeaponTemplate> ParseWeaponTemplateReference()
        {
            var name = ParseAssetReference();
            return _assetStore.WeaponTemplates.GetLazyAssetReferenceByName(name);
        }

        public LazyAssetReference<ArmorTemplate> ParseArmorTemplateReference()
        {
            var name = ParseAssetReference();
            return _assetStore.ArmorTemplates.GetLazyAssetReferenceByName(name);
        }

        public LazyAssetReference<FXList> ParseFXListReference()
        {
            var name = ParseAssetReference();
            return _assetStore.FXLists.GetLazyAssetReferenceByName(name);
        public LazyAssetReference<DamageFX> ParseDamageFXReference()
        {
            var name = ParseAssetReference();
            return _assetStore.DamageFXs.GetLazyAssetReferenceByName(name);
        }

        }

        public LazyAssetReference<Graphics.Animation.W3DAnimation>[] ParseAnimationReferenceArray()
        {
            var result = new List<LazyAssetReference<Graphics.Animation.W3DAnimation>>();

            IniToken? token;
            while ((token = GetNextTokenOptional()).HasValue)
            {
                result.Add(_assetStore.ModelAnimations.GetLazyAssetReferenceByName(token.Value.Text));
            }

            return result.ToArray();
        }

        public LazyAssetReference<LocomotorTemplate>[] ParseLocomotorTemplateReferenceArray()
        {
            var result = new List<LazyAssetReference<LocomotorTemplate>>();

            IniToken? token;
            while ((token = GetNextTokenOptional()).HasValue)
            {
                result.Add(_assetStore.LocomotorTemplates.GetLazyAssetReferenceByName(token.Value.Text));
            }

            return result.ToArray();
        }

        public LazyAssetReference<BaseAudioEventInfo> ParseAudioEventReference()
        {
            var name = ParseAssetReference();
            return new LazyAssetReference<BaseAudioEventInfo>(() => _assetStore.AudioEvents.GetByName(name));
        }

        public LazyAssetReference<AudioFile> ParseAudioFileReference()
        {
            var name = ParseAssetReference();
            return _assetStore.AudioFiles.GetLazyAssetReferenceByName(name);
        }

        public AudioFileWithWeight[] ParseAudioFileWithWeightArray()
        {
            var result = new List<AudioFileWithWeight>();

            IniToken? token;
            while ((token = GetNextTokenOptional()).HasValue)
            {
                result.Add(new AudioFileWithWeight
                {
                    AudioFile = _assetStore.AudioFiles.GetLazyAssetReferenceByName(token.Value.Text)
                });
            }

            return result.ToArray();
        }

        public LazyAssetReference<ObjectDefinition> ParseObjectReference()
        {
            var name = ParseAssetReference();

            return (!string.Equals(name, "NONE", StringComparison.OrdinalIgnoreCase))
                ? _assetStore.ObjectDefinitions.GetLazyAssetReferenceByName(name)
                : null;
        }

        public LazyAssetReference<ObjectDefinition>[] ParseObjectReferenceArray()
        {
            var result = new List<LazyAssetReference<ObjectDefinition>>();

            IniToken? token;
            while ((token = GetNextTokenOptional()).HasValue)
            {
                result.Add(_assetStore.ObjectDefinitions.GetLazyAssetReferenceByName(token.Value.Text));
            }

            return result.ToArray();
        }

        public LazyAssetReference<TextureAsset> ParseTextureReference()
        {
            var fileName = ParseFileName();
            return _assetStore.Textures.GetLazyAssetReferenceByName(fileName);
        }

        public LazyAssetReference<GuiTextureAsset> ParseGuiTextureReference()
        {
            var fileName = ParseFileName();
            return _assetStore.GuiTextures.GetLazyAssetReferenceByName(fileName);
        }

        public LazyAssetReference<Model> ParseModelReference()
        {
            var fileName = ParseFileName();
            return (!string.Equals(fileName, "NONE", StringComparison.OrdinalIgnoreCase))
                ? _assetStore.Models.GetLazyAssetReferenceByName(fileName)
                : null;
        }

        public LazyAssetReference<Graphics.Animation.W3DAnimation> ParseAnimationReference()
        {
            var animationName = ParseAnimationName();
            return (!string.Equals(animationName, "NONE", StringComparison.OrdinalIgnoreCase))
                ? _assetStore.ModelAnimations.GetLazyAssetReferenceByName(animationName)
                : null;
        }

        public string ScanBoneName(in IniToken token) => token.Text;
        public string ParseBoneName() => ScanBoneName(GetNextToken());

        public string[] ParseBoneNameArray() => ParseAssetReferenceArray();
        public string ParseAnimationName() => ParseIdentifier();

        public ColorRgb ParseColorRgb()
        {
            var r = ParseAttributeByte("R");
            var g = ParseAttributeByte("G");
            var b = ParseAttributeByte("B");
            return new ColorRgb(r, g, b);
        }

        public ColorRgba ParseColorRgba()
        {
            var r = ParseAttributeByte("R");
            var g = ParseAttributeByte("G");
            var b = ParseAttributeByte("B");

            var aToken = GetNextTokenOptional(SeparatorsColon);
            var a = (byte) 255;
            if (aToken != null)
            {
                if (aToken.Value.Text != "A")
                {
                    throw new IniParseException($"Expected attribute name 'A'", aToken.Value.Position);
                }
                a = ScanByte(GetNextToken(SeparatorsColon));
            }

            return new ColorRgba(r, g, b, a);
        }

        public Point2D ParsePoint()
        {
            return new Point2D(
                ParseAttributeInteger("X"),
                ParseAttributeInteger("Y"));
        }

        public Vector2 ParseVector2()
        {
            return new Vector2
            {
                X = ParseAttributeFloat("X"),
                Y = ParseAttributeFloat("Y")
            };
        }

        public Vector3 ParseVector3()
        {
            return new Vector3
            {
                X = ParseAttributeFloat("X"),
                Y = ParseAttributeFloat("Y"),
                Z = ParseAttributeFloat("Z")
            };
        }

        public Size ParseSize()
        {
            return new Size(
                ParseAttributeInteger("X"),
                ParseAttributeInteger("Y"));
        }

        public IntRange ParseIntRange()
        {
            return new IntRange(ParseInteger(), ParseInteger());
        }

        public FloatRange ParseFloatRange()
        {
            return new FloatRange(ParseFloat(), ParseFloat());
        }

        public RandomVariable ParseRandomVariable()
        {
            var low = ParseFloat();
            var high = ParseFloat();

            var distributionTypeToken = GetNextTokenOptional();
            var distributionType = (distributionTypeToken != null)
                ? IniParser.ScanEnum<DistributionType>(distributionTypeToken.Value)
                : DistributionType.Uniform;

            return new RandomVariable(low, high, distributionType);
        }

        public IniToken GetNextToken(char[] separators = null)
        {
            var result = GetNextTokenOptional(separators);
            if (result == null)
            {
                throw new IniParseException("Expected a token", _tokenReader.CurrentPosition);
            }

            return result.Value;
        }

        private bool ResolveFunc(string text, out IniToken? resolved)
        {
            if (!text.StartsWith("#") || text.StartsWith("#(")) //hacky for e.g. #(MODEL)_WLK as animation names
            {
                resolved = null;
                return false;
            }

            resolved = MacroFunctions[text](this);
            return true;
        }

        private IniToken DivideFunc()
        {
            return Func(ParseFloat() / ParseFloat());
        }

        private IniToken AddFunc()
        {
            return Func(ParseFloat() + ParseFloat());
        }

        private IniToken MultiplyFunc()
        {
            return Func(ParseFloat() * ParseFloat());
        }

        private IniToken SubtractFunc()
        {
            return Func(ParseFloat() - ParseFloat());
        }

        private IniToken Func(float value)
        {
            GetNextToken(); //read the ')'
            return new IniToken(ParseUtility.ToInvariant(value), _tokenReader.CurrentPosition);
        }

        public IniToken? GetNextTokenOptional(char[] separators = null)
        {
            var result = _tokenReader.NextToken(separators ?? Separators);

            if (result.HasValue)
            {
                if (_dataContext.Defines.TryGetValue(result.Value.Text.ToUpper(), out var macroExpansion))
                {
                    return macroExpansion;
                }
                else if (ResolveFunc(result.Value.Text, out var funcResult))
                {
                    return funcResult.Value;
                }
            }

            return result;
        }

        public IniToken? PeekNextTokenOptional(char[] separators = null)
        {
            return _tokenReader.PeekToken(separators ?? Separators);
        }

        public int? PeekInteger()
        {
            var token = PeekNextTokenOptional();

            if (token.HasValue && int.TryParse(token.Value.Text, out var integer))
            {
                return integer;
            }

            return null;
        }

        public T ParseNamedBlock<T>(
            Action<T, string> setNameCallback,
            IIniFieldParserProvider<T> fieldParserProvider,
            IIniFieldParserProvider<T> fieldParserProviderFallback = null)
            where T : class, new()
        {
            var result = new T();

            var name = GetNextToken();

            setNameCallback(result, name.Text);

            ParseBlockContent(result, fieldParserProvider, false, fieldParserProviderFallback);

            return result;
        }

        public T ParseIndexedBlock<T>(
            Action<T, int> setNameCallback,
            IIniFieldParserProvider<T> fieldParserProvider)
            where T : class, new()
        {
            var result = new T();

            var name = ParseInteger();
            setNameCallback(result, name);

            ParseBlockContent(result, fieldParserProvider);

            return result;
        }

        public T ParseTopLevelBlock<T>(
            IIniFieldParserProvider<T> fieldParserProvider)
            where T : class, new()
        {
            var result = ParseBlock(fieldParserProvider);

            return result;
        }

        public T ParseBlock<T>(
           IIniFieldParserProvider<T> fieldParserProvider)
           where T : class, new()
        {
            var result = new T();

            ParseBlockContent(result, fieldParserProvider);

            return result;
        }

        public bool ParseBlockContent<T>(
            T result,
            IIniFieldParserProvider<T> fieldParserProvider,
            bool isIncludedBlock = false,
            IIniFieldParserProvider<T> fieldParserProviderFallback = null)
            where T : class, new()
        {
            var done = false;
            var reachedEndOfBlock = false;
            while (!done)
            {
                if (_tokenReader.EndOfFile)
                {
                    if (!isIncludedBlock)
                    {
                        throw new InvalidOperationException();
                    }
                    done = true;
                    continue;
                }

                _tokenReader.GoToNextLine();

                var token = _tokenReader.NextToken(Separators);

                if (token == null)
                {
                    continue;
                }

                if (string.Equals(token.Value.Text, EndToken, StringComparison.InvariantCultureIgnoreCase))
                {
                    reachedEndOfBlock = true;
                    done = true;
                }
                else if (token.Value.Text == "#include")
                {
                    done = ParseIncludedFile(result, fieldParserProvider);
                }
                else
                {
                    var fieldName = token.Value.Text;

                    if (fieldParserProvider.TryGetFieldParser(fieldName, out var fieldParser))
                    {
                        _currentBlockOrFieldStack.Push(fieldName);
                        fieldParser(this, result);
                        _currentBlockOrFieldStack.Pop();
                    }
                    else if (fieldParserProviderFallback != null && fieldParserProviderFallback.TryGetFieldParser(fieldName, out var fieldParserFallback))
                    {
                        _currentBlockOrFieldStack.Push(fieldName);
                        fieldParserFallback(this, result);
                        _currentBlockOrFieldStack.Pop();
                    }
                    else
                    {
                        throw new IniParseException($"Unexpected field '{fieldName}' in block '{_currentBlockOrFieldStack.Peek()}'.", token.Value.Position);
                    }
                }
            }

            return reachedEndOfBlock;
        }

        private bool ParseIncludedFile<T>(T result, IIniFieldParserProvider<T> fieldParserProvider) where T : class, new()
        {
            var includeFileName = ParseQuotedString();
            var directory = _directory;
            while (includeFileName.StartsWith(".."))
            {
                includeFileName = includeFileName.Remove(0, 3);
                directory = directory.Substring(0, directory.LastIndexOf(Path.DirectorySeparatorChar));
            }
            if (includeFileName.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                includeFileName = includeFileName.Remove(0, 1);
            }

            var path = Path.Combine(directory, includeFileName);
            var includeEntry = _fileSystem.GetFile(path);
            // I doubt locale-specific-encoded files will ever include other files.
            // But if they do, it's reasonable to assume included files use the same encoding as the includer.
            var tokenReader = CreateTokenReader(includeEntry, _encoding);

            var original = _tokenReader;
            bool reachedEndOfBlock;
            try
            {
                _tokenReader = tokenReader;
                reachedEndOfBlock = ParseBlockContent(result, fieldParserProvider, isIncludedBlock: true);
            }
            finally
            {
                _tokenReader = original;
            }

            return reachedEndOfBlock;
        }

        public void ParseFile()
        {
            while (!_tokenReader.EndOfFile)
            {
                _tokenReader.GoToNextLine();

                var token = _tokenReader.NextToken(Separators);

                if (token == null)
                {
                    continue;
                }

                var fieldName = token.Value.Text;

                if (fieldName == "#define")
                {
                    var macroName = ParseIdentifier();
                    var macroExpansionToken = _tokenReader.NextToken(Separators);
                    if (ResolveFunc(macroExpansionToken.Value.Text, out var resolved))
                    {
                        macroExpansionToken = resolved;
                    }

                    _dataContext.Defines.Add(macroName.ToUpper(), macroExpansionToken.Value);
                }
                else if (fieldName == "#include")
                {
                    var includeFileName = ParseQuotedString();
                    if (includeFileName.StartsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        includeFileName = includeFileName.Remove(0, 1);
                    }

                    var includePath = Path.Combine(_directory, includeFileName);
                    var includeEntry = _fileSystem.GetFile(includePath);
                    // I doubt locale-specific-encoded files will ever include other files.
                    // But if they do, it's reasonable to assume included files use the same encoding as the includer.
                    var includeParser = new IniParser(includeEntry, _assetStore, SageGame, _dataContext, _encoding);
                    includeParser.ParseFile();
                }
                else if (BlockParsers.TryGetValue(fieldName, out var blockParser))
                {
                    _currentBlockOrFieldStack.Push(fieldName);
                    blockParser(this, _assetStore);
                    _currentBlockOrFieldStack.Pop();
                }
                else
                {
                    throw new IniParseException($"Unexpected block '{fieldName}'.", token.Value.Position);
                }
            }
        }
    }
}
