using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// This AI, if armed with a weapon using the DEPLOY Damaged type, will order the passengers
    /// to hop out of the vehicle and attack the selected target. The passengers will auto return
    /// if ordered to stop or the target is dead.
    /// </summary>
    public sealed class AssaultTransportAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static AssaultTransportAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<AssaultTransportAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<AssaultTransportAIUpdateModuleData>
            {
                { "MembersGetHealedAtLifeRatio", (parser, x) => x.MembersGetHealedAtLifeRatio = parser.ParseFloat() },
            });

        public float MembersGetHealedAtLifeRatio { get; private set; }
    }
}
