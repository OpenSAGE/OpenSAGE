using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using OpenSage.Content;
using OpenSage.FileFormats;
using OpenSage.Graphics;
using OpenSage.Gui.Wnd.Transitions;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Terrain;
using Veldrid;

namespace OpenSage.Data.Ini.Parser
{
    internal sealed partial class IniParser
    {
        private static readonly Dictionary<string, Action<IniParser, IniDataContext>> BlockParsers = new Dictionary<string, Action<IniParser, IniDataContext>>
        {
            { "AIBase", (parser, context) => context.AIBases.Add(AIBase.Parse(parser)) },
            { "AIData", (parser, context) => context.AIData = AIData.Parse(parser) },
            { "AIDozerAssignment", (parser, context) => context.AIDozerAssignments.Add(AIDozerAssignment.Parse(parser)) },
            { "AmbientStream", (parser, context) => context.AmbientStreams.Add(AmbientStream.Parse(parser)) },
            { "Animation", (parser, context) => context.Animations.Add(Animation.Parse(parser)) },
            { "AnimationSoundClientBehaviorGlobalSetting", (parser, context) => context.AnimationSoundClientBehaviorGlobalSetting = AnimationSoundClientBehaviorGlobalSetting.Parse(parser) },
            { "AptButtonTooltipMap", (parser, context) => context.AptButtonTooltipMap = AptButtonTooltipMap.Parse(parser) },
            { "Armor", (parser, context) => context.Armors.Add(Armor.Parse(parser)) },
            { "ArmyDefinition", (parser, context) => context.ArmyDefinitions.Add(ArmyDefinition.Parse(parser)) },
            { "ArmySummaryDescription", (parser, x) => x.ArmySummaryDescription = ArmySummaryDescription.Parse(parser) },
            { "AudioEvent", (parser, context) =>
                {
                    var ev = AudioEvent.Parse(parser);
                    context.AudioEvents[ev.Name] = ev;
                }
            },
            { "AudioLOD", (parser, context) => context.AudioLods.Add(AudioLod.Parse(parser)) },
            { "AudioLowMHz", (parser, context) => context.AudioLowMHz = parser.ParseInteger() },
            { "AudioSettings", (parser, context) => context.AudioSettings = AudioSettings.Parse(parser) },
            { "AutoResolveArmor", (parser, context) => context.AutoResolveArmors.Add(AutoResolveArmor.Parse(parser)) },
            { "AutoResolveBody", (parser, context) => context.AutoResolveBodies.Add(AutoResolveBody.Parse(parser)) },
            { "AutoResolveCombatChain", (parser, context) => context.AutoResolveCombatChains.Add(AutoResolveCombatChain.Parse(parser)) },
            { "AutoResolveHandicapLevel", (parser, context) => context.AutoResolveHandicapLevels.Add(AutoResolveHandicapLevel.Parse(parser)) },
            { "AutoResolveReinforcementSchedule", (parser, context) => context.AutoResolveReinforcementSchedules.Add(AutoResolveReinforcementSchedule.Parse(parser)) },
            { "AutoResolveLeadership", (parser, context) => context.AutoResolveLeaderships.Add(AutoResolveLeadership.Parse(parser)) },
            { "AutoResolveWeapon", (parser, context) => context.AutoResolveWeapons.Add(AutoResolveWeapon.Parse(parser)) },
            { "AwardSystem", (parser, context) => context.AwardSystem = AwardSystem.Parse(parser) },
            { "BannerType", (parser, context) => context.BannerTypes.Add(BannerType.Parse(parser)) },
            { "BannerUI", (parser, context) => context.BannerUI = BannerUI.Parse(parser) },
            { "BenchProfile", (parser, context) => context.BenchProfiles.Add(BenchProfile.Parse(parser)) },
            { "Bridge", (parser, context) => parser.AssetStore.BridgeTemplates.Add(BridgeTemplate.Parse(parser)) },
            { "Campaign", (parser, context) => context.Campaigns.Add(Campaign.Parse(parser)) },
            { "ChallengeGenerals", (parser, context) => context.ChallengeGenerals = ChallengeGenerals.Parse(parser) },
            { "ChildObject", (parser, context) => parser.AssetStore.ObjectDefinitions.Add(ChildObject.Parse(parser)) },
            { "CloudEffect", (parser, context) => context.Environment.CloudEffect = CloudEffect.Parse(parser) },
            { "CommandButton", (parser, context) => context.CommandButtons.Add(CommandButton.Parse(parser)) },
            { "CommandMap", (parser, context) => context.CommandMaps.Add(CommandMap.Parse(parser)) },
            { "CommandSet", (parser, context) => context.CommandSets.Add(CommandSet.Parse(parser)) },
            { "ControlBarResizer", (parser, context) => context.ControlBarResizers.Add(ControlBarResizer.Parse(parser)) },
            { "ControlBarScheme", (parser, context) => context.ControlBarSchemes.Add(ControlBarScheme.Parse(parser)) },
            { "CrateData", (parser, context) => context.CrateDatas.Add(CrateData.Parse(parser)) },
            { "CreateAHeroSystem", (parser, context) => context.CreateAHeroSystem = CreateAHeroSystem.Parse(parser) },
            { "Credits", (parser, context) => context.Credits = Credits.Parse(parser) },
            { "CrowdResponse", (parser, context) => context.CrowdResponses.Add(CrowdResponse.Parse(parser)) },
            { "DamageFX", (parser, context) => context.DamageFXs.Add(DamageFX.Parse(parser)) },
            { "DialogEvent", (parser, context) => context.DialogEvents.Add(DialogEvent.Parse(parser)) },
            { "DrawGroupInfo", (parser, context) => context.DrawGroupInfo = DrawGroupInfo.Parse(parser) },
            { "DynamicGameLOD", (parser, context) => context.DynamicGameLods.Add(DynamicGameLod.Parse(parser)) },
            { "EmotionNugget", (parser, context) => context.EmotionNuggets.Add(EmotionNugget.Parse(parser)) },
            { "EvaEvent", (parser, context) =>
                {
                    var evaEvent = EvaEvent.Parse(parser);
                    context.EvaEvents[evaEvent.Name] = evaEvent;
                }
            },
            { "EvaEventForwardReference", (parser, context) =>
                {
                    var name = parser.ParseString();
                    context.EvaEvents[name] = new EvaEvent { Name = name };
                }
            },
            { "ExperienceLevel", (parser, context) => context.ExperienceLevels.Add(ExperienceLevel.Parse(parser)) },
            { "ExperienceScalarTable", (parser, context) => context.ExperienceScalarTables.Add(ExperienceScalarTable.Parse(parser)) },
            { "FactionVictoryData", (parser, context) => context.FactionVictoryDatas.Add(FactionVictoryData.Parse(parser)) },
            { "Fire", (parser, context) => context.Fire = Fire.Parse(parser) },
            { "FireLogicSystem", (parser, context) => context.FireLogicSystem = FireLogicSystem.Parse(parser) },
            { "FireEffect", (parser, context) => context.Environment.FireEffect = RingEffect.Parse(parser) },
            { "FontDefaultSettings", (parser, context) => context.FontDefaultSettings.Add(FontDefaultSetting.Parse(parser)) },
            { "FontSubstitution", (parser, context) => context.FontSubstitutions.Add(FontSubstitution.Parse(parser)) },
            { "FormationAssistant", (parser, context) => context.FormationAssistant = FormationAssistant.Parse(parser) },
            { "FXList", (parser, context) => context.FXLists.Add(FXList.Parse(parser)) },
            { "FXParticleSystem", (parser, context) => parser.AssetStore.FXParticleSystems.Add(FXParticleSystemTemplate.Parse(parser)) },
            { "GameData", (parser, context) => context.GameData = GameData.Parse(parser) },
            { "GlowEffect", (parser, context) => context.Environment.GlowEffect = GlowEffect.Parse(parser) },
            { "HeaderTemplate", (parser, context) => context.HeaderTemplates.Add(HeaderTemplate.Parse(parser)) },
            { "HouseColor", (parser, context) => context.HouseColors.Add(HouseColor.Parse(parser)) },
            { "InGameNotificationBox", (parser, context) => context.InGameNotificationBox = InGameNotificationBox.Parse(parser) },
            { "InGameUI", (parser, context) => context.InGameUI = InGameUI.Parse(parser) },
            { "Language", (parser, context) => context.Language = Language.Parse(parser) },
            { "LargeGroupAudioMap", (parser, context) => context.LargeGroupAudioMaps.Add(LargeGroupAudioMap.Parse(parser)) },
            { "LinearCampaign", (parser, context) => context.LinearCampaign = LinearCampaign.Parse(parser) },
            { "LivingWorldAITemplate", (parser, context) => context.LivingWorldAiTemplate = LivingWorldAiTemplate.Parse(parser) },
            { "LivingWorldAnimObject", (parser, context) => context.LivingWorldAnimObjects.Add(LivingWorldAnimObject.Parse(parser)) },
            { "LivingWorldArmyIcon", (parser, context) => context.LivingWorldArmyIcons.Add(LivingWorldArmyIcon.Parse(parser)) },
            { "LivingWorldAutoResolveResourceBonus", (parser, context) => context.LivingWorldAutoResolveResourceBonus = LivingWorldAutoResolveResourceBonus.Parse(parser) },
            { "LivingWorldAutoResolveSciencePurchasePointBonus", (parser, context) => context.LivingWorldAutoResolveSciencePurchasePointBonus = LivingWorldAutoResolveSciencePurchasePointBonus.Parse(parser) },
            { "LivingWorldBuilding", (parser, context) => context.LivingWorldBuildings.Add(LivingWorldBuilding.Parse(parser)) },
            { "LivingWorldBuildPlotIcon", (parser, context) => context.LivingWorldBuildPlotIcons.Add(LivingWorldBuildPlotIcon.Parse(parser)) },
            { "LivingWorldBuildingIcon", (parser, context) => context.LivingWorldBuildingIcons.Add(LivingWorldBuildingIcon.Parse(parser)) },
            { "LargeGroupAudioUnusedKnownKeys", (parser, context) => context.LargeGroupAudioUnusedKnownKeys = LargeGroupAudioUnusedKnownKeys.Parse(parser) },
            { "LivingWorldCampaign", (parser, context) => context.LivingWorldCampaigns.Add(LivingWorldCampaign.Parse(parser)) },
            { "LivingWorldMapInfo", (parser, context) => context.LivingWorldMapInfo = LivingWorldMapInfo.Parse(parser) },
            { "LivingWorldObject", (parser, context) => context.LivingWorldObjects.Add(LivingWorldObject.Parse(parser)) },
            { "LivingWorldPlayerArmy", (parser, context) => context.LivingWorldPlayerArmies.Add(LivingWorldPlayerArmy.Parse(parser)) },
            { "LivingWorldPlayerTemplate", (parser, context) => context.LivingWorldPlayerTemplates.Add(LivingWorldPlayerTemplate.Parse(parser)) },
            { "LivingWorldRegionCampaign", (parser, context) => context.LivingWorldRegionCampaigns.Add(LivingWorldRegionCampaign.Parse(parser)) },
            { "LivingWorldRegionEffects", (parser, x) => x.LivingWorldRegionEffects = LivingWorldRegionEffects.Parse(parser) },
            { "LivingWorldSound", (parser, context) => context.LivingWorldSounds.Add(LivingWorldSound.Parse(parser)) },
            { "LoadSubsystem", (parser, context) => context.Subsystems.Add(LoadSubsystem.Parse(parser)) },
            { "Locomotor", (parser, context) => parser.AssetStore.Locomotors.Add(Locomotor.Parse(parser)) },
            { "LODPreset", (parser, context) => context.LodPresets.Add(LodPreset.Parse(parser)) },
            { "MapCache", (parser, context) => context.MapCaches.Add(MapCache.Parse(parser)) },
            { "MappedImage", (parser, context) => parser.AssetStore.MappedImages.Add(MappedImage.Parse(parser)) },
            { "MeshNameMatches", (parser, context) => context.MeshNameMatches.Add(MeshNameMatches.Parse(parser)) },
            { "MiscAudio", (parser, context) => context.MiscAudio = MiscAudio.Parse(parser) },
            { "MiscEvaData", (parser, context) => context.MiscEvaData = MiscEvaData.Parse(parser) },
            { "MissionObjectiveList", (parser, context) => context.MissionObjectiveList = MissionObjectiveList.Parse(parser) },
            { "ModifierList", (parser, context) => context.ModifierLists.Add(ModifierList.Parse(parser)) },
            { "Mouse", (parser, context) => context.MouseData = MouseData.Parse(parser) },
            { "MouseCursor", (parser, context) => context.MouseCursors.Add(MouseCursor.Parse(parser)) },
            { "MultiplayerColor", (parser, context) => context.MultiplayerColors.Add(MultiplayerColor.Parse(parser)) },
            { "MultiplayerSettings", (parser, context) => context.MultiplayerSettings = MultiplayerSettings.Parse(parser) },
            { "MultiplayerStartingMoneyChoice", (parser, context) => context.MultiplayerStartingMoneyChoices.Add(MultiplayerStartingMoneyChoice.Parse(parser)) },
            { "Multisound", (parser, context) => context.Multisounds.Add(Multisound.Parse(parser)) },
            { "MusicTrack", (parser, context) => context.MusicTracks.Add(MusicTrack.Parse(parser)) },
            { "NewEvaEvent", (parser, context) =>
                {
                    var evaEvent = EvaEvent.Parse(parser);
                    context.EvaEvents[evaEvent.Name] = evaEvent;
                }
            },
            { "Object", (parser, context) => parser.AssetStore.ObjectDefinitions.Add(ObjectDefinition.Parse(parser)) },
            { "ObjectReskin", (parser, context) => parser.AssetStore.ObjectDefinitions.Add(ObjectDefinition.ParseReskin(parser)) },
            { "ObjectCreationList", (parser, context) => context.ObjectCreationLists.Add(ObjectCreationList.Parse(parser)) },
            { "OnlineChatColors", (parser, context) => context.OnlineChatColors = OnlineChatColors.Parse(parser) },
            { "ParticleSystem", (parser, context) => parser.AssetStore.ParticleSystems.Add(ParticleSystemDefinition.Parse(parser)) },
            { "Pathfinder", (parser, context) => context.Pathfinder = Pathfinder.Parse(parser) },
            { "PlayerAIType", (parser, context) => context.PlayerAITypes.Add(PlayerAIType.Parse(parser)) },
            { "PlayerTemplate", (parser, context) => parser.AssetStore.PlayerTemplates.Add(PlayerTemplate.Parse(parser)) },
            { "PredefinedEvaEvent", (parser, context) =>
                {
                    var evaEvent = EvaEvent.Parse(parser);
                    context.EvaEvents[evaEvent.Name] = evaEvent;
                }
            },
            { "Rank", (parser, context) => context.Ranks.Add(Rank.Parse(parser)) },
            { "RegionCampain", (parser, context) => context.RegionCampaign = RegionCampain.Parse(parser) },
            { "RingEffect", (parser, context) => context.Environment.RingEffect = RingEffect.Parse(parser) },
            { "Road", (parser, context) => parser.AssetStore.RoadTemplates.Add(RoadTemplate.Parse(parser)) },
            { "ReallyLowMHz", (parser, context) => context.ReallyLowMHz = parser.ParseInteger() },
            { "Science", (parser, context) => context.Sciences.Add(Science.Parse(parser)) },
            { "ScoredKillEvaAnnouncer", (parser, context) => context.ScoredKillEvaAnnouncers.Add(ScoredKillEvaAnnouncer.Parse(parser)) },
            { "ShadowMap", (parser, context) => context.Environment.ShadowMap = ShadowMap.Parse(parser) },
            { "ShellMenuScheme", (parser, context) => context.ShellMenuSchemes.Add(ShellMenuScheme.Parse(parser)) },
            { "SkirmishAIData", (parser, context) => context.SkirmishAIData = SkirmishAIData.Parse(parser) },
            { "SkyboxTextureSet", (parser, context) => context.SkyboxTextureSets.Add(SkyboxTextureSet.Parse(parser)) },
            { "SpecialPower", (parser, context) => context.SpecialPowers.Add(SpecialPower.Parse(parser)) },
            { "StanceTemplate", (parser, context) => context.StanceTemplates.Add(StanceTemplate.Parse(parser)) },
            { "StreamedSound", (parser, context) => context.StreamedSounds.Add(StreamedSound.Parse(parser)) },
            { "StaticGameLOD", (parser, context) => context.StaticGameLods.Add(StaticGameLod.Parse(parser)) },
            { "StrategicHUD", (parser, context) => context.StrategicHud = StrategicHud.Parse(parser) },
            { "Terrain", (parser, context) => parser.AssetStore.TerrainTextures.Add(TerrainTexture.Parse(parser)) },
            { "Upgrade", (parser, context) => context.Upgrades.Add(Upgrade.Parse(parser)) },
            { "VictorySystemData", (parser, context) => context.VictorySystemDatas.Add(VictorySystemData.Parse(parser)) },
            { "Video", (parser, context) => context.Videos.Add(Video.Parse(parser)) },
            { "WaterSet", (parser, context) => parser.AssetStore.WaterSets.Add(WaterSet.Parse(parser)) },
            { "WaterTextureList", (parser, context) => context.WaterTextureLists.Add(WaterTextureList.Parse(parser)) },
            { "WaterTransparency", (parser, context) => context.WaterTransparency = WaterTransparency.Parse(parser) },
            { "Weapon", (parser, context) => context.Weapons.Add(Weapon.Parse(parser)) },
            { "Weather", (parser, context) => context.Weather = Weather.Parse(parser) },
            { "WeatherData", (parser, context) => context.WeatherDatas.Add(WeatherData.Parse(parser)) },
            { "WebpageURL", (parser, context) => context.WebpageUrls.Add(WebpageUrl.Parse(parser)) },
            { "WindowTransition", (parser, context) => parser.AssetStore.WindowTransitions.Add(WindowTransition.Parse(parser)) },
        };

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

        // Used for some things that need temporary storage, like AliasConditionState.
        public object Temp { get; set; }

        public IniTokenPosition CurrentPosition => _tokenReader.CurrentPosition;

        public AssetStore AssetStore { get; }
        public SageGame SageGame { get; }

        public IniParser(FileSystemEntry entry, AssetStore assetStore, SageGame sageGame, IniDataContext dataContext)
        {
            _directory = Path.GetDirectoryName(entry.FilePath);
            _dataContext = dataContext;
            _fileSystem = entry.FileSystem;
            AssetStore = assetStore;
            SageGame = sageGame;

            _tokenReader = CreateTokenReader(entry);

            _currentBlockOrFieldStack = new Stack<string>();
        }

        private TokenReader CreateTokenReader(FileSystemEntry entry)
        {
            string source;

            if (entry != null)
            {
                using (var stream = entry.Open())
                using (var reader = new StreamReader(stream, Encoding.ASCII))
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

        public float ParsePercentage() => ScanPercentage(GetNextToken(SeparatorsPercent));

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

        public LazyAssetReference<Locomotor> ParseLocomotorReference()
        {
            var name = ParseAssetReference();
            return new LazyAssetReference<Locomotor>(() => AssetStore.Locomotors.GetByName(name));
        }

        public LazyAssetReference<Locomotor>[] ParseLocomotorReferenceArray()
        {
            var result = new List<LazyAssetReference<Locomotor>>();

            IniToken? token;
            while ((token = GetNextTokenOptional()).HasValue)
            {
                var localToken = token;
                result.Add(new LazyAssetReference<Locomotor>(() => AssetStore.Locomotors.GetByName(localToken.Value.Text)));
            }

            return result.ToArray();
        }

        public LazyAssetReference<ObjectDefinition> ParseObjectReference()
        {
            var name = ParseAssetReference();
            return new LazyAssetReference<ObjectDefinition>(() => AssetStore.ObjectDefinitions.GetByName(name));
        }

        public LazyAssetReference<Texture> ParseTextureReference()
        {
            var fileName = ParseFileName();
            return new LazyAssetReference<Texture>(() => AssetStore.Textures.GetByName(fileName));
        }

        public LazyAssetReference<Texture> ParseGuiTextureReference()
        {
            var fileName = ParseFileName();
            return new LazyAssetReference<Texture>(() => AssetStore.GuiTextures.GetByName(fileName));
        }

        public LazyAssetReference<Model> ParseModelReference()
        {
            var fileName = ParseFileName();
            return (!string.Equals(fileName, "NONE", StringComparison.OrdinalIgnoreCase))
                ? new LazyAssetReference<Model>(() => AssetStore.Models.GetByName(fileName))
                : null;
        }

        public LazyAssetReference<Graphics.Animation.Animation> ParseAnimationReference()
        {
            var animationName = ParseAnimationName();
            return (!string.Equals(animationName, "NONE", StringComparison.OrdinalIgnoreCase))
                ? new LazyAssetReference<Graphics.Animation.Animation>(() => AssetStore.Animations.GetByName(animationName))
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
            var tokenReader = CreateTokenReader(includeEntry);

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
                    var includeParser = new IniParser(includeEntry, AssetStore, SageGame, _dataContext);
                    includeParser.ParseFile();
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
