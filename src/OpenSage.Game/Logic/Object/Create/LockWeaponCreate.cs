using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Prevents indeterminate state plus two unpickable weapons equaling no attack.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class LockWeaponCreateModuleData : CreateModuleData
    {
        internal static LockWeaponCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LockWeaponCreateModuleData> FieldParseTable = new IniParseTable<LockWeaponCreateModuleData>
        {
            { "SlotToLock", (parser, x) => x.SlotToLock = parser.ParseEnum<WeaponSlot>() }
        };

        public WeaponSlot SlotToLock { get; private set; }
    }
}
