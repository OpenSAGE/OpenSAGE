using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CleanupAreaPowerModuleData : SpecialPowerModuleData
    {
        internal static new CleanupAreaPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CleanupAreaPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<CleanupAreaPowerModuleData>
            {
                { "MaxMoveDistanceFromLocation", (parser, x) => x.MaxMoveDistanceFromLocation = parser.ParseFloat() }
            });

        public float MaxMoveDistanceFromLocation { get; private set; }
    }
}
