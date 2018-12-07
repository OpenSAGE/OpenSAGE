using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Microsoft.Extensions.DependencyModel.Resolution;
using OpenSage.Data.Utilities;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini.Parser
{
    internal sealed partial class IniParser
    {
        private static readonly Dictionary<string, Action<IniParser, IniDataContext>> BlockParsers = new Dictionary<string, Action<IniParser, IniDataContext>>
        {
            { "AIData", (parser, context) => context.AIData = AIData.Parse(parser) },
            { "AmbientStream", (parser, context) => context.AmbientStreams.Add(AmbientStream.Parse(parser)) },
            { "Animation", (parser, context) => context.Animations.Add(Animation.Parse(parser)) },
            { "AnimationSoundClientBehaviorGlobalSetting", (parser, context) => context.AnimationSoundClientBehaviorGlobalSetting = AnimationSoundClientBehaviorGlobalSetting.Parse(parser) },
            { "Armor", (parser, context) => context.Armors.Add(Armor.Parse(parser)) },
            { "AudioEvent", (parser, context) =>
                {
                    var ev = AudioEvent.Parse(parser);
                    context.AudioEvents[ev.Name] = ev;
                }
            },
            { "AudioLOD", (parser, context) => context.AudioLods.Add(AudioLod.Parse(parser)) },
            { "AudioLowMHz", (parser, context) => context.AudioLowMHz = parser.ParseInteger() },
            { "AudioSettings", (parser, context) => context.AudioSettings = AudioSettings.Parse(parser) },
            { "BannerType", (parser, context) => context.BannerTypes.Add(BannerType.Parse(parser)) },
            { "BannerUI", (parser, context) => context.BannerUI = BannerUI.Parse(parser) },
            { "BenchProfile", (parser, context) => context.BenchProfiles.Add(BenchProfile.Parse(parser)) },
            { "Bridge", (parser, context) => context.Bridges.Add(BridgeTemplate.Parse(parser)) },
            { "Campaign", (parser, context) => context.Campaigns.Add(Campaign.Parse(parser)) },
            { "ChallengeGenerals", (parser, context) => context.ChallengeGenerals = ChallengeGenerals.Parse(parser) },
            { "ChildObject", (parser, context) => context.Objects.Add(ChildObject.Parse(parser)) },
            { "CloudEffect", (parser, context) => context.Environment.CloudEffect = CloudEffect.Parse(parser) },
            { "CommandButton", (parser, context) => context.CommandButtons.Add(CommandButton.Parse(parser)) },
            { "CommandMap", (parser, context) => context.CommandMaps.Add(CommandMap.Parse(parser)) },
            { "CommandSet", (parser, context) => context.CommandSets.Add(CommandSet.Parse(parser)) },
            { "ControlBarResizer", (parser, context) => context.ControlBarResizers.Add(ControlBarResizer.Parse(parser)) },
            { "ControlBarScheme", (parser, context) => context.ControlBarSchemes.Add(ControlBarScheme.Parse(parser)) },
            { "CrateData", (parser, context) => context.CrateDatas.Add(CrateData.Parse(parser)) },
            { "Credits", (parser, context) => context.Credits = Credits.Parse(parser) },
            { "DamageFX", (parser, context) => context.DamageFXs.Add(DamageFX.Parse(parser)) },
            { "DialogEvent", (parser, context) => context.DialogEvents.Add(DialogEvent.Parse(parser)) },
            { "DrawGroupInfo", (parser, context) => context.DrawGroupInfo = DrawGroupInfo.Parse(parser) },
            { "DynamicGameLOD", (parser, context) => context.DynamicGameLods.Add(DynamicGameLod.Parse(parser)) },
            { "EmotionNugget", (parser, context) => context.EmotionNuggets.Add(EmotionNugget.Parse(parser)) },
            { "EvaEvent", (parser, context) => context.EvaEvents.Add(EvaEvent.Parse(parser)) },
            { "ExperienceLevel", (parser, context) => context.ExperienceLevels.Add(ExperienceLevel.Parse(parser)) },
            { "ExperienceScalarTable", (parser, context) => context.ExperienceScalarTables.Add(ExperienceScalarTable.Parse(parser)) },
            { "FactionVictoryData", (parser, context) => context.FactionVictoryDatas.Add(FactionVictoryData.Parse(parser)) },
            { "FireEffect", (parser, context) => context.Environment.FireEffect = RingEffect.Parse(parser) },
            { "FontDefaultSettings", (parser, context) => context.FontDefaultSettings.Add(FontDefaultSetting.Parse(parser)) },
            { "FontSubstitution", (parser, context) => context.FontSubstitutions.Add(FontSubstitution.Parse(parser)) },
            { "FXList", (parser, context) => context.FXLists.Add(FXList.Parse(parser)) },
            { "FXParticleSystem", (parser, context) => context.FXParticleSystems.Add(FXParticleSystemTemplate.Parse(parser)) },
            { "GameData", (parser, context) => context.GameData = GameData.Parse(parser) },
            { "GlowEffect", (parser, context) => context.Environment.GlowEffect = GlowEffect.Parse(parser) },
            { "HeaderTemplate", (parser, context) => context.HeaderTemplates.Add(HeaderTemplate.Parse(parser)) },
            { "HouseColor", (parser, context) => context.HouseColors.Add(HouseColor.Parse(parser)) },
            { "InGameUI", (parser, context) => context.InGameUI = InGameUI.Parse(parser) },
            { "Language", (parser, context) => context.Language = Language.Parse(parser) },
            { "LargeGroupAudioMap", (parser, context) => context.LargeGroupAudioMaps.Add(LargeGroupAudioMap.Parse(parser)) },
            { "LivingWorldAnimObject", (parser, context) => context.LivingWorldAnimObjects.Add(LivingWorldAnimObject.Parse(parser)) },
            { "LivingWorldArmyIcon", (parser, context) => context.LivingWorldArmyIcons.Add(LivingWorldArmyIcon.Parse(parser)) },
            { "LargeGroupAudioUnusedKnownKeys", (parser, context) => context.LargeGroupAudioUnusedKnownKeys = LargeGroupAudioUnusedKnownKeys.Parse(parser) },
            { "LivingWorldCampaign", (parser, context) => context.LivingWorldCampaigns.Add(LivingWorldCampaign.Parse(parser)) },
            { "LivingWorldMapInfo", (parser, context) => context.LivingWorldMapInfo = LivingWorldMapInfo.Parse(parser) },
            { "LivingWorldPlayerArmy", (parser, context) => context.LivingWorldPlayerArmies.Add(LivingWorldPlayerArmy.Parse(parser)) },
            { "LivingWorldRegionCampaign", (parser, context) => context.LivingWorldRegionCampaigns.Add(LivingWorldRegionCampaign.Parse(parser)) },
            { "LivingWorldSound", (parser, context) => context.LivingWorldSounds.Add(LivingWorldSound.Parse(parser)) },
            { "LoadSubsystem", (parser, context) => context.Subsystems.Add(LoadSubsystem.Parse(parser)) },
            { "Locomotor", (parser, context) => context.Locomotors.Add(Locomotor.Parse(parser)) },
            { "LODPreset", (parser, context) => context.LodPresets.Add(LodPreset.Parse(parser)) },
            { "MapCache", (parser, context) => context.MapCaches.Add(MapCache.Parse(parser)) },
            { "MappedImage", (parser, context) => context.MappedImages.Add(MappedImage.Parse(parser)) },
            { "MiscAudio", (parser, context) => context.MiscAudio = MiscAudio.Parse(parser) },
            { "MiscEvaData", (parser, context) => context.MiscEvaData = MiscEvaData.Parse(parser) },
            { "ModifierList", (parser, context) => context.ModifierLists.Add(ModifierList.Parse(parser)) },
            { "Mouse", (parser, context) => context.MouseData = MouseData.Parse(parser) },
            { "MouseCursor", (parser, context) => context.MouseCursors.Add(MouseCursor.Parse(parser)) },
            { "MultiplayerColor", (parser, context) => context.MultiplayerColors.Add(MultiplayerColor.Parse(parser)) },
            { "MultiplayerSettings", (parser, context) => context.MultiplayerSettings = MultiplayerSettings.Parse(parser) },
            { "MultiplayerStartingMoneyChoice", (parser, context) => context.MultiplayerStartingMoneyChoices.Add(MultiplayerStartingMoneyChoice.Parse(parser)) },
            { "Multisound", (parser, context) => context.Multisounds.Add(Multisound.Parse(parser)) },
            { "MusicTrack", (parser, context) => context.MusicTracks.Add(MusicTrack.Parse(parser)) },
            { "NewEvaEvent", (parser, context) => context.EvaEvents.Add(EvaEvent.Parse(parser)) },
            { "Object", (parser, context) => context.Objects.Add(ObjectDefinition.Parse(parser)) },
            { "ObjectReskin", (parser, context) => context.Objects.Add(ObjectDefinition.ParseReskin(parser)) },
            { "ObjectCreationList", (parser, context) => context.ObjectCreationLists.Add(ObjectCreationList.Parse(parser)) },
            { "OnlineChatColors", (parser, context) => context.OnlineChatColors = OnlineChatColors.Parse(parser) },
            { "ParticleSystem", (parser, context) => context.ParticleSystems.Add(ParticleSystemDefinition.Parse(parser)) },
            { "PlayerTemplate", (parser, context) => context.PlayerTemplates.Add(PlayerTemplate.Parse(parser)) },
            { "PredefinedEvaEvent", (parser, context) => context.EvaEvents.Add(EvaEvent.Parse(parser)) },
            { "Rank", (parser, context) => context.Ranks.Add(Rank.Parse(parser)) },
            { "RingEffect", (parser, context) => context.Environment.RingEffect = RingEffect.Parse(parser) },
            { "Road", (parser, context) => context.RoadTemplates.Add(RoadTemplate.Parse(parser)) },
            { "ReallyLowMHz", (parser, context) => context.ReallyLowMHz = parser.ParseInteger() },
            { "Science", (parser, context) => context.Sciences.Add(Science.Parse(parser)) },
            { "ShellMenuScheme", (parser, context) => context.ShellMenuSchemes.Add(ShellMenuScheme.Parse(parser)) },
            { "SkyboxTextureSet", (parser, context) => context.SkyboxTextureSets.Add(SkyboxTextureSet.Parse(parser)) },
            { "SpecialPower", (parser, context) => context.SpecialPowers.Add(SpecialPower.Parse(parser)) },
            { "StreamedSound", (parser, context) => context.StreamedSounds.Add(StreamedSound.Parse(parser)) },
            { "StaticGameLOD", (parser, context) => context.StaticGameLods.Add(StaticGameLod.Parse(parser)) },
            { "Terrain", (parser, context) => context.TerrainTextures.Add(TerrainTexture.Parse(parser)) },
            { "Upgrade", (parser, context) => context.Upgrades.Add(Upgrade.Parse(parser)) },
            { "VictorySystemData", (parser, context) => context.VictorySystemDatas.Add(VictorySystemData.Parse(parser)) },
            { "Video", (parser, context) => context.Videos.Add(Video.Parse(parser)) },
            { "WaterSet", (parser, context) => context.WaterSets.Add(WaterSet.Parse(parser)) },
            { "WaterTextureList", (parser, context) => context.WaterTextureLists.Add(WaterTextureList.Parse(parser)) },
            { "WaterTransparency", (parser, context) => context.WaterTransparency = WaterTransparency.Parse(parser) },
            { "Weapon", (parser, context) => context.Weapons.Add(Weapon.Parse(parser)) },
            { "Weather", (parser, context) => context.Weather = Weather.Parse(parser) },
            { "WebpageURL", (parser, context) => context.WebpageUrls.Add(WebpageUrl.Parse(parser)) },
            { "WindowTransition", (parser, context) => context.WindowTransitions.Add(WindowTransition.Parse(parser)) },
        };

        private static readonly Dictionary<string, Func<IniParser, IniToken>> MacroFunctions = new Dictionary<string, Func<IniParser, IniToken>>
        {
             { "#DIVIDE(", (parser) => { return parser.DivideFunc(); } },
             { "#ADD(", (parser) => { return parser.AddFunc(); } },
             { "#MULTIPLY(", (parser) => { return parser.MultiplyFunc(); } },
        };

        private static readonly char[] Separators = { ' ', '\n', '\r', '\t', '=' };
        private static readonly char[] SeparatorsPercent = { ' ', '\n', '\r', '\t', '=', '%' };
        public static readonly char[] SeparatorsColon = { ' ', '\n', '\r', '\t', '=', ':' };
        private static readonly char[] SeparatorsQuote = { '"', '\n', '=' };

        public const string EndToken = "END";

        private TokenReader _tokenReader;

        private readonly string _directory;
        private readonly IniDataContext _dataContext;

        private readonly Stack<string> _currentBlockOrFieldStack;

        // Used for some things that need temporary storage, like AliasConditionState.
        public object Temp { get; set; }

        public IniTokenPosition CurrentPosition => _tokenReader.CurrentPosition;

        public SageGame SageGame { get; private set; }

        public IniParser(string source, FileSystemEntry entry, IniDataContext dataContext, SageGame game)
        {
            _directory = Path.GetDirectoryName(entry.FilePath);
            _dataContext = dataContext;
            SageGame = game;

            _tokenReader = new TokenReader(source, Path.Combine(entry.FileSystem.RootDirectory, entry.FilePath));

            _currentBlockOrFieldStack = new Stack<string>();
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

        public string ParseUnicodeString()
        {
            var text = ParseString();

            int ConvertHexNibble(char c)
            {
                if (c >= '0' && c <= '9')
                {
                    return c - '0';
                }
                return c - 'A' + 10;
            }

            var unicodeBytes = new List<byte>(text.Length / 2);
            var i = 0;

            while (i < text.Length)
            {
                if (text[i] == '_')
                {
                    var firstNibble = ConvertHexNibble(text[i + 1]);
                    var secondNibble = ConvertHexNibble(text[i + 2]);
                    var decodedByte = (byte) ((firstNibble << 4) | secondNibble);
                    unicodeBytes.Add(decodedByte);
                    i += 3;
                }
                else
                {
                    unicodeBytes.Add((byte) text[i]);
                    i += 1;
                }
            }
            
            return Encoding.Unicode.GetString(unicodeBytes.ToArray());
        }

        public string ScanAssetReference(IniToken token) => token.Text;

        public string ParseAssetReference()
        {
            var token = GetNextTokenOptional();
            if (token.HasValue)
                return token.Value.Text;
            return "";
        }

        public string[] ParseAssetReferenceArray()
        {
            var result = new List<string>();

            IniToken? token;
            while ((token = GetNextTokenOptional()) != null)
            {
                result.Add(token.Value.Text);
            }

            return result.ToArray();
        }

        public bool IsInteger(IniToken token) => int.TryParse(token.Text, out _);

        public int GetIntegerOptional()
        {
            var token = GetNextTokenOptional();
            if (!token.HasValue)
            {
                return 0;
            }

            if (_dataContext.Defines.TryGetValue(token.Value.Text, out var macroExpansion))
            {
                token =  macroExpansion;
            }
            return ScanInteger(token.Value);
        }

        public int ScanInteger(IniToken token) => Convert.ToInt32(token.Text);

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

        public uint ScanUnsignedInteger(IniToken token) => Convert.ToUInt32(token.Text);

        public uint ParseUnsignedInteger() => ScanUnsignedInteger(GetNextToken());

        private long ScanLong(IniToken token) => Convert.ToInt64(token.Text);

        public long ParseLong() => ScanLong(GetNextToken());

        public byte ScanByte(IniToken token) => (byte) ScanInteger(token);

        public byte ParseByte() => ScanByte(GetNextToken());

        private static string GetFloatText(IniToken token)
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

        public bool IsFloat(IniToken token)
        {
            var floatText = GetFloatText(token);
            return !string.IsNullOrEmpty(floatText) && ParseUtility.TryParseFloat(floatText, out _);
        }

        public float ScanFloat(IniToken token)
        {
            if (ResolveFunc(token.Text, out var funcResult))
            {
                token = funcResult.Value;
            }
            return ParseUtility.ParseFloat(GetFloatText(token));
        }

        public float ParseFloat() => ScanFloat(GetNextToken());

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

        private float ScanPercentage(IniToken token) => ScanFloat(token);

        public float ParsePercentage() => ScanPercentage(GetNextToken(SeparatorsPercent));

        private bool ScanBoolean(IniToken token)
        {
            switch (token.Text.ToUpperInvariant())
            {
                case "YES":
                    return true;

                case "NO":
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

        public string ParseFileName() => ParseIdentifier();

        public string ScanBoneName(IniToken token) => token.Text;
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

        public IniToken GetNextToken(char[] separators = null)
        {
            var result = GetNextTokenOptional(separators);
            if (result == null)
            {
                throw new IniParseException("Expected a token", _tokenReader.CurrentPosition);
            }

            if (_dataContext.Defines.TryGetValue(result.Value.Text, out var macroExpansion))
            {
                return macroExpansion;
            }

            if (ResolveFunc(result.Value.Text, out var funcResult))
            {
                return funcResult.Value;
            }

            return result.Value;
        }

        private bool ResolveFunc(string text, out IniToken? resolved)
        {
            if (!text.StartsWith("#"))
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

        private IniToken Func(float value)
        {
            GetNextToken(); //read the ')'
            return new IniToken(ParseUtility.ToInvariant(value), _tokenReader.CurrentPosition);
        }

        public IniToken? GetNextTokenOptional(char[] separators = null)
        {
            var result = _tokenReader.NextToken(separators ?? Separators);

            if (result.HasValue && _dataContext.Defines.TryGetValue(result.Value.Text, out var macroExpansion))
            {
                return macroExpansion;
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

        public T ParseTopLevelNamedBlock<T>(
            Action<T, string> setNameCallback,
            IIniFieldParserProvider<T> fieldParserProvider)
            where T : class, new()
        {
            var result = ParseNamedBlock(setNameCallback, fieldParserProvider);

            return result;
        }

        public T ParseTopLevelNamedBlock<T>(
            Action<T, int> setNameCallback,
            IIniFieldParserProvider<T> fieldParserProvider)
            where T : class, new()
        {
            var result = ParseNamedBlock(setNameCallback, fieldParserProvider);

            return result;
        }

        public T ParseNamedBlock<T>(
            Action<T, string> setNameCallback,
            IIniFieldParserProvider<T> fieldParserProvider)
            where T : class, new()
        {
            var result = new T();

            var name = GetNextToken();

            setNameCallback(result, name.Text);

            ParseBlockContent(result, fieldParserProvider);

            return result;
        }

        public T ParseNamedBlock<T>(
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

        private bool ParseBlockContent<T>(
            T result,
           IIniFieldParserProvider<T> fieldParserProvider,
           bool isIncludedBlock = false)
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

                if (token == null) continue;

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
                directory = directory.Substring(0, directory.LastIndexOf('\\'));
            }

            var path = Path.Combine(directory, includeFileName);
            var content = _dataContext.GetIniFileContent(path);
            var tokenReader = new TokenReader(content, path);
            var copy = _tokenReader;
            _tokenReader = tokenReader;
            var reachedEndOfBlock = ParseBlockContent(result, fieldParserProvider, isIncludedBlock: true);
            _tokenReader = copy;
            return reachedEndOfBlock;
        }

        public void ParseFile()
        {
            while (!_tokenReader.EndOfFile)
            {
                _tokenReader.GoToNextLine();

                var token = _tokenReader.NextToken(Separators);

                if (token == null) continue;

                var fieldName = token.Value.Text;

                if (fieldName == "#define")
                {
                    var macroName = ParseIdentifier();
                    var macroExpansion = _tokenReader.NextToken(Separators);

                    _dataContext.Defines.Add(macroName, macroExpansion.Value);
                }
                else if (fieldName == "#include")
                {
                    var includeFileName = ParseQuotedString();
                    _dataContext.LoadIniFile(Path.Combine(_directory, includeFileName));
                }
                else if (BlockParsers.TryGetValue(fieldName, out var blockParser))
                {
                    _currentBlockOrFieldStack.Push(fieldName);
                    blockParser(this, _dataContext);
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
