using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Treats the first death as a state change. Triggers the Use of SECOND_LIFE 
    /// ConditionState/ArmorSet and allows the use of the BattleBusSlowDeathBehavior module.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class UndeadBody : ObjectBody
    {
        internal static UndeadBody Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<UndeadBody> FieldParseTable = new IniParseTable<UndeadBody>
        {
            { "SecondLifeMaxHealth", (parser, x) => x.SecondLifeMaxHealth = parser.ParseFloat() },
        }.Concat<UndeadBody, ObjectBody>(BodyFieldParseTable);

        public float SecondLifeMaxHealth { get; private set; }
    }
}
