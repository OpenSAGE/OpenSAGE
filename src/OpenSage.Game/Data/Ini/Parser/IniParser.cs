using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Logic.Object;

namespace OpenSage.Data.Ini.Parser
{
    internal sealed partial class IniParser
    {
        private static readonly Dictionary<string, Action<IniParser, IniDataContext>> BlockParsers = new Dictionary<string, Action<IniParser, IniDataContext>>
        {
            { "AIData", (parser, context) => context.AIData = AIData.Parse(parser) },
            { "Animation", (parser, context) => context.Animations.Add(Animation.Parse(parser)) },
            { "Armor", (parser, context) => context.Armors.Add(Armor.Parse(parser)) },
            { "AudioEvent", (parser, context) => context.AudioEvents.Add(AudioEvent.Parse(parser)) },
            { "AudioSettings", (parser, context) => context.AudioSettings = AudioSettings.Parse(parser) },
            { "BenchProfile", (parser, context) => context.BenchProfiles.Add(BenchProfile.Parse(parser)) },
            { "Bridge", (parser, context) => context.Bridges.Add(Bridge.Parse(parser)) },
            { "Campaign", (parser, context) => context.Campaigns.Add(Campaign.Parse(parser)) },
            { "ChallengeGenerals", (parser, context) => context.ChallengeGenerals = ChallengeGenerals.Parse(parser) },
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
            { "FXList", (parser, context) => context.FXLists.Add(FXList.Parse(parser)) },
            { "GameData", (parser, context) => context.GameData = GameData.Parse(parser) },
            { "HeaderTemplate", (parser, context) => context.HeaderTemplates.Add(HeaderTemplate.Parse(parser)) },
            { "InGameUI", (parser, context) => context.InGameUI = InGameUI.Parse(parser) },
            { "Language", (parser, context) => context.Language = Language.Parse(parser) },
            { "Locomotor", (parser, context) => context.Locomotors.Add(Locomotor.Parse(parser)) },
            { "LODPreset", (parser, context) => context.LodPresets.Add(LodPreset.Parse(parser)) },
            { "MapCache", (parser, context) => context.MapCaches.Add(MapCache.Parse(parser)) },
            { "MappedImage", (parser, context) => context.MappedImages.Add(MappedImage.Parse(parser)) },
            { "MiscAudio", (parser, context) => context.MiscAudio = MiscAudio.Parse(parser) },
            { "Mouse", (parser, context) => context.MouseData = MouseData.Parse(parser) },
            { "MouseCursor", (parser, context) => context.MouseCursors.Add(MouseCursor.Parse(parser)) },
            { "MultiplayerColor", (parser, context) => context.MultiplayerColors.Add(MultiplayerColor.Parse(parser)) },
            { "MultiplayerSettings", (parser, context) => context.MultiplayerSettings = MultiplayerSettings.Parse(parser) },
            { "MultiplayerStartingMoneyChoice", (parser, context) => context.MultiplayerStartingMoneyChoices.Add(MultiplayerStartingMoneyChoice.Parse(parser)) },
            { "MusicTrack", (parser, context) => context.MusicTracks.Add(MusicTrack.Parse(parser)) },
            { "Object", (parser, context) => context.Objects.Add(ObjectDefinition.Parse(parser)) },
            { "ObjectReskin", (parser, context) => context.Objects.Add(ObjectDefinition.ParseReskin(parser)) },
            { "ObjectCreationList", (parser, context) => context.ObjectCreationLists.Add(ObjectCreationList.Parse(parser)) },
            { "OnlineChatColors", (parser, context) => context.OnlineChatColors = OnlineChatColors.Parse(parser) },
            { "ParticleSystem", (parser, context) => context.ParticleSystems.Add(ParticleSystem.Parse(parser)) },
            { "PlayerTemplate", (parser, context) => context.PlayerTemplates.Add(PlayerTemplate.Parse(parser)) },
            { "Rank", (parser, context) => context.Ranks.Add(Rank.Parse(parser)) },
            { "Road", (parser, context) => context.Roads.Add(Road.Parse(parser)) },
            { "ReallyLowMHz", (parser, context) => context.ReallyLowMHz = ReallyLowMHz.Parse(parser) },
            { "Science", (parser, context) => context.Sciences.Add(Science.Parse(parser)) },
            { "ShellMenuScheme", (parser, context) => context.ShellMenuSchemes.Add(ShellMenuScheme.Parse(parser)) },
            { "SpecialPower", (parser, context) => context.SpecialPowers.Add(SpecialPower.Parse(parser)) },
            { "StaticGameLOD", (parser, context) => context.StaticGameLods.Add(StaticGameLod.Parse(parser)) },
            { "Terrain", (parser, context) => context.TerrainTextures.Add(TerrainTexture.Parse(parser)) },
            { "Upgrade", (parser, context) => context.Upgrades.Add(Upgrade.Parse(parser)) },
            { "Video", (parser, context) => context.Videos.Add(Video.Parse(parser)) },
            { "WaterSet", (parser, context) => context.WaterSets.Add(WaterSet.Parse(parser)) },
            { "WaterTransparency", (parser, context) => context.WaterTransparency = WaterTransparency.Parse(parser) },
            { "Weapon", (parser, context) => context.Weapons.Add(Weapon.Parse(parser)) },
            { "Weather", (parser, context) => context.Weather = Weather.Parse(parser) },
            { "WebpageURL", (parser, context) => context.WebpageUrls.Add(WebpageUrl.Parse(parser)) },
            { "WindowTransition", (parser, context) => context.WindowTransitions.Add(WindowTransition.Parse(parser)) },
        };

        private readonly IniLexer _lexer;
        private readonly Stack<IniLexerMode> _lexerModeStack;
        private readonly List<IniToken> _tokens;
        private int _tokenIndex;

        private struct IniParserState
        {
            public IniLexerState LexerState;
            public int TokenIndex;
        }

        private readonly Stack<string> _currentBlockOrFieldStack;

        // Used for some things that need temporary storage, like AliasConditionState.
        public object Temp { get; set; }

        public IniParser(string source, string fileName)
        {
            _lexer = new IniLexer(source, fileName);

            _lexerModeStack = new Stack<IniLexerMode>();
            _lexerModeStack.Push(IniLexerMode.Normal);

            _tokens = new List<IniToken>();
            _tokenIndex = 0;

            _currentBlockOrFieldStack = new Stack<string>();
        }

        public IniToken Current => _tokens[EnsureToken(_tokenIndex)];

        public IniTokenPosition CurrentPosition => Current.Position;

        public IniTokenType CurrentTokenType
        {
            get
            {
                var state = GetState();

                try
                {
                    return Current.TokenType;
                }
                finally
                {
                    // Undo lexing, because we might want to re-lex in a different mode.
                    RestoreState(state);
                }
            }
        }

        private int EnsureToken(int tokenIndex)
        {
            if (_tokens.Count > 0 
                && _tokens[_tokens.Count - 1].TokenType == IniTokenType.EndOfFile
                && tokenIndex == _tokens.Count - 1)
            {
                return _tokens.Count - 1;
            }

            while (tokenIndex >= _tokens.Count)
            {
                var token = _lexer.Lex(_lexerModeStack.Peek());

                _tokens.Add(token);

                if (token.TokenType == IniTokenType.EndOfFile)
                {
                    break;
                }
            }

            return Math.Min(tokenIndex, _tokens.Count - 1);
        }

        private IniParserState GetState()
        {
            return new IniParserState
            {
                LexerState = _lexer.GetState(),
                TokenIndex = _tokenIndex
            };
        }

        private void RestoreState(IniParserState state)
        {
            _lexer.RestoreState(state.LexerState);
            _tokenIndex = state.TokenIndex;

            _tokens.RemoveRange(_tokenIndex, _tokens.Count - _tokenIndex);
        }

        public IniToken NextToken(params IniTokenType[] tokenTypes)
        {
            var token = Current;
            if (tokenTypes.Length > 0 && !tokenTypes.Contains(token.TokenType))
            {
                throw new IniParseException($"Expected token of type '{string.Join(",", tokenTypes)}', but got token of type '{token.TokenType}'.", token.Position);
            }
            _tokenIndex++;
            return token;
        }

        public IniToken? NextTokenIf(IniTokenType tokenType)
        {
            var state = GetState();

            var token = Current;
            if (token.TokenType == tokenType)
            {
                _tokenIndex++;
                return token;
            }
            // Undo lexing, because we might want to re-lex in a different mode.
            RestoreState(state);
            return null;
        }

        private IniToken NextIdentifierToken(string expectedStringValue)
        {
            var token = NextToken(IniTokenType.Identifier);
            if (token.StringValue != expectedStringValue)
            {
                throw new IniParseException($"Expected an identifier with name '{expectedStringValue}', but got '{token.StringValue}'.", token.Position);
            }
            return token;
        }

        private void UnexpectedToken(IniToken token)
        {
            throw new IniParseException($"Unexpected token: {token}", token.Position);
        }

        public string ParseString(bool allowWhitespace = false)
        {
            var lexerMode = allowWhitespace
                ? IniLexerMode.StringWithWhitespace
                : IniLexerMode.String;

            _lexerModeStack.Push(lexerMode);

            try
            {
                return NextToken(IniTokenType.StringLiteral).StringValue;
            }
            finally
            {
                _lexerModeStack.Pop();
            }
        }

        public string ParseAssetReference()
        {
            _lexerModeStack.Push(IniLexerMode.AssetReference);

            try
            {
                return NextToken(IniTokenType.StringLiteral).StringValue;
            }
            finally
            {
                _lexerModeStack.Pop();
            }
        }

        public string[] ParseAssetReferenceArray()
        {
            _lexerModeStack.Push(IniLexerMode.AssetReference);

            try
            {
                var result = new List<string>();

                while (Current.TokenType == IniTokenType.StringLiteral)
                {
                    result.Add(ParseAssetReference());
                }

                return result.ToArray();
            }
            finally
            {
                _lexerModeStack.Pop();
            }
        }

        public int ParseInteger() => NextToken(IniTokenType.IntegerLiteral).IntegerValue;

        public long ParseLong()
        {
            var token = NextToken(IniTokenType.LongLiteral, IniTokenType.IntegerLiteral);
            return (token.TokenType == IniTokenType.LongLiteral)
                ? token.LongValue
                : token.IntegerValue;
        }

        public byte ParseByte() => (byte) ParseInteger();

        public float ParseFloat()
        {
            var token = NextToken(IniTokenType.FloatLiteral, IniTokenType.IntegerLiteral);

            // ODDITY: Zero Hour ObjectCreationList.ini:179, there's an extra "End":
            // DispositionIntensity = 0.1 End
            var state = GetState();
            var ateNextToken = false;
            try
            {
                if (Current.TokenType == IniTokenType.Identifier && Current.StringValue == "End")
                {
                    NextToken();
                    ateNextToken = true;
                }
            }
            finally
            {
                if (!ateNextToken)
                {
                    RestoreState(state);
                }
            }

            return (token.TokenType == IniTokenType.FloatLiteral)
                ? token.FloatValue
                : token.IntegerValue;
        }

        public float ParsePercentage() => NextToken(IniTokenType.PercentLiteral).FloatValue;

        public bool ParseBoolean()
        {
            var token = NextToken(IniTokenType.Identifier);

            switch (token.StringValue.ToUpper())
            {
                case "YES":
                    return true;

                case "NO":
                    return false;

                default:
                    throw new IniParseException($"Invalid value for boolean: '{token.StringValue}'", token.Position);
            }
        }

        public string ParseIdentifier()
        {
            return NextToken(IniTokenType.Identifier).StringValue;
        }

        public string ParseLocalizedStringKey()
        {
            var result = ParseString();

            // ODDITY: ZH NatureProp.ini:37 incorrectly has OPTIMIZED_TREE after the localized string key.
            if (CurrentTokenType == IniTokenType.Identifier)
            {
                NextToken();
            }

            return result;
        }

        public string ParseFileName() => ParseString();
        public string ParseBoneName() => ParseString();
        public string[] ParseBoneNameArray() => ParseAssetReferenceArray();
        public string ParseAnimationName() => ParseString();

        private T ParseEnum<T>(Dictionary<string, T> stringToValueMap)
            where T : struct
        {
            var token = NextToken(IniTokenType.Identifier);

            if (stringToValueMap.TryGetValue(token.StringValue.ToUpper(), out var enumValue))
                return enumValue;

            throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{token.StringValue}'", token.Position);
        }

        private T ParseEnumFlags<T>(T noneValue, Dictionary<string, T> stringToValueMap)
            where T : struct
        {
            var result = noneValue;

            do
            {
                var token = NextToken(IniTokenType.Identifier);

                if (!stringToValueMap.TryGetValue(token.StringValue.ToUpper(), out var enumValue))
                    throw new IniParseException($"Invalid value for type '{typeof(T).Name}': '{token.StringValue}'", token.Position);

                // Ugh.
                result = (T) (object) ((int) (object) result | (int) (object) enumValue);
            } while (Current.TokenType != IniTokenType.EndOfLine);

            return result;
        }

        public T ParseTopLevelNamedBlock<T>(
            Action<T, string> setNameCallback,
            IIniFieldParserProvider<T> fieldParserProvider)
            where T : class, new()
        {
            NextToken();

            var result = ParseNamedBlock(setNameCallback, fieldParserProvider);

            NextTokenIf(IniTokenType.EndOfLine);

            return result;
        }

        public T ParseTopLevelNamedBlock<T>(
            Action<T, int> setNameCallback,
            IIniFieldParserProvider<T> fieldParserProvider)
            where T : class, new()
        {
            NextToken();

            var result = ParseNamedBlock(setNameCallback, fieldParserProvider);

            NextTokenIf(IniTokenType.EndOfLine);

            return result;
        }

        public T ParseNamedBlock<T>(
            Action<T, string> setNameCallback,
            IIniFieldParserProvider<T> fieldParserProvider)
            where T : class, new()
        {
            var result = new T();

            NextTokenIf(IniTokenType.Equals);

            // In only a few places in SCShellUserInterface512.INI,
            // there are what look like accidental spaces in the MappedImage names.
            var name = ParseString(allowWhitespace: true).Replace(" ", string.Empty);
            setNameCallback(result, name);

            var errantEnd = NextTokenIf(IniTokenType.Identifier);
            if (errantEnd != null && errantEnd.Value.StringValue != "End")
            {
                throw new IniParseException($"Unexpected identifier after block name: {errantEnd.Value.StringValue}", errantEnd.Value.Position);
            }

            NextToken(IniTokenType.EndOfLine);

            ParseBlockContent(result, fieldParserProvider);

            return result;
        }

        public T ParseNamedBlock<T>(
            Action<T, int> setNameCallback,
            IIniFieldParserProvider<T> fieldParserProvider)
            where T : class, new()
        {
            var result = new T();

            NextTokenIf(IniTokenType.Equals);

            var name = ParseInteger();
            setNameCallback(result, name);

            NextToken(IniTokenType.EndOfLine);

            ParseBlockContent(result, fieldParserProvider);

            return result;
        }

        public T ParseTopLevelBlock<T>(
            IIniFieldParserProvider<T> fieldParserProvider)
            where T : class, new()
        {
            NextToken();

            var result = ParseBlock(fieldParserProvider);

            NextTokenIf(IniTokenType.EndOfLine);

            return result;
        }

        public T ParseBlock<T>(
           IIniFieldParserProvider<T> fieldParserProvider)
           where T : class, new()
        {
            var result = new T();

            NextToken(IniTokenType.EndOfLine);

            ParseBlockContent(result, fieldParserProvider);

            return result;
        }

        private void ParseBlockContent<T>(
            T result,
           IIniFieldParserProvider<T> fieldParserProvider)
            where T : class, new()
        {
            while (Current.TokenType == IniTokenType.Identifier || Current.TokenType == IniTokenType.IntegerLiteral)
            {
                if (Current.TokenType == IniTokenType.Identifier && Current.StringValue.ToUpper() == "END")
                {
                    NextToken();

                    // ODDITY: ZH AmericaVehicle.ini:1993 has Upgrade_AmericaHellfireDrone after "End"
                    NextTokenIf(IniTokenType.Identifier);

                    break;
                }
                else
                {
                    var fieldName = Current.TokenType == IniTokenType.Identifier
                        ? Current.StringValue
                        : Current.IntegerValue.ToString();

                    if (fieldParserProvider.TryGetFieldParser(fieldName, out var fieldParser))
                    {
                        _currentBlockOrFieldStack.Push(fieldName);

                        NextToken();
                        NextTokenIf(IniTokenType.Equals);

                        // ODDITY: FactionBuilding.ini:13383 has a redundant =
                        NextTokenIf(IniTokenType.Equals);

                        fieldParser(this, result);

                        NextToken(IniTokenType.EndOfLine);

                        _currentBlockOrFieldStack.Pop();
                    }
                    else
                    {
                        throw new IniParseException($"Unexpected field '{fieldName}' in block '{_currentBlockOrFieldStack.Peek()}'.", Current.Position);
                    }
                }
            }
        }

        public void ParseFile(IniDataContext dataContext)
        {
            while (Current.TokenType != IniTokenType.EndOfFile)
            {
                switch (Current.TokenType)
                {
                    case IniTokenType.Identifier:
                        if (BlockParsers.TryGetValue(Current.StringValue, out var blockParser))
                        {
                            _currentBlockOrFieldStack.Push(Current.StringValue);
                            blockParser(this, dataContext);
                            _currentBlockOrFieldStack.Pop();
                        }
                        else
                        {
                            throw new IniParseException($"Unexpected block '{Current.StringValue}'.", Current.Position);
                        }
                        break;

                    default:
                        UnexpectedToken(Current);
                        break;
                }
            }
        }
    }
}
