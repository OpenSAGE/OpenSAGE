using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class SpecialAbilityModule : SpecialPowerModule
    {
        // TODO

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    public sealed class SpecialAbilityModuleData : SpecialPowerModuleData
    {
        internal static new SpecialAbilityModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SpecialAbilityModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<SpecialAbilityModuleData>());

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new SpecialAbilityModule();
        }
    }
}
