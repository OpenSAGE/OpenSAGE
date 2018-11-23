using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to use the GarrisonGun object definition for the weapons pointing from the object 
    /// when occupants are firing and these are drawn at bones named FIREPOINT. Also, it Allows use 
    /// of the GARRISONED Model ModelConditionState.
    /// </summary>
    public class GarrisonContainModuleData : OpenContainModuleData
    {
        internal static GarrisonContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<GarrisonContainModuleData> FieldParseTable = OpenContainModuleData.FieldParseTable
            .Concat(new IniParseTable<GarrisonContainModuleData>
            {
                { "MobileGarrison", (parser, x) => x.MobileGarrison = parser.ParseBoolean() },
                { "InitialRoster", (parser, x) => x.InitialRoster = InitialRoster.Parse(parser) },
                { "ImmuneToClearBuildingAttacks", (parser, x) => x.ImmuneToClearBuildingAttacks = parser.ParseBoolean() },
                { "IsEnclosingContainer", (parser, x) => x.IsEnclosingContainer = parser.ParseBoolean() },
            });
        
        public bool MobileGarrison { get; private set; }
        public InitialRoster InitialRoster { get; private set; }
        public bool ImmuneToClearBuildingAttacks { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool IsEnclosingContainer { get; private set; }
    }

    public sealed class InitialRoster
    {
        internal static InitialRoster Parse(IniParser parser)
        {
            return new InitialRoster
            {
                TemplateId = parser.ParseAssetReference(),
                Count = parser.ParseInteger()
            };
        }

        public string TemplateId { get; private set; }
        public int Count { get; private set; }
    }
}
