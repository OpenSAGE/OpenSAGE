using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Treats the first death as a state change. Triggers the Use of SECOND_LIFE 
    /// ModelConditionState/ArmorSet and allows the use of the BattleBusSlowDeathBehavior module.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class UndeadBodyModuleData : ActiveBodyModuleData
    {
        internal static new UndeadBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<UndeadBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
            .Concat(new IniParseTable<UndeadBodyModuleData>
            {
                { "SecondLifeMaxHealth", (parser, x) => x.SecondLifeMaxHealth = parser.ParseFloat() },
            });

        public float SecondLifeMaxHealth { get; private set; }
    }
}
