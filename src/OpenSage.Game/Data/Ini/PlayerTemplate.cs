using System.Numerics;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class PlayerTemplate
    {
        internal static PlayerTemplate Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<PlayerTemplate> FieldParseTable = new IniParseTable<PlayerTemplate>
        {
            { "Side", (parser, x) => x.Side = parser.ParseAssetReference() },
            { "BaseSide", (parser, x) => x.BaseSide = parser.ParseAssetReference() },
            { "PlayableSide", (parser, x) => x.PlayableSide = parser.ParseBoolean() },
            { "IsObserver", (parser, x) => x.IsObserver = parser.ParseBoolean() },
            { "StartMoney", (parser, x) => x.StartMoney = parser.ParseInteger() },
            { "PreferredColor", (parser, x) => x.PreferredColor = IniColorRgb.Parse(parser) },
            { "IntrinsicSciences", (parser, x) => x.IntrinsicSciences = parser.ParseAssetReferenceArray() },
            { "PurchaseScienceCommandSetRank1", (parser, x) => x.PurchaseScienceCommandSetRank1 = parser.ParseAssetReference() },
            { "PurchaseScienceCommandSetRank3", (parser, x) => x.PurchaseScienceCommandSetRank3 = parser.ParseAssetReference() },
            { "PurchaseScienceCommandSetRank8", (parser, x) => x.PurchaseScienceCommandSetRank8 = parser.ParseAssetReference() },
            { "SpecialPowerShortcutCommandSet", (parser, x) => x.SpecialPowerShortcutCommandSet = parser.ParseAssetReference() },
            { "SpecialPowerShortcutWinName", (parser, x) => x.SpecialPowerShortcutWinName = parser.ParseFileName() },
            { "SpecialPowerShortcutButtonCount", (parser, x) => x.SpecialPowerShortcutButtonCount = parser.ParseInteger() },
            { "DisplayName", (parser, x) => x.DisplayName = parser.ParseLocalizedStringKey() },
            { "StartingBuilding", (parser, x) => x.StartingBuilding = parser.ParseAssetReference() },
            { "StartingUnit0", (parser, x) => x.StartingUnit0 = parser.ParseAssetReference() },
            { "ScoreScreenImage", (parser, x) => x.ScoreScreenImage = parser.ParseAssetReference() },
            { "LoadScreenImage", (parser, x) => x.LoadScreenImage = parser.ParseAssetReference() },
            { "LoadScreenMusic", (parser, x) => x.LoadScreenMusic = parser.ParseAssetReference() },
            { "FlagWaterMark", (parser, x) => x.FlagWatermark = parser.ParseAssetReference() },
            { "EnabledImage", (parser, x) => x.EnabledImage = parser.ParseAssetReference() },
            { "BeaconName", (parser, x) => x.BeaconName = parser.ParseAssetReference() },
            { "SideIconImage", (parser, x) => x.SideIconImage = parser.ParseAssetReference() },
            //CNC ZH
            { "ScoreScreenMusic", (parser, x) => x.ScoreScreenMusic = parser.ParseAssetReference() },
            { "OldFaction", (parser, x) => x.OldFaction = parser.ParseBoolean() },
            { "GeneralImage", (parser, x) => x.GeneralImage = parser.ParseAssetReference() },
            { "ArmyTooltip", (parser, x) => x.ArmyTooltip = parser.ParseLocalizedStringKey() },
            { "Features", (parser, x) => x.Features = parser.ParseLocalizedStringKey() },
            { "MedallionRegular", (parser, x) => x.MedallionRegular = parser.ParseAssetReference() },
            { "MedallionHilite", (parser, x) => x.MedallionHilite = parser.ParseAssetReference() },
            { "MedallionSelect", (parser, x) => x.MedallionSelect = parser.ParseAssetReference() },
            //BFME
            { "Evil", (parser, x) => x.PlayableSide = parser.ParseBoolean() },
            { "MaxLevelSP", (parser, x) => x.MaxLevelSP = parser.ParseInteger() },
            { "MaxLevelMP", (parser, x) => x.MaxLevelMP = parser.ParseInteger() },
            { "StartingUnit1", (parser, x) => x.StartingUnit1 = parser.ParseAssetReference() },
            { "StartingUnitOffset0", (parser, x) => x.StartingUnitOffset0 = parser.ParseVector3() },
            { "StartingUnitOffset1", (parser, x) => x.StartingUnitOffset1 = parser.ParseVector3() },
            { "StartingUnitTacticalWOTR", (parser, x) => x.StartingUnitTacticalWOTR = parser.ParseAssetReference() },
            { "IntrinsicSciencesMP", (parser, x) => x.IntrinsicSciencesMP = parser.ParseAssetReferenceArray() },
            { "SpellBook", (parser, x) => x.SpellBook = parser.ParseAssetReference() },
            { "SpellBookMp", (parser, x) => x.SpellBookMp = parser.ParseAssetReference() },
            { "PurchaseScienceCommandSet", (parser, x) => x.PurchaseScienceCommandSet = parser.ParseAssetReference() },
            { "PurchaseScienceCommandSetMP", (parser, x) => x.PurchaseScienceCommandSetMP = parser.ParseAssetReference() },
            { "DefaultPlayerAIType", (parser, x) => x.DefaultPlayerAIType = parser.ParseAssetReference() },
            { "LightPointsUpSound", (parser, x) => x.LightPointsUpSound = parser.ParseAssetReference() },
            { "ObjectiveAddedSound", (parser, x) => x.ObjectiveAddedSound = parser.ParseAssetReference() },
            { "ObjectiveCompletedSound", (parser, x) => x.ObjectiveCompletedSound = parser.ParseAssetReference() },
            { "InitialUpgrades", (parser, x) => x.InitialUpgrades = parser.ParseAssetReferenceArray() },
            { "BuildableHeroesMP", (parser, x) => x.BuildableHeroesMP = parser.ParseAssetReferenceArray() },
            { "BuildableRingHeroesMP", (parser, x) => x.BuildableRingHeroesMP = parser.ParseAssetReferenceArray() },
            { "SpellStoreCurrentPowerLabel", (parser, x) => x.SpellStoreCurrentPowerLabel = parser.ParseAssetReference() },
            { "SpellStoreMaximumPowerLabel", (parser, x) => x.SpellStoreMaximumPowerLabel = parser.ParseAssetReference() },
            { "ResourceModifierObjectFilter", (parser, x) => x.ResourceModifierObjectFilter = parser.ParseString() },
            { "ResourceModifierValues", (parser, x) => x.ResourceModifierValues = parser.ParseIntegerArray() },
            { "MultiSelectionPortrait", (parser, x) => x.MultiSelectionPortrait = parser.ParseAssetReference() },
        };

        public string Name { get; private set; }

        public string Side { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string BaseSide { get; private set; }

        public bool PlayableSide { get; private set; }
        public bool IsObserver { get; private set; }
        public int StartMoney { get; private set; }
        public IniColorRgb PreferredColor { get; private set; }
        public string[] IntrinsicSciences { get; private set; }
        public string PurchaseScienceCommandSetRank1 { get; private set; }
        public string PurchaseScienceCommandSetRank3 { get; private set; }
        public string PurchaseScienceCommandSetRank8 { get; private set; }
        public string SpecialPowerShortcutCommandSet { get; private set; }
        public string SpecialPowerShortcutWinName { get; private set; }
        public int SpecialPowerShortcutButtonCount { get; private set; }
        public string DisplayName { get; private set; }
        public string StartingBuilding { get; private set; }
        public string StartingUnit0 { get; private set; }
        public string ScoreScreenImage { get; private set; }
        public string LoadScreenImage { get; private set; }
        public string LoadScreenMusic { get; private set; }
        public string FlagWatermark { get; private set; }
        public string EnabledImage { get; private set; }
        public string BeaconName { get; private set; }
        public string SideIconImage { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string ScoreScreenMusic { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool OldFaction { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string GeneralImage { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string ArmyTooltip { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string Features { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string MedallionRegular { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string MedallionHilite { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string MedallionSelect { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool Evil { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MaxLevelSP { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MaxLevelMP { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Vector3 StartingUnitOffset0 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string StartingUnit1 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Vector3 StartingUnitOffset1 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string[] IntrinsicSciencesMP { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string StartingUnitTacticalWOTR { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SpellBook { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SpellBookMp { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string PurchaseScienceCommandSet { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string PurchaseScienceCommandSetMP { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string DefaultPlayerAIType { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string LightPointsUpSound { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string  ObjectiveAddedSound { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string  ObjectiveCompletedSound { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string[] InitialUpgrades { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string[] BuildableHeroesMP { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string[] BuildableRingHeroesMP { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SpellStoreCurrentPowerLabel { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SpellStoreMaximumPowerLabel { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string ResourceModifierObjectFilter { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int[] ResourceModifierValues { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string MultiSelectionPortrait { get; private set; }
    }
}
