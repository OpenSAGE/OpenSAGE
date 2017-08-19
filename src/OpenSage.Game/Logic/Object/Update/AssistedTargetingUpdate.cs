using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows weapons (or defense) to relay with a similar weapon (or defense) within its range.
    /// </summary>
    public sealed class AssistedTargetingUpdate : ObjectBehavior
    {
        internal static AssistedTargetingUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AssistedTargetingUpdate> FieldParseTable = new IniParseTable<AssistedTargetingUpdate>
        {
            { "AssistingClipSize", (parser, x) => x.AssistingClipSize = parser.ParseInteger() },
            { "AssistingWeaponSlot", (parser, x) => x.AssistingWeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "LaserFromAssisted", (parser, x) => x.LaserFromAssisted = parser.ParseAssetReference() },
            { "LaserToTarget", (parser, x) => x.LaserToTarget = parser.ParseAssetReference() }
        };

        public int AssistingClipSize { get; private set; }
        public WeaponSlot AssistingWeaponSlot { get; private set; }
        public string LaserFromAssisted { get; private set; }
        public string LaserToTarget { get; private set; }
    }
}
