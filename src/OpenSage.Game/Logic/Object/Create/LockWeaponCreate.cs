using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class LockWeaponCreate : CreateModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

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

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new LockWeaponCreate();
        }
    }
}
