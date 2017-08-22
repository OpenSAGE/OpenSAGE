using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of PLAYER_UPGRADE WeaponSet on this object.
    /// Allows the use of WeaponUpgradeSound within UnitSpecificSounds section of the object.
    /// Allows the use of the WEAPONSET_PLAYER_UPGRADE ConditionState.
    /// </summary>
    public sealed class WeaponSetUpgrade : ObjectBehavior
    {
        internal static WeaponSetUpgrade Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<WeaponSetUpgrade> FieldParseTable = new IniParseTable<WeaponSetUpgrade>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() }
        };

        public string[] TriggeredBy { get; private set; }
    }
}
