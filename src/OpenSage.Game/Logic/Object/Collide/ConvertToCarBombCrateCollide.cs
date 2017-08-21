using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Triggers use of CARBOMB WeaponSet Condition of the hijacked object and turns it to a 
    /// suicide unit unless given with a different weapon.
    /// </summary>
    public sealed class ConvertToCarBombCrateCollide : ObjectBehavior
    {
        internal static ConvertToCarBombCrateCollide Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ConvertToCarBombCrateCollide> FieldParseTable = new IniParseTable<ConvertToCarBombCrateCollide>
        {
            { "RequiredKindOf", (parser, x) => x.RequiredKindOf = parser.ParseEnum<ObjectKinds>() },
            { "FXList", (parser, x) => x.FXList = parser.ParseAssetReference() }
        };

        public ObjectKinds RequiredKindOf { get; private set; }
        public string FXList { get; private set; }
    }
}
