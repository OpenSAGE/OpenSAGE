using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class LivingWorldBuilding
    {
        internal static LivingWorldBuilding Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldBuilding> FieldParseTable = new IniParseTable<LivingWorldBuilding>
        {
            { "AvailableTo", (parser, x) => x.AvailableTo = parser.ParseString() },
            { "BattleThingTemplate", (parser, x) => x.BattleThingTemplate = parser.ParseString() },
            { "BuildingIcon", (parser, x) => x.BuildingIcon = parser.ParseAssetReference() },
            { "TurnsToBuild", (parser, x) => x.TurnsToBuild = parser.ParseInteger() },
            { "ConstructButtonImage", (parser, x) => x.ConstructButtonImage = parser.ParseLocalizedStringKey() },
            { "ConstructButtonTitle", (parser, x) => x.ConstructButtonTitle = parser.ParseLocalizedStringKey() },
            { "ConstructButtonHelp", (parser, x) => x.ConstructButtonHelp = parser.ParseLocalizedStringKey() },
            { "DisplayNameTag", (parser, x) => x.DisplayNameTag = parser.ParseLocalizedStringKey() },
            { "DisplayDescriptionTag", (parser, x) => x.DisplayDescriptionTag = parser.ParseLocalizedStringKey() },
            { "CreateUnitDuringAutoResolve", (parser, x) => x.CreateUnitDuringAutoResolve = parser.ParseBoolean() },
            { "CanDefendTerritory", (parser, x) => x.CanDefendTerritory = parser.ParseBoolean() },
            { "Type", (parser, x) => x.Type = parser.ParseString() },
            { "BuildingNugget", (parser, x) => x.BuildingNuggets.Add(BuildingNugget.Parse(parser)) },
            { "StrategicResourceCost", (parser, x) => x.StrategicResourceCost = parser.ParseInteger() }
        };

        public string Name { get; private set; }

        public string AvailableTo { get; private set; }
        public string BattleThingTemplate { get; private set; }
        public string BuildingIcon { get; private set; } 
        public int TurnsToBuild { get; private set; } 
        public string ConstructButtonImage { get; private set; } 
        public string ConstructButtonTitle { get; private set; } 
        public string ConstructButtonHelp { get; private set; } 
        public string DisplayNameTag { get; private set; } 
        public string DisplayDescriptionTag { get; private set; } 
        public bool CreateUnitDuringAutoResolve { get; private set; } 
        public bool CanDefendTerritory { get; private set; } 
        public string Type { get; private set; }
        public List<BuildingNugget> BuildingNuggets { get; } = new List<BuildingNugget>();

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int StrategicResourceCost { get; private set; }
    }

    public class BuildingNugget
    {
        internal static BuildingNugget Parse(IniParser parser)
        {
            var name = parser.ParseString();
            var tag = parser.ParseString();
            var result = parser.ParseBlock(FieldParseTable);
            result.Name = name;
            result.Tag = tag;
            return result;
        }

        private static readonly IniParseTable<BuildingNugget> FieldParseTable = new IniParseTable<BuildingNugget>
        {
            { "StrengtheningRange", (parser, x) => x.StrengtheningRange = parser.ParseString() },
            { "BonusKey", (parser, x) => x.BonusKey = parser.ParseString() },
            { "Bonus", (parser, x) => x.Boni.Add(Bonus.Parse(parser)) },
            { "QueueSize", (parser, x) => x.QueueSize = parser.ParseInteger() },
            { "ArmyToSpawn", (parser, x) => x.ArmysToSpawn.Add(ArmyToSpawn.Parse(parser)) },
            { "NumUpgradesPerTurn", (parser, x) => x.NumUpgradesPerTurn = parser.ParseInteger() },
            { "UpgradeableUnits", (parser, x) => x.UpgradeableUnits = parser.ParseAssetReferenceArray() },
            { "Amount", (parser, x) => x.Amount = parser.ParseInteger() },
            { "Type", (parser, x) => x.Type = parser.ParseString() },
            { "TreasureAmount", (parser, x) => x.TreasureAmount = parser.ParseInteger() }
        };

        public string Name { get; private set; }
        public string Tag { get; private set; }

        public string StrengtheningRange { get; private set; } //should be a enum? is always "THIS_TERRIORITY" which is never defined
        public string BonusKey { get; private set; }
        public List<Bonus> Boni { get; } = new List<Bonus>();
        public int QueueSize { get; private set; }
        public List<ArmyToSpawn> ArmysToSpawn { get; } = new List<ArmyToSpawn>();
        public int NumUpgradesPerTurn { get; private set; }
        public string[] UpgradeableUnits { get; private set; }
        public int Amount { get; private set; }
        public string Type { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int TreasureAmount { get; private set; }
    }

    public class Bonus
    {
        internal static Bonus Parse(IniParser parser)
        {
            return new Bonus
            {
                ID = parser.ParseInteger(),
                Armor = parser.ParseAttributePercentage("Armor")
            };
        }


        public int ID { get; private set; }
        public float Armor { get; private set; }
    }

    public class ArmyToSpawn
    {
        internal static ArmyToSpawn Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ArmyToSpawn> FieldParseTable = new IniParseTable<ArmyToSpawn>
        {
            { "PlayerArmy", (parser, x) => x.PlayerArmy = parser.ParseString() },
            { "Icon", (parser, x) => x.Icon = parser.ParseString() },
            { "IconSize", (parser, x) => x.IconSize = parser.ParseString() },
            { "PalantirMovie", (parser, x) => x.PalantirMovie = parser.ParseAssetReference() },
            { "BuildTime", (parser, x) => x.BuildTime = parser.ParseInteger() },
            { "ConstructButtonImage", (parser, x) => x.ConstructButtonImage = parser.ParseLocalizedStringKey() },
            { "ConstructButtonTitle", (parser, x) => x.ConstructButtonTitle = parser.ParseLocalizedStringKey() },
            { "ConstructButtonHelp", (parser, x) => x.ConstructButtonHelp = parser.ParseLocalizedStringKey() },
            { "HeroTemplateName", (parser, x) => x.HeroTemplateName = parser.ParseString() }
        };

        public string PlayerArmy { get; private set; }
        public string Icon { get; private set; }
        public string IconSize { get; private set; }
        public string PalantirMovie { get; private set; }
        public int BuildTime { get; private set; }
        public string ConstructButtonImage { get; private set; }
        public string ConstructButtonTitle { get; private set; }
        public string ConstructButtonHelp { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string HeroTemplateName { get; private set; }
    }
}
