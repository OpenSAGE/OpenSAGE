using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Prevents indeterminate state plus two unpickable weapons equaling no attack.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class LockWeaponCreate : ObjectBehavior
    {
        internal static LockWeaponCreate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LockWeaponCreate> FieldParseTable = new IniParseTable<LockWeaponCreate>
        {
            { "SlotToLock", (parser, x) => x.SlotToLock = parser.ParseEnum<WeaponSlot>() }
        };

        public WeaponSlot SlotToLock { get; private set; }
    }
}
