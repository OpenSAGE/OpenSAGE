using System;
using System.Collections.Generic;
using System.IO;
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
            { "AudioEvent", (parser, context) => context.AudioEvents.Add(AudioEvent.Parse(parser)) },
            { "AudioSettings", (parser, context) => context.AudioSettings = AudioSettings.Parse(parser) },
            { "BannerType", (parser, context) => context.BannerTypes.Add(BannerType.Parse(parser)) },
            { "BannerUI", (parser, context) => context.BannerUI = BannerUI.Parse(parser) },
            { "BenchProfile", (parser, context) => context.BenchProfiles.Add(BenchProfile.Parse(parser)) },
            { "Bridge", (parser, context) => context.Bridges.Add(Bridge.Parse(parser)) },
            { "Campaign", (parser, context) => context.Campaigns.Add(Campaign.Parse(parser)) },
            { "ChallengeGenerals", (parser, context) => context.ChallengeGenerals = ChallengeGenerals.Parse(parser) },
            { "ChildObject", (parser, context) => context.Objects.Add(ChildObject.Parse(parser)) },
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
            { "EvaEvent", (parser, context) => context.EvaEvents.Add(EvaEvent.Parse(parser)) },
            { "FactionVictoryData", (parser, context) => context.FactionVictoryDatas.Add(FactionVictoryData.Parse(parser)) },
            { "FXList", (parser, context) => context.FXLists.Add(FXList.Parse(parser)) },
            { "GameData", (parser, context) => context.GameData = GameData.Parse(parser) },
            { "HeaderTemplate", (parser, context) => context.HeaderTemplates.Add(HeaderTemplate.Parse(parser)) },
            { "InGameUI", (parser, context) => context.InGameUI = InGameUI.Parse(parser) },
            { "Language", (parser, context) => context.Language = Language.Parse(parser) },
            { "LivingWorldCampaign", (parser, context) => context.LivingWorldCampaigns.Add(LivingWorldCampaign.Parse(parser)) },
            { "LivingWorldPlayerArmy", (parser, context) => context.LivingWorldPlayerArmies.Add(LivingWorldPlayerArmy.Parse(parser)) },
            { "LivingWorldRegionCampaign", (parser, context) => context.LivingWorldRegionCampaigns.Add(LivingWorldRegionCampaign.Parse(parser)) },
            { "LoadSubsystem", (parser, context) => context.Subsystems.Add(LoadSubsystem.Parse(parser)) },
            { "Locomotor", (parser, context) => context.Locomotors.Add(Locomotor.Parse(parser)) },
            { "LODPreset", (parser, context) => context.LodPresets.Add(LodPreset.Parse(parser)) },
            { "MapCache", (parser, context) => context.MapCaches.Add(MapCache.Parse(parser)) },
            { "MappedImage", (parser, context) => context.MappedImages.Add(MappedImage.Parse(parser)) },
            { "MiscAudio", (parser, context) => context.MiscAudio = MiscAudio.Parse(parser) },
            { "ModifierList", (parser, context) => context.ModifierLists.Add(ModifierList.Parse(parser)) },
            { "Mouse", (parser, context) => context.MouseData = MouseData.Parse(parser) },
            { "MouseCursor", (parser, context) => context.MouseCursors.Add(MouseCursor.Parse(parser)) },
            { "MultiplayerColor", (parser, context) => context.MultiplayerColors.Add(MultiplayerColor.Parse(parser)) },
            { "MultiplayerSettings", (parser, context) => context.MultiplayerSettings = MultiplayerSettings.Parse(parser) },
            { "MultiplayerStartingMoneyChoice", (parser, context) => context.MultiplayerStartingMoneyChoices.Add(MultiplayerStartingMoneyChoice.Parse(parser)) },
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
            { "Road", (parser, context) => context.Roads.Add(Road.Parse(parser)) },
            { "ReallyLowMHz", (parser, context) => context.ReallyLowMHz = ReallyLowMHz.Parse(parser) },
            { "Science", (parser, context) => context.Sciences.Add(Science.Parse(parser)) },
            { "ShellMenuScheme", (parser, context) => context.ShellMenuSchemes.Add(ShellMenuScheme.Parse(parser)) },
            { "SpecialPower", (parser, context) => context.SpecialPowers.Add(SpecialPower.Parse(parser)) },
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

        private static readonly char[] Separators = { ' ', '\n', '\r', '\t', '=' };
        private static readonly char[] SeparatorsPercent = { ' ', '\n', '\r', '\t', '=', '%' };
        public static readonly char[] SeparatorsColon = { ' ', '\n', '\r', '\t', '=', ':' };
        private static readonly char[] SeparatorsQuote = { '"', '\n', '=' };

        public const string EndToken = "END";

        private readonly TokenReader _tokenReader;

        private readonly string _directory;
        private readonly IniDataContext _dataContext;

        private readonly Stack<string> _currentBlockOrFieldStack;

        // Used for some things that need temporary storage, like AliasConditionState.
        public object Temp { get; set; }

        public IniTokenPosition CurrentPosition => _tokenReader.CurrentPosition;

        public IniParser(string source, FileSystemEntry entry, IniDataContext dataContext)
        {
            _directory = Path.GetDirectoryName(entry.FilePath);
            _dataContext = dataContext;

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

        public string ScanAssetReference(IniToken token) => token.Text;

        public string ParseAssetReference() => ScanAssetReference(GetNextToken());

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

        public int ScanInteger(IniToken token) => Convert.ToInt32(token.Text);

        public int ParseInteger() => ScanInteger(GetNextToken());

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
                if (!char.IsDigit(c) && c != '.' && c != '-')
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
            return ParseUtility.ParseFloat(GetFloatText(token));
        }

        public float ParseFloat() => ScanFloat(GetNextToken());

        private float ScanPercentage(IniToken token) => ScanFloat(token);

        public float ParsePercentage() => ScanPercentage(GetNextToken(SeparatorsPercent));

        private bool ScanBoolean(IniToken token)
        {
            switch (token.Text.ToUpper())
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

            return new ColorRgba
            {
                R = r,
                G = g,
                B = b,
                A = a
            };
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

            return result.Value;
        }

        public IniToken? GetNextTokenOptional(char[] separators = null)
        {
            return _tokenReader.NextToken(separators ?? Separators);
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

        private void ParseBlockContent<T>(
            T result,
           IIniFieldParserProvider<T> fieldParserProvider)
            where T : class, new()
        {
            var done = false;

            while (!done)
            {
                if (_tokenReader.EndOfFile)
                {
                    throw new InvalidOperationException();
                }

                _tokenReader.GoToNextLine();

                var token = _tokenReader.NextToken(Separators);

                if (token == null)
                {
                    continue;
                }

                if (string.Equals(token.Value.Text, EndToken, StringComparison.InvariantCultureIgnoreCase))
                {
                    done = true;
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

                if (token.Value.Text == "#define")
                {
                    var macroName = ParseIdentifier();
                    var macroExpansion = _tokenReader.NextToken(Separators);

                    _dataContext.Defines.Add(macroName, macroExpansion.Value);
                }
                else if (token.Value.Text == "#include")
                {
                    var includeFileName = ParseQuotedString();
                    _dataContext.LoadIniFile(Path.Combine(_directory, includeFileName));
                }
                else if (BlockParsers.TryGetValue(token.Value.Text, out var blockParser))
                {
                    _currentBlockOrFieldStack.Push(token.Value.Text);
                    blockParser(this, _dataContext);
                    _currentBlockOrFieldStack.Pop();
                }
                else
                {
                    throw new IniParseException($"Unexpected block '{token.Value.Text}'.", token.Value.Position);
                }
            }
        }
    }
}
