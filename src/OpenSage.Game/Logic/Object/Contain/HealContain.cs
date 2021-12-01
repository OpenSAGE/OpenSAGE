using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class HealContain : OpenContainModule
    {
        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    /// <summary>
    /// Automatically heals and restores the health of units that enter or exit the object.
    /// </summary>
    public sealed class HealContainModuleData : GarrisonContainModuleData
    {
        internal static new HealContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<HealContainModuleData> FieldParseTable = GarrisonContainModuleData.FieldParseTable
            .Concat(new IniParseTable<HealContainModuleData>
            {
                { "TimeForFullHeal", (parser, x) => x.TimeForFullHeal = parser.ParseInteger() }
            });

        public int TimeForFullHeal { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new HealContain();
        }
    }
}
