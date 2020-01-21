using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class HordeAttackNugget : WeaponEffectNugget
    {
        internal static HordeAttackNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<HordeAttackNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<HordeAttackNugget>
            {
                { "LockWeaponSlot", (parser, x) => x.LockWeaponSlot = parser.ParseEnum<WeaponSlot>() },
            });

        [AddedIn(SageGame.Bfme2)]
        public WeaponSlot LockWeaponSlot { get; private set; }
    }
}
