using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class AssistedTargetingUpdate : UpdateModule
    {
        // TODO
    }

    /// <summary>
    /// Allows weapons (or defense) to relay with a similar weapon (or defense) within its range.
    /// </summary>
    public sealed class AssistedTargetingUpdateModuleData : UpdateModuleData
    {
        internal static AssistedTargetingUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AssistedTargetingUpdateModuleData> FieldParseTable = new IniParseTable<AssistedTargetingUpdateModuleData>
        {
            { "AssistingClipSize", (parser, x) => x.AssistingClipSize = parser.ParseInteger() },
            { "AssistingWeaponSlot", (parser, x) => x.AssistingWeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "LaserFromAssisted", (parser, x) => x.LaserFromAssisted = parser.ParseAssetReference() },
            { "LaserToTarget", (parser, x) => x.LaserToTarget = parser.ParseAssetReference() }
        };

        public int AssistingClipSize { get; private set; } = 1;
        public WeaponSlot AssistingWeaponSlot { get; private set; } = WeaponSlot.Primary;
        public string LaserFromAssisted { get; private set; }
        public string LaserToTarget { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new AssistedTargetingUpdate();
        }
    }
}
