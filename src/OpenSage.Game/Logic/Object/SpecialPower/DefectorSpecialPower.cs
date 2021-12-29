using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class DefectorSpecialPower : SpecialPowerModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    /// <summary>
    /// When used in junction with the SPECIAL_DEFECTOR special power, the unit will defect to 
    /// your side.
    /// </summary>
    public sealed class DefectorSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new DefectorSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DefectorSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<DefectorSpecialPowerModuleData>());

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new DefectorSpecialPower();
        }
    }
}
