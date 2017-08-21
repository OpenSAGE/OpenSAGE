using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to play the HijackDriver sound definition when triggered and converts the unit to 
    /// your side.
    /// </summary>
    public sealed class ConvertToHijackedVehicleCrateCollide : ObjectBehavior
    {
        internal static ConvertToHijackedVehicleCrateCollide Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ConvertToHijackedVehicleCrateCollide> FieldParseTable = new IniParseTable<ConvertToHijackedVehicleCrateCollide>
        {
            { "RequiredKindOf", (parser, x) => x.RequiredKindOf = parser.ParseEnum<ObjectKinds>() }
        };

        public ObjectKinds RequiredKindOf { get; private set; }
    }
}
