using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class DefectorSpecialPower : SpecialPowerModule
    {
        internal DefectorSpecialPower(GameObject gameObject, GameContext context, DefectorSpecialPowerModuleData moduleData) : base(gameObject, context, moduleData)
        {
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
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

        internal override DefectorSpecialPower CreateModule(GameObject gameObject, GameContext context)
        {
            return new DefectorSpecialPower(gameObject, context, this);
        }
    }
}
