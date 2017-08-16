using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class TurretAIData
    {
        internal static TurretAIData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TurretAIData> FieldParseTable = new IniParseTable<TurretAIData>
        {
            { "TurretTurnRate", (parser, x) => x.TurretTurnRate = parser.ParseInteger() },
            { "ControlledWeaponSlots", (parser, x) => x.ControlledWeaponSlots = parser.ParseEnum<WeaponSlot>() }
        };

        public int TurretTurnRate { get; private set; }
        public WeaponSlot ControlledWeaponSlots { get; private set; }
    }
}
