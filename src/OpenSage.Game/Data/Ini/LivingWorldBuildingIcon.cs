using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class LivingWorldBuildingIcon
    {
        internal static LivingWorldBuildingIcon Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldBuildingIcon> FieldParseTable = new IniParseTable<LivingWorldBuildingIcon>
        {
            { "OnSelectedSound", (parser, x) => x.OnSelectedSound = parser.ParseAssetReference() },
            { "OnConstructionBegunSound", (parser, x) => x.OnConstructionBegunSound = parser.ParseAssetReference() },
            { "OnConstructionFinishedSound", (parser, x) => x.OnConstructionFinishedSound = parser.ParseAssetReference() },
            { "Object", (parser, x) => x.Objects.Add(BuildingIconObject.Parse(parser)) },
        };

        public string Name { get; private set; }

        public string OnSelectedSound { get; private set; }
        public string OnConstructionBegunSound { get; private set; }
        public string OnConstructionFinishedSound { get; private set; }
        public List<BuildingIconObject> Objects { get; } = new List<BuildingIconObject>();
    }

    public sealed class BuildingIconObject
    {
        internal static BuildingIconObject Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<BuildingIconObject> FieldParseTable = new IniParseTable<BuildingIconObject>
        {
            { "Model", (parser, x) => x.Model = parser.ParseAssetReference() },
            { "SubObjects", (parser, x) => x.SubObjects = parser.ParseAssetReferenceArray() },
            { "OrientAngle", (parser, x) => x.OrientAngle = parser.ParseFloat() },
            { "ZOffset", (parser, x) => x.ZOffset = parser.ParseInteger() },
            { "Scale", (parser, x) => x.Scale = parser.ParseFloat() },
            { "Clickable", (parser, x) => x.Clickable = parser.ParseBoolean() },
            { "Pickbox", (parser, x) => x.Pickbox = parser.ParseAssetReference() },
            { "Shadow", (parser, x) => x.Shadow = parser.ParseAssetReference() },
            { "HideWhenUnderConstruction", (parser, x) => x.HideWhenUnderConstruction = parser.ParseBoolean() },
            { "HideWhenNotProducing", (parser, x) => x.HideWhenNotProducing = parser.ParseBoolean() },
            { "ShowOnlyForAllies", (parser, x) => x.ShowOnlyForAllies = parser.ParseBoolean() },
            { "HideWhenNotUnderConstruction", (parser, x) => x.HideWhenNotUnderConstruction = parser.ParseBoolean() },
            { "FadeTypeForHilighting", (parser, x) => x.FadeTypeForHilighting = parser.ParseEnum<FadeType>() },
            { "FadeTypeForUnhilighting", (parser, x) => x.FadeTypeForUnhilighting = parser.ParseEnum<FadeType>() },
            { "AnimMode", (parser, x) => x.AnimMode = parser.ParseEnum<AnimationMode>() },
            { "HideWhenUnhilighted", (parser, x) => x.HideWhenUnhilighted = parser.ParseBoolean() },
            { "FadeMethod", (parser, x) => x.FadeMethod = parser.ParseString() },
            { "HideWhenUnselected", (parser, x) => x.HideWhenUnselected = parser.ParseBoolean() },
            { "FadeTypeForSelection", (parser, x) => x.FadeTypeForSelection = parser.ParseEnum<FadeType>() },
            { "FadeHoldPercent", (parser, x) => x.FadeHoldPercent = parser.ParsePercentage() },
            { "FadeTypeForShowing", (parser, x) => x.FadeTypeForShowing = parser.ParseEnum<FadeType>() },
            { "FadeTypeForHiding", (parser, x) => x.FadeTypeForHiding = parser.ParseEnum<FadeType>() },
            { "FadeInTime", (parser, x) => x.FadeInTime = parser.ParseInteger() },
            { "FadeOutTime", (parser, x) => x.FadeOutTime = parser.ParseInteger() }
        };

        public string Name { get; private set; }

        public string Model { get; private set; }
        public string[] SubObjects { get; private set; }
        public float OrientAngle { get; private set; }
        public int ZOffset { get; private set; }
        public float Scale { get; private set; }
        public bool Clickable { get; private set; }
        public string Pickbox { get; private set; }
        public string Shadow { get; private set; }
        public bool HideWhenUnderConstruction { get; private set; }
        public bool HideWhenNotUnderConstruction { get; private set; }
        public bool HideWhenNotProducing { get; private set; }
        public bool ShowOnlyForAllies { get; private set; }
        public FadeType FadeTypeForHilighting { get; private set; }
        public FadeType FadeTypeForUnhilighting { get; private set; }
        public AnimationMode AnimMode { get; private set; }
        public bool HideWhenUnhilighted { get; private set; }
        public string FadeMethod { get; private set; }
        public bool HideWhenUnselected { get; private set; }
        public FadeType FadeTypeForSelection { get; private set; }
        public Percentage FadeHoldPercent { get; private set; }
        public FadeType FadeTypeForShowing { get; private set; }
        public FadeType FadeTypeForHiding { get; private set; }
        public int FadeInTime { get; private set; }
        public int FadeOutTime { get; private set; }
    }
}
