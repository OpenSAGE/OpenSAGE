using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using System.Numerics;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaign
    {
        internal static LivingWorldCampaign Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldCampaign> FieldParseTable = new IniParseTable<LivingWorldCampaign>
        {
            { "IsEvilCampaign", (parser, x) => x.IsEvilCampaign = parser.ParseBoolean() },

            { "Act", (parser, x) => x.Acts.Add(LivingWorldCampaignAct.Parse(parser)) }
        };

        public string Name { get; private set; }

        public bool IsEvilCampaign { get; private set; }

        public List<LivingWorldCampaignAct> Acts { get; } = new List<LivingWorldCampaignAct>();
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignAct
    {
        internal static LivingWorldCampaignAct Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldCampaignAct> FieldParseTable = new IniParseTable<LivingWorldCampaignAct>
        {
            { "EndAct", (parser, x) => x.EndAct = parser.ParseBoolean() },

            { "WorldText", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActWorldText.Parse(parser)) },
            { "AudioEvent", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActAudioEvent.Parse(parser)) },
            { "SplineCamera", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActSplineCamera.Parse(parser)) },
            { "SpawnArmy", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActSpawnArmy.Parse(parser)) },
            { "DespawnArmy", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActDespawnArmy.Parse(parser)) },
            { "ToggleArmyControl", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActToggleArmyControl.Parse(parser)) },
            { "EnableRegion", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActEnableRegion.Parse(parser)) },
            { "MoveArmy", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActMoveArmy.Parse(parser)) },
            { "ForceBattle", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActForceBattle.Parse(parser)) },
            { "EyeTowerPoints", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActEyeTowerPoints.Parse(parser)) },
            { "MoveCamera", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActMoveCamera.Parse(parser)) },
            { "ModifyArmyEntry", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActModifyArmyEntry.Parse(parser)) },
            { "MergePlayerArmy", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActMergePlayerArmy.Parse(parser)) },
            { "RegionReinforcements", (parser, x) => x.Nuggets.Add(LivingWorldCampaignActRegionReinforcements.Parse(parser)) }
        };

        public string Name { get; private set; }

        public bool EndAct { get; private set; }

        public List<LivingWorldCampaignActNugget> Nuggets { get; } = new List<LivingWorldCampaignActNugget>();
    }

    [AddedIn(SageGame.Bfme)]
    public abstract class LivingWorldCampaignActNugget
    {

    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActWorldText : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActWorldText Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldCampaignActWorldText> FieldParseTable = new IniParseTable<LivingWorldCampaignActWorldText>
        {
            { "DelayFromActStart", (parser, x) => x.DelayFromActStart = parser.ParseFloat() },
            { "StringTag", (parser, x) => x.StringTag = parser.ParseAssetReference() },
        };

        public float DelayFromActStart { get; private set; }
        public string StringTag { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActAudioEvent : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActAudioEvent Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldCampaignActAudioEvent> FieldParseTable = new IniParseTable<LivingWorldCampaignActAudioEvent>
        {
            { "DelayFromActStart", (parser, x) => x.DelayFromActStart = parser.ParseFloat() },
            { "Sound", (parser, x) => x.Sound = parser.ParseAssetReference() },
            { "SummaryEvent", (parser, x) => x.SummaryEvent = parser.ParseBoolean() },
        };

        public float DelayFromActStart { get; private set; }
        public string Sound { get; private set; }
        public bool SummaryEvent { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActDespawnArmy : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActDespawnArmy Parse(IniParser parser) => new LivingWorldCampaignActDespawnArmy { Name = parser.ParseAssetReference() };

        public string Name { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActSplineCamera : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActSplineCamera Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldCampaignActSplineCamera> FieldParseTable = new IniParseTable<LivingWorldCampaignActSplineCamera>
        {
            { "DelayFromActStart", (parser, x) => x.DelayFromActStart = parser.ParseFloat() },
            { "ControlPoint", (parser, x) => x.ControlPoints.Add(LivingWorldControlPoint.Parse(parser)) },
        };

        public float DelayFromActStart { get; private set; }
        public List<LivingWorldControlPoint> ControlPoints { get; } = new List<LivingWorldControlPoint>();
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActSpawnArmy : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActSpawnArmy Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldCampaignActSpawnArmy> FieldParseTable = new IniParseTable<LivingWorldCampaignActSpawnArmy>
        {
            { "Name", (parser, x) => x.Name = parser.ParseIdentifier() },
            { "Faction", (parser, x) => x.Faction = parser.ParseAssetReference() },
            { "PlayerArmy", (parser, x) => x.PlayerArmy = parser.ParseIdentifier() },
            { "Banner", (parser, x) => x.Banner = parser.ParseAssetReference() },
            { "Icon", (parser, x) => x.Icon = parser.ParseAssetReference() },
            { "IconSize", (parser, x) => x.IconSize = parser.ParseIdentifier() },
            { "PalantirMovie", (parser, x) => x.PalantirMovie = parser.ParseAssetReference() },
            { "PlayerOwned", (parser, x) => x.PlayerOwned = parser.ParseBoolean() },
            { "PlayerControlled", (parser, x) => x.PlayerControlled = parser.ParseBoolean() },
            { "Position", (parser, x) => x.Position = parser.ParseVector2() },
            { "IsCity", (parser, x) => x.IsCity = parser.ParseBoolean() }
        };

        public string Name { get; private set; }
        public string Faction { get; private set; }
        public string PlayerArmy { get; private set; }
        public string Banner { get; private set; }
        public string Icon { get; private set; }
        public string IconSize { get; private set; }
        public string PalantirMovie { get; private set; }
        public bool PlayerOwned { get; private set; }
        public bool PlayerControlled { get; private set; }
        public Vector2 Position { get; private set; }
        public bool IsCity { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActToggleArmyControl : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActToggleArmyControl Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldCampaignActToggleArmyControl> FieldParseTable = new IniParseTable<LivingWorldCampaignActToggleArmyControl>
        {
            { "Name", (parser, x) => x.Name = parser.ParseAssetReference() },
            { "PlayerControl", (parser, x) => x.PlayerControl = parser.ParseBoolean() },
        };

        public string Name { get; private set; }
        public bool PlayerControl { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActEnableRegion : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActEnableRegion Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldCampaignActEnableRegion> FieldParseTable = new IniParseTable<LivingWorldCampaignActEnableRegion>
        {
            { "Region", (parser, x) => x.Region = parser.ParseAssetReference() },
        };

        public string Region { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActMoveArmy : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActMoveArmy Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldCampaignActMoveArmy> FieldParseTable = new IniParseTable<LivingWorldCampaignActMoveArmy>
        {
            { "Name", (parser, x) => x.Name = parser.ParseAssetReference() },
            { "MoveTo", (parser, x) => x.MoveTo = parser.ParseVector2() },
            { "PalantirMovie", (parser, x) => x.PalantirMovie = parser.ParseAssetReference() },
            { "PlayNextActAfterMove", (parser, x) => x.PlayNextActAfterMove = parser.ParseBoolean() },
            { "MoveSpeed", (parser, x) => x.MoveSpeed = parser.ParseFloat() }
        };

        public string Name { get; private set; }
        public Vector2 MoveTo { get; private set; }
        public string PalantirMovie { get; private set; }
        public bool PlayNextActAfterMove { get; private set; }
        public float MoveSpeed { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActForceBattle : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActForceBattle Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldCampaignActForceBattle> FieldParseTable = new IniParseTable<LivingWorldCampaignActForceBattle>
        {
            { "Region", (parser, x) => x.Region = parser.ParseAssetReference() },
            { "Position", (parser, x) => x.Position = parser.ParseVector2() },
            { "UseArmy", (parser, x) => x.UseArmy = parser.ParseAssetReference() },
            { "ArmyAttackDirection", (parser, x) => x.ArmyAttackDirection = parser.ParseVector2() },
        };

        public string Region { get; private set; }
        public Vector2 Position { get; private set; }
        public string UseArmy { get; private set; }
        public Vector2 ArmyAttackDirection { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActEyeTowerPoints : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActEyeTowerPoints Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldCampaignActEyeTowerPoints> FieldParseTable = new IniParseTable<LivingWorldCampaignActEyeTowerPoints>
        {
            { "LookPoint", (parser, x) => x.LookPoints.Add(parser.ParseVector2()) },
        };

        public List<Vector2> LookPoints { get; } = new List<Vector2>();
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActMoveCamera : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActMoveCamera Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldCampaignActMoveCamera> FieldParseTable = new IniParseTable<LivingWorldCampaignActMoveCamera>
        {
            { "DelayFromActStart", (parser, x) => x.DelayFromActStart = parser.ParseFloat() },
            { "Position", (parser, x) => x.Position = parser.ParseVector3() },
            { "ViewAngle", (parser, x) => x.ViewAngle = parser.ParseInteger() },
            { "ScrollTime", (parser, x) => x.ScrollTime = parser.ParseFloat() }
        };

        public float DelayFromActStart { get; private set; }
        public Vector3 Position { get; private set; }
        public int ViewAngle { get; private set; }
        public float ScrollTime { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActModifyArmyEntry : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActModifyArmyEntry Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldCampaignActModifyArmyEntry> FieldParseTable = new IniParseTable<LivingWorldCampaignActModifyArmyEntry>
        {
            { "PlayerArmy", (parser, x) => x.PlayerArmy = parser.ParseAssetReference() },
            { "CurUnitTemplate", (parser, x) => x.CurUnitTemplate = parser.ParseAssetReference() },
            { "NewUnitTemplate", (parser, x) => x.NewUnitTemplate = parser.ParseAssetReference() }
        };

        public string PlayerArmy { get; private set; }
        public string CurUnitTemplate { get; private set; }
        public string NewUnitTemplate { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActMergePlayerArmy : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActMergePlayerArmy Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldCampaignActMergePlayerArmy> FieldParseTable = new IniParseTable<LivingWorldCampaignActMergePlayerArmy>
        {
            { "SourceArmy", (parser, x) => x.SourceArmy = parser.ParseAssetReference() },
            { "DestArmy", (parser, x) => x.DestArmy = parser.ParseAssetReference() },
            { "SplitArmyTemplate", (parser, x) => x.SplitArmyTemplate = parser.ParseAssetReference() },
            { "SplitArmy", (parser, x) => x.SplitArmy = parser.ParseBoolean() }
        };

        public string SourceArmy { get; private set; }
        public string DestArmy { get; private set; }
        public string SplitArmyTemplate { get; private set; }
        public bool SplitArmy { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldCampaignActRegionReinforcements : LivingWorldCampaignActNugget
    {
        internal static LivingWorldCampaignActRegionReinforcements Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldCampaignActRegionReinforcements> FieldParseTable = new IniParseTable<LivingWorldCampaignActRegionReinforcements>
        {
            { "RegionName", (parser, x) => x.RegionName = parser.ParseAssetReference() },
            { "AddReinforcementArmy", (parser, x) => x.AddReinforcementArmy = parser.ParseAssetReference() },
            { "CloseDistanceTime", (parser, x) => x.CloseDistanceTime = parser.ParseInteger() },
            { "MediumDistanceTime", (parser, x) => x.MediumDistanceTime = parser.ParseInteger() },
            { "FarDistanceTime", (parser, x) => x.FarDistanceTime = parser.ParseInteger() },
            { "PathFindRule", (parser, x) => x.PathFindRule = parser.ParseIdentifier() }
        };

        public string RegionName { get; private set; }
        public string AddReinforcementArmy { get; private set; }
        public int CloseDistanceTime { get; private set; }
        public int MediumDistanceTime { get; private set; }
        public int FarDistanceTime { get; private set; }
        public string PathFindRule { get; private set; }
    }
}
