using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Keeps this object attached properly to the intended target if the targetted object moves 
    /// and allows the use of UnitBombPing and StickyBombCreated within the UnitSpecificSounds section 
    /// of the object.
    /// </summary>
    public sealed class StickyBombUpdateModuleData : UpdateModuleData
    {
        internal static StickyBombUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StickyBombUpdateModuleData> FieldParseTable = new IniParseTable<StickyBombUpdateModuleData>
        {
             { "GeometryBasedDamageWeapon", (parser, x) => x.GeometryBasedDamageWeapon = parser.ParseAssetReference() },
             { "GeometryBasedDamageFX", (parser, x) => x.GeometryBasedDamageFX = parser.ParseAssetReference() },
        };

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string GeometryBasedDamageWeapon { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string GeometryBasedDamageFX { get; private set; }
    }
}
