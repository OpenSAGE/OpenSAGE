using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires the object to have the UndeadBody Module and allows the use of the SECOND_LIFE 
    /// Model and Armour Conditionstate.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class BattleBusSlowDeathBehaviorModuleData : SlowDeathBehaviorModuleData
    {
        internal static new BattleBusSlowDeathBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<BattleBusSlowDeathBehaviorModuleData> FieldParseTable = SlowDeathBehaviorModuleData.FieldParseTable
            .Concat(new IniParseTable<BattleBusSlowDeathBehaviorModuleData>
            {
                { "FXStartUndeath", (parser, x) => x.FXStartUndeath = parser.ParseAssetReference() },
                { "OCLStartUndeath", (parser, x) => x.OCLStartUndeath = parser.ParseAssetReference() },
                { "FXHitGround", (parser, x) => x.FXHitGround = parser.ParseAssetReference() },
                { "OCLHitGround", (parser, x) => x.OCLHitGround = parser.ParseAssetReference() },
                { "ThrowForce", (parser, x) => x.ThrowForce = parser.ParseFloat() },
                { "PercentDamageToPassengers", (parser, x) => x.PercentDamageToPassengers = parser.ParsePercentage() },
                { "EmptyHulkDestructionDelay", (parser, x) => x.EmptyHulkDestructionDelay = parser.ParseInteger() },
            });

        public string FXStartUndeath { get; private set; }
        public string OCLStartUndeath { get; private set; }
        public string FXHitGround { get; private set; }
        public string OCLHitGround { get; private set; }
        public float ThrowForce { get; private set; }
        public float PercentDamageToPassengers { get; private set; }
        public int EmptyHulkDestructionDelay { get; private set; }
    }
}
