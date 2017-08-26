using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class FireWeaponWhenDeadBehavior : ObjectBehavior
    {
        internal static FireWeaponWhenDeadBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireWeaponWhenDeadBehavior> FieldParseTable = new IniParseTable<FireWeaponWhenDeadBehavior>
        {
            { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnum<ObjectStatus>() },
            { "DeathWeapon", (parser, x) => x.DeathWeapon = parser.ParseAssetReference() },
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReferenceArray() },
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() },
            { "RequiresAllTriggers", (parser, x) => x.RequiresAllTriggers = parser.ParseBoolean() },
        };

        public ObjectStatus RequiredStatus { get; private set; }
        public string DeathWeapon { get; private set; }
        public bool StartsActive { get; private set; }
        public BitArray<DeathType> DeathTypes { get; private set; }
        public string[] ConflictsWith { get; private set; }
        public string[] TriggeredBy { get; private set; }
        public bool RequiresAllTriggers { get; private set; }
    }
}
