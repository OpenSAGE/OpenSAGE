using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldArmyIcon
    {
        internal static LivingWorldArmyIcon Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldArmyIcon> FieldParseTable = new IniParseTable<LivingWorldArmyIcon>
        {
            { "OnSelectedSound", (parser, x) => x.OnSelectedSound = parser.ParseAssetReference() },
            { "OnMoveSound", (parser, x) => x.OnMoveSound = parser.ParseAssetReference() },
            { "Object", (parser, x) => x.Objects.Add(Object.Parse(parser)) },
            { "OnMovePlannedSound", (parser, x) => x.OnMovePlannedSound = parser.ParseAssetReference() },
            { "OnMoveStartedSound", (parser, x) => x.OnMoveStartedSound = parser.ParseAssetReference() },
            { "WelcomeReinforcementsSound", (parser, x) => x.WelcomeReinforcementsSound = parser.ParseAssetReference() },
            { "KickOutReinforcementsSound", (parser, x) => x.KickOutReinforcementsSound = parser.ParseAssetReference() },
            { "DisbandUnitSound", (parser, x) => x.DisbandUnitSound = parser.ParseAssetReference() },
            { "RetreatTeleportToHomeRegionEvaEvent", (parser, x) => x.RetreatTeleportToHomeRegionEvaEvent = parser.ParseAssetReference() },
            { "RetreatTeleportToNonHomeRegionEvaEvent", (parser, x) => x.RetreatTeleportToNonHomeRegionEvaEvent = parser.ParseAssetReference() },
        };

        public string OnSelectedSound { get; private set; }
        public string OnMoveSound { get; private set; }
        public List<Object> Objects { get; } = new List<Object>();

        [AddedIn(SageGame.Bfme2)]
        public string OnMovePlannedSound { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string OnMoveStartedSound { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string WelcomeReinforcementsSound { get; private set; } // Sound played when garrison joins with another garrison

        [AddedIn(SageGame.Bfme2)]
        public string KickOutReinforcementsSound { get; private set; } // Sound played when we split off some troops

        [AddedIn(SageGame.Bfme2)]
        public string DisbandUnitSound { get; private set; } // Sound when units are dismissed

        [AddedIn(SageGame.Bfme2)]
        public string RetreatTeleportToHomeRegionEvaEvent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string RetreatTeleportToNonHomeRegionEvaEvent { get; private set; }
    }

    public sealed class Object
    {
        internal static Object Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Object> FieldParseTable = new IniParseTable<Object>
        {
            { "Model", (parser, x) => x.Model = parser.ParseAssetReference() },
            { "ZOffset", (parser, x) => x.ZOffset = parser.ParseInteger() },
            { "Scale", (parser, x) => x.Scale = parser.ParseFloat() },
            { "Clickable", (parser, x) => x.Clickable = parser.ParseBoolean() },
            { "Hidden", (parser, x) => x.Hidden = parser.ParseBoolean() },
            { "CanFade", (parser, x) => x.CanFade = parser.ParseBoolean() },
            { "OrientAngle", (parser, x) => x.OrientAngle = parser.ParseFloat() },
            { "Pickbox", (parser, x) => x.Pickbox = parser.ParseAssetReference() },
            { "VisibleArmySizes", (parser, x) => x.VisibleArmySizes = parser.ParseEnumFlags<ArmySizes>() },
            { "SubObjects", (parser, x) => x.SubObjects = parser.ParseAssetReferenceArray() },
            { "Shadow", (parser, x) => x.Shadow = parser.ParseAssetReference() },
            { "UseHouseColor", (parser, x) => x.UseHouseColor = parser.ParseBoolean() },
            { "HideWhenUnhilighted", (parser, x) => x.HideWhenUnhilighted = parser.ParseBoolean() },
            { "FadeTypeForHilighting", (parser, x) => x.FadeTypeForHilighting = parser.ParseEnum<FadeType>() },
            { "FadeTypeForUnhilighting", (parser, x) => x.FadeTypeForUnhilighting = parser.ParseEnum<FadeType>() },
            { "FadeMethod", (parser, x) => x.FadeMethod = parser.ParseString() },
            { "HideWhenUnselected", (parser, x) => x.HideWhenUnselected = parser.ParseBoolean() },
            { "FadeTypeForSelection", (parser, x) => x.FadeTypeForSelection = parser.ParseEnum<FadeType>() },
            { "FadeHoldPercent", (parser, x) => x.FadeHoldPercent = parser.ParsePercentage() },
            { "DisplayAtRallyPoint", (parser, x) => x.DisplayAtRallyPoint = parser.ParseBoolean() },
            { "ShowOnlyAfterMoveOrder", (parser, x) => x.ShowOnlyAfterMoveOrder = parser.ParseBoolean() },
            { "ShowOnlyForAllies", (parser, x) => x.ShowOnlyForAllies = parser.ParseBoolean() }
        };

        public string Name { get; private set; }

        public string Model { get; private set; }
        public int ZOffset { get; private set; }
        public float Scale { get; private set; }
        public bool Clickable { get; private set; }
        public bool Hidden { get; private set; }
        public bool CanFade { get; private set; }
        public float OrientAngle { get; private set; }
        public string Pickbox { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ArmySizes VisibleArmySizes { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string[] SubObjects { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string Shadow { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool UseHouseColor { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool HideWhenUnhilighted { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public FadeType FadeTypeForHilighting { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public FadeType FadeTypeForUnhilighting { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string FadeMethod { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool HideWhenUnselected { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public FadeType FadeTypeForSelection { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float FadeHoldPercent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool DisplayAtRallyPoint { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ShowOnlyAfterMoveOrder { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ShowOnlyForAllies { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public enum ArmySizes
    {
        [IniEnum("SMALL")]
        Small,

        [IniEnum("MEDIUM")]
        Medium,

        [IniEnum("LARGE")]
        Large,
    }

    [AddedIn(SageGame.Bfme2)]
    public enum FadeType
    {
        [IniEnum("NONE")]
        None,

        [IniEnum("IN")]
        In,

        [IniEnum("OUT")]
        Out,

        [IniEnum("INOUT")]
        InOut,
    }
}
