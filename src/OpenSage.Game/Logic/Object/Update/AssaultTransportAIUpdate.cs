using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// This AI, if armed with a weapon using the DEPLOY Damaged type, will order the passengers
    /// to hop out of the vehicle and attack the selected target. The passengers will auto return
    /// if ordered to stop or the target is dead.
    /// </summary>
    public sealed class AssaultTransportAIUpdate : ObjectBehavior
    {
        internal static AssaultTransportAIUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AssaultTransportAIUpdate> FieldParseTable = new IniParseTable<AssaultTransportAIUpdate>
        {
            { "MembersGetHealedAtLifeRatio", (parser, x) => x.MembersGetHealedAtLifeRatio = parser.ParseFloat() },
        };

        public float MembersGetHealedAtLifeRatio { get; private set; }
    }
}
