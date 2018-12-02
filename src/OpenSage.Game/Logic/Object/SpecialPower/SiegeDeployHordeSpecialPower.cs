using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SiegeDeployHordeSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new SiegeDeployHordeSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SiegeDeployHordeSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<SiegeDeployHordeSpecialPowerModuleData>
            {
                { "HordeDeploy", (parser, x) => x.HordeDeploy = parser.ParseBoolean() }
            });

        public bool HordeDeploy { get; private set; }
    }
}
