using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Eva;
using OpenSage.FileFormats;
using OpenSage.Graphics;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.InGame;
using OpenSage.Gui.Wnd.Transitions;
using OpenSage.Input;
using OpenSage.LivingWorld;
using OpenSage.LivingWorld.AutoResolve;
using OpenSage.Lod;
using OpenSage.Logic;
using OpenSage.Logic.AI;
using OpenSage.Logic.Object;
using OpenSage.Logic.Object.Damage;
using OpenSage.Logic.Pathfinding;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.Data.Ini
{
    internal sealed partial class IniParser
    {
        private static readonly Dictionary<string, Action<IniParser, IniDataContext>> BlockParsers = new Dictionary<string, Action<IniParser, IniDataContext>>
        {
            { "AIBase", (parser, context) => parser.AssetStore.AIBases.Add(AIBase.Parse(parser)) },
            { "AIData", (parser, context) => AIData.Parse(parser, parser.AssetStore.AIData.Current) },
            { "AIDozerAssignment", (parser, context) => parser.AssetStore.AIDozerAssignments.Add(AIDozerAssignment.Parse(parser)) },
            { "AmbientStream", (parser, context) => parser.AssetStore.AmbientStreams.Add(AmbientStream.Parse(parser)) },
            { "Animation", (parser, context) => parser.AssetStore.Animations.Add(Animation.Parse(parser)) },
            { "AnimationSoundClientBehaviorGlobalSetting", (parser, context) => AnimationSoundClientBehaviorGlobalSetting.Parse(parser, parser.AssetStore.AnimationSoundClientBehaviorGlobalSetting.Current) },
            { "AptButtonTooltipMap", (parser, context) => AptButtonTooltipMap.Parse(parser, parser.AssetStore.AptButtonTooltipMap.Current) },
            { "Armor", (parser, context) => parser.AssetStore.Armors.Add(Armor.Parse(parser)) },
            { "ArmyDefinition", (parser, context) => parser.AssetStore.ArmyDefinitions.Add(ArmyDefinition.Parse(parser)) },
            { "ArmySummaryDescription", (parser, context) => ArmySummaryDescription.Parse(parser, parser.AssetStore.ArmySummaryDescription.Current) },
            { "AudioEvent", (parser, context) => parser.AssetStore.AudioEvents.Add(AudioEvent.Parse(parser)) },
            { "AudioLOD", (parser, context) => parser.AssetStore.AudioLods.Add(AudioLod.Parse(parser)) },
            { "AudioLowMHz", (parser, context) => parser.ParseInteger() },
            { "AudioSettings", (parser, context) => AudioSettings.Parse(parser, parser.AssetStore.AudioSettings.Current) },
            { "AutoResolveArmor", (parser, context) => parser.AssetStore.AutoResolveArmors.Add(AutoResolveArmor.Parse(parser)) },
            { "AutoResolveBody", (parser, context) => parser.AssetStore.AutoResolveBodies.Add(AutoResolveBody.Parse(parser)) },
            { "AutoResolveCombatChain", (parser, context) => parser.AssetStore.AutoResolveCombatChains.Add(AutoResolveCombatChain.Parse(parser)) },
            { "AutoResolveHandicapLevel", (parser, context) => parser.AssetStore.AutoResolveHandicapLevels.Add(AutoResolveHandicapLevel.Parse(parser)) },
            { "AutoResolveReinforcementSchedule", (parser, context) => parser.AssetStore.AutoResolveReinforcementSchedules.Add(AutoResolveReinforcementSchedule.Parse(parser)) },
            { "AutoResolveLeadership", (parser, context) => parser.AssetStore.AutoResolveLeaderships.Add(AutoResolveLeadership.Parse(parser)) },
            { "AutoResolveWeapon", (parser, context) => parser.AssetStore.AutoResolveWeapons.Add(AutoResolveWeapon.Parse(parser)) },
            { "AwardSystem", (parser, context) => AwardSystem.Parse(parser, parser.AssetStore.AwardSystem.Current) },
            { "BannerType", (parser, context) => parser.AssetStore.BannerTypes.Add(BannerType.Parse(parser)) },
            { "BannerUI", (parser, context) => BannerUI.Parse(parser, parser.AssetStore.BannerUI.Current) },
            { "BenchProfile", (parser, context) => parser.AssetStore.BenchProfiles.Add(BenchProfile.Parse(parser)) },
            { "Bridge", (parser, context) => parser.AssetStore.BridgeTemplates.Add(BridgeTemplate.Parse(parser)) },
            { "Campaign", (parser, context) => parser.AssetStore.CampaignTemplates.Add(CampaignTemplate.Parse(parser)) },
            { "ChallengeGenerals", (parser, context) => ChallengeGenerals.Parse(parser, parser.AssetStore.ChallengeGenerals.Current) },
            { "ChildObject", (parser, context) => parser.AssetStore.ObjectDefinitions.Add(ChildObject.Parse(parser)) },
            { "CloudEffect", (parser, context) => parser.AssetStore.Environment.Current.CloudEffect = CloudEffect.Parse(parser) },
            { "CommandButton", (parser, context) => parser.AssetStore.CommandButtons.Add(CommandButton.Parse(parser)) },
            { "CommandMap", (parser, context) => parser.AssetStore.CommandMaps.Add(CommandMap.Parse(parser)) },
            { "CommandSet", (parser, context) => parser.AssetStore.CommandSets.Add(CommandSet.Parse(parser)) },
            { "ControlBarResizer", (parser, context) => parser.AssetStore.ControlBarResizers.Add(ControlBarResizer.Parse(parser)) },
            { "ControlBarScheme", (parser, context) => parser.AssetStore.ControlBarSchemes.Add(ControlBarScheme.Parse(parser)) },
            { "CrateData", (parser, context) => parser.AssetStore.CrateDatas.Add(CrateData.Parse(parser)) },
            { "CreateAHeroSystem", (parser, context) => CreateAHeroSystem.Parse(parser, parser.AssetStore.CreateAHeroSystem.Current) },
            { "Credits", (parser, context) => Credits.Parse(parser, parser.AssetStore.Credits.Current) },
            { "CrowdResponse", (parser, context) => parser.AssetStore.CrowdResponses.Add(CrowdResponse.Parse(parser)) },
            { "DamageFX", (parser, context) => parser.AssetStore.DamageFXs.Add(DamageFX.Parse(parser)) },
            { "DialogEvent", (parser, context) => parser.AssetStore.DialogEvents.Add(DialogEvent.Parse(parser)) },
            { "DrawGroupInfo", (parser, context) => DrawGroupInfo.Parse(parser, parser.AssetStore.DrawGroupInfo.Current) },
            { "DynamicGameLOD", (parser, context) => parser.AssetStore.DynamicGameLods.Add(DynamicGameLod.Parse(parser)) },
            { "EmotionNugget", (parser, context) => parser.AssetStore.EmotionNuggets.Add(EmotionNugget.Parse(parser)) },
            { "EvaEvent", (parser, context) => parser.AssetStore.EvaEvents.Add(EvaEvent.Parse(parser)) },
            { "EvaEventForwardReference", (parser, context) => EvaEvent.Parse(parser) },
            { "ExperienceLevel", (parser, context) => parser.AssetStore.ExperienceLevels.Add(ExperienceLevel.Parse(parser)) },
            { "ExperienceScalarTable", (parser, context) => parser.AssetStore.ExperienceScalarTables.Add(ExperienceScalarTable.Parse(parser)) },
            { "FactionVictoryData", (parser, context) => parser.AssetStore.FactionVictoryDatas.Add(FactionVictoryData.Parse(parser)) },
            { "Fire", (parser, context) => Fire.Parse(parser, parser.AssetStore.Fire.Current) },
            { "FireLogicSystem", (parser, context) => FireLogicSystem.Parse(parser, parser.AssetStore.FireLogicSystem.Current) },
            { "FireEffect", (parser, context) => parser.AssetStore.Environment.Current.FireEffect = RingEffect.Parse(parser) },
            { "FontDefaultSettings", (parser, context) => parser.AssetStore.FontDefaultSettings.Add(FontDefaultSetting.Parse(parser)) },
            { "FontSubstitution", (parser, context) => parser.AssetStore.FontSubstitutions.Add(FontSubstitution.Parse(parser)) },
            { "FormationAssistant", (parser, context) => FormationAssistant.Parse(parser, parser.AssetStore.FormationAssistant.Current) },
            { "FXList", (parser, context) => parser.AssetStore.FXLists.Add(FXList.Parse(parser)) },
            { "FXParticleSystem", (parser, context) => parser.AssetStore.FXParticleSystemTemplates.Add(FXParticleSystemTemplate.Parse(parser)) },
            { "GameData", (parser, context) => GameData.Parse(parser, parser.AssetStore.GameData.Current) },
            { "GlowEffect", (parser, context) => parser.AssetStore.Environment.Current.GlowEffect = GlowEffect.Parse(parser) },
            { "HeaderTemplate", (parser, context) => parser.AssetStore.HeaderTemplates.Add(HeaderTemplate.Parse(parser)) },
            { "HouseColor", (parser, context) => parser.AssetStore.HouseColors.Add(HouseColor.Parse(parser)) },
            { "InGameNotificationBox", (parser, context) => InGameNotificationBox.Parse(parser, parser.AssetStore.InGameNotificationBox.Current) },
            { "InGameUI", (parser, context) => InGameUI.Parse(parser, parser.AssetStore.InGameUI.Current) },
            { "Language", (parser, context) => Language.Parse(parser, parser.AssetStore.Language.Current) },
            { "LargeGroupAudioMap", (parser, context) => parser.AssetStore.LargeGroupAudioMaps.Add(LargeGroupAudioMap.Parse(parser)) },
            { "LinearCampaign", (parser, context) => LinearCampaign.Parse(parser, parser.AssetStore.LinearCampaign.Current) },
            { "LivingWorldAITemplate", (parser, context) => parser.AssetStore.LivingWorldAITemplates.Add(LivingWorldAITemplate.Parse(parser)) },
            { "LivingWorldAnimObject", (parser, context) => parser.AssetStore.LivingWorldAnimObjects.Add(LivingWorldAnimObject.Parse(parser)) },
            { "LivingWorldArmyIcon", (parser, context) => parser.AssetStore.LivingWorldArmyIcons.Add(LivingWorldArmyIcon.Parse(parser)) },
            { "LivingWorldAutoResolveResourceBonus", (parser, context) => LivingWorldAutoResolveResourceBonus.Parse(parser, parser.AssetStore.LivingWorldAutoResolveResourceBonus.Current) },
            { "LivingWorldAutoResolveSciencePurchasePointBonus", (parser, context) => LivingWorldAutoResolveSciencePurchasePointBonus.Parse(parser, parser.AssetStore.LivingWorldAutoResolveSciencePurchasePointBonus.Current) },
            { "LivingWorldBuilding", (parser, context) => parser.AssetStore.LivingWorldBuildings.Add(LivingWorldBuilding.Parse(parser)) },
            { "LivingWorldBuildPlotIcon", (parser, context) => parser.AssetStore.LivingWorldBuildPlotIcons.Add(LivingWorldBuildPlotIcon.Parse(parser)) },
            { "LivingWorldBuildingIcon", (parser, context) => parser.AssetStore.LivingWorldBuildingIcons.Add(LivingWorldBuildingIcon.Parse(parser)) },
            { "LargeGroupAudioUnusedKnownKeys", (parser, context) => LargeGroupAudioUnusedKnownKeys.Parse(parser, parser.AssetStore.LargeGroupAudioUnusedKnownKeys.Current) },
            { "LivingWorldCampaign", (parser, context) => parser.AssetStore.LivingWorldCampaigns.Add(LivingWorldCampaign.Parse(parser)) },
            { "LivingWorldMapInfo", (parser, context) => LivingWorldMapInfo.Parse(parser, parser.AssetStore.LivingWorldMapInfo.Current) },
            { "LivingWorldObject", (parser, context) => parser.AssetStore.LivingWorldObjects.Add(LivingWorldObject.Parse(parser)) },
            { "LivingWorldPlayerArmy", (parser, context) => parser.AssetStore.LivingWorldPlayerArmies.Add(LivingWorldPlayerArmy.Parse(parser)) },
            { "LivingWorldPlayerTemplate", (parser, context) => parser.AssetStore.LivingWorldPlayerTemplates.Add(LivingWorldPlayerTemplate.Parse(parser)) },
            { "LivingWorldRegionCampaign", (parser, context) => parser.AssetStore.LivingWorldRegionCampaigns.Add(LivingWorldRegionCampaign.Parse(parser)) },
            { "LivingWorldRegionEffects", (parser, context) => parser.AssetStore.LivingWorldRegionEffects.Add(LivingWorldRegionEffects.Parse(parser)) },
            { "LivingWorldSound", (parser, context) => parser.AssetStore.LivingWorldSounds.Add(LivingWorldSound.Parse(parser)) },
            { "LoadSubsystem", (parser, context) => parser.AssetStore.Subsystems.Add(LoadSubsystem.Parse(parser)) },
            { "Locomotor", (parser, context) => parser.AssetStore.Locomotors.Add(Locomotor.Parse(parser)) },
            { "LODPreset", (parser, context) => parser.AssetStore.LodPresets.Add(LodPreset.Parse(parser)) },
            { "MapCache", (parser, context) => parser.AssetStore.MapCaches.Add(MapCache.Parse(parser)) },
            { "MappedImage", (parser, context) => parser.AssetStore.MappedImages.Add(MappedImage.Parse(parser)) },
            { "MeshNameMatches", (parser, context) => parser.AssetStore.MeshNameMatches.Add(MeshNameMatches.Parse(parser)) },
            { "MiscAudio", (parser, context) => MiscAudio.Parse(parser, parser.AssetStore.MiscAudio.Current) },
            { "MiscEvaData", (parser, context) => MiscEvaData.Parse(parser, parser.AssetStore.MiscEvaData.Current) },
            { "MissionObjectiveList", (parser, context) => MissionObjectiveList.Parse(parser, parser.AssetStore.MissionObjectiveList.Current) },
            { "ModifierList", (parser, context) => parser.AssetStore.ModifierLists.Add(ModifierList.Parse(parser)) },
            { "Mouse", (parser, context) => MouseData.Parse(parser, parser.AssetStore.MouseData.Current) },
            { "MouseCursor", (parser, context) => parser.AssetStore.MouseCursors.Add(MouseCursor.Parse(parser)) },
            { "MultiplayerColor", (parser, context) => parser.AssetStore.MultiplayerColors.Add(MultiplayerColor.Parse(parser)) },
            { "MultiplayerSettings", (parser, context) => MultiplayerSettings.Parse(parser, parser.AssetStore.MultiplayerSettings.Current) },
            { "MultiplayerStartingMoneyChoice", (parser, context) => parser.AssetStore.MultiplayerStartingMoneyChoices.Add(MultiplayerStartingMoneyChoice.Parse(parser)) },
            { "Multisound", (parser, context) => parser.AssetStore.Multisounds.Add(Multisound.Parse(parser)) },
            { "MusicTrack", (parser, context) => parser.AssetStore.MusicTracks.Add(MusicTrack.Parse(parser)) },
            { "NewEvaEvent", (parser, context) => parser.AssetStore.EvaEvents.Add(EvaEvent.Parse(parser)) },
            { "Object", (parser, context) => parser.AssetStore.ObjectDefinitions.Add(ObjectDefinition.Parse(parser)) },
            { "ObjectReskin", (parser, context) => parser.AssetStore.ObjectDefinitions.Add(ObjectDefinition.ParseReskin(parser)) },
            { "ObjectCreationList", (parser, context) => parser.AssetStore.ObjectCreationLists.Add(ObjectCreationList.Parse(parser)) },
            { "OnlineChatColors", (parser, context) => OnlineChatColors.Parse(parser, parser.AssetStore.OnlineChatColors.Current) },
            { "ParticleSystem", (parser, context) => parser.AssetStore.ParticleSystemTemplates.Add(ParticleSystemTemplate.Parse(parser)) },
            { "Pathfinder", (parser, context) => Pathfinder.Parse(parser, parser.AssetStore.Pathfinder.Current) },
            { "PlayerAIType", (parser, context) => parser.AssetStore.PlayerAITypes.Add(PlayerAIType.Parse(parser)) },
            { "PlayerTemplate", (parser, context) => parser.AssetStore.PlayerTemplates.Add(PlayerTemplate.Parse(parser)) },
            { "PredefinedEvaEvent", (parser, context) => parser.AssetStore.EvaEvents.Add(EvaEvent.Parse(parser)) },
            { "Rank", (parser, context) => parser.AssetStore.Ranks.Add(Rank.Parse(parser)) },
            { "RegionCampain", (parser, context) => parser.AssetStore.RegionCampaigns.Add(RegionCampaign.Parse(parser)) },
            { "RingEffect", (parser, context) => parser.AssetStore.Environment.Current.RingEffect = RingEffect.Parse(parser) },
            { "Road", (parser, context) => parser.AssetStore.RoadTemplates.Add(RoadTemplate.Parse(parser)) },
            { "ReallyLowMHz", (parser, context) => parser.ParseInteger() },
            { "Science", (parser, context) => parser.AssetStore.Sciences.Add(Science.Parse(parser)) },
            { "ScoredKillEvaAnnouncer", (parser, context) => parser.AssetStore.ScoredKillEvaAnnouncers.Add(ScoredKillEvaAnnouncer.Parse(parser)) },
            { "ShadowMap", (parser, context) => parser.AssetStore.Environment.Current.ShadowMap = ShadowMap.Parse(parser) },
            { "ShellMenuScheme", (parser, context) => parser.AssetStore.ShellMenuSchemes.Add(ShellMenuScheme.Parse(parser)) },
            { "SkirmishAIData", (parser, context) => parser.AssetStore.SkirmishAIDatas.Add(SkirmishAIData.Parse(parser)) },
            { "SkyboxTextureSet", (parser, context) => parser.AssetStore.SkyboxTextureSets.Add(SkyboxTextureSet.Parse(parser)) },
            { "SpecialPower", (parser, context) => parser.AssetStore.SpecialPowers.Add(SpecialPower.Parse(parser)) },
            { "StanceTemplate", (parser, context) => parser.AssetStore.StanceTemplates.Add(StanceTemplate.Parse(parser)) },
            { "StreamedSound", (parser, context) => parser.AssetStore.StreamedSounds.Add(StreamedSound.Parse(parser)) },
            { "StaticGameLOD", (parser, context) => parser.AssetStore.StaticGameLods.Add(StaticGameLod.Parse(parser)) },
            { "StrategicHUD", (parser, context) => StrategicHud.Parse(parser, parser.AssetStore.StrategicHud.Current) },
            { "Terrain", (parser, context) => parser.AssetStore.TerrainTextures.Add(TerrainTexture.Parse(parser)) },
            { "Upgrade", (parser, context) => parser.AssetStore.Upgrades.Add(Upgrade.Parse(parser)) },
            { "VictorySystemData", (parser, context) => parser.AssetStore.VictorySystemDatas.Add(VictorySystemData.Parse(parser)) },
            { "Video", (parser, context) => parser.AssetStore.Videos.Add(Video.Parse(parser)) },
            { "WaterSet", (parser, context) => parser.AssetStore.WaterSets.Add(WaterSet.Parse(parser)) },
            { "WaterTextureList", (parser, context) => parser.AssetStore.WaterTextureLists.Add(WaterTextureList.Parse(parser)) },
            { "WaterTransparency", (parser, context) => WaterTransparency.Parse(parser, parser.AssetStore.WaterTransparency.Current) },
            { "Weapon", (parser, context) => parser.AssetStore.Weapons.Add(Weapon.Parse(parser)) },
            { "Weather", (parser, context) => Weather.Parse(parser, parser.AssetStore.Weather.Current) },
            { "WeatherData", (parser, context) => parser.AssetStore.WeatherDatas.Add(WeatherData.Parse(parser)) },
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
            return new LazyAssetReference<CommandButton>(() => AssetStore.CommandButtons.GetByName(name));
        }

        public LazyAssetReference<CommandSet> ParseCommandSetReference()
        {
            var name = ParseAssetReference();
            return new LazyAssetReference<CommandSet>(() => AssetStore.CommandSets.GetByName(name));
        }

        public LazyAssetReference<Upgrade> ParseUpgradeReference()
        {
            var name = ParseAssetReference();
            return new LazyAssetReference<Upgrade>(() => AssetStore.Upgrades.GetByName(name));
        }

        public LazyAssetReference<MappedImage> ParseMappedImageReference()
        {
            var name = ParseAssetReference();
            return new LazyAssetReference<MappedImage>(() => AssetStore.MappedImages.GetByName(name));
        }

        public LazyAssetReference<Locomotor> ParseLocomotorReference()
        {
            var name = ParseAssetReference();
            return new LazyAssetReference<Locomotor>(() => AssetStore.Locomotors.GetByName(name));
        }

        public LazyAssetReference<Graphics.Animation.W3DAnimation>[] ParseAnimationReferenceArray()
        {
            var result = new List<LazyAssetReference<Graphics.Animation.W3DAnimation>>();

            IniToken? token;
            while ((token = GetNextTokenOptional()).HasValue)
            {
                var localToken = token;
                result.Add(new LazyAssetReference<Graphics.Animation.W3DAnimation>(() => AssetStore.ModelAnimations.GetByName(localToken.Value.Text)));
            }

            return result.ToArray();
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

        public LazyAssetReference<BaseAudioEventInfo> ParseAudioEventReference()
        {
            var name = ParseAssetReference();
            return new LazyAssetReference<BaseAudioEventInfo>(() => AssetStore.AudioEvents.GetByName(name));
        }

        public LazyAssetReference<AudioFile> ParseAudioFileReference()
        {
            var name = ParseAssetReference();
            return new LazyAssetReference<AudioFile>(() => AssetStore.AudioFiles.GetByName(name));
        }

        public LazyAssetReference<AudioFileWithWeight>[] ParseAudioFileReferenceArray()
        {
            var result = new List<LazyAssetReference<AudioFileWithWeight>>();

            IniToken? token;
            while ((token = GetNextTokenOptional()).HasValue)
            {
                var localToken = token;
                result.Add(new LazyAssetReference<AudioFileWithWeight>(() => new AudioFileWithWeight
                {
                    AudioFile = new LazyAssetReference<AudioFile>(() => AssetStore.AudioFiles.GetByName(localToken.Value.Text))
                }));
            }

            return result.ToArray();
        }

        public LazyAssetReference<ObjectDefinition> ParseObjectReference()
        {
            var name = ParseAssetReference();
            return new LazyAssetReference<ObjectDefinition>(() => AssetStore.ObjectDefinitions.GetByName(name));
        }

        public LazyAssetReference<TextureAsset> ParseTextureReference()
        {
            var fileName = ParseFileName();
            return new LazyAssetReference<TextureAsset>(() => AssetStore.Textures.GetByName(fileName));
        }

        public LazyAssetReference<GuiTextureAsset> ParseGuiTextureReference()
        {
            var fileName = ParseFileName();
            return new LazyAssetReference<GuiTextureAsset>(() => AssetStore.GuiTextures.GetByName(fileName));
        }

        public LazyAssetReference<Model> ParseModelReference()
        {
            var fileName = ParseFileName();
            return (!string.Equals(fileName, "NONE", StringComparison.OrdinalIgnoreCase))
                ? new LazyAssetReference<Model>(() => AssetStore.Models.GetByName(fileName))
                : null;
        }

        public LazyAssetReference<Graphics.Animation.W3DAnimation> ParseAnimationReference()
        {
            var animationName = ParseAnimationName();
            return (!string.Equals(animationName, "NONE", StringComparison.OrdinalIgnoreCase))
                ? new LazyAssetReference<Graphics.Animation.W3DAnimation>(() => AssetStore.ModelAnimations.GetByName(animationName))
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
