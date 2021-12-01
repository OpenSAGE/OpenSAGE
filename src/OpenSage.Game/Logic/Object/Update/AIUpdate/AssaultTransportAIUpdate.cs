using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class AssaultTransportAIUpdate : AIUpdate
    {
        internal AssaultTransportAIUpdate(GameObject gameObject, AIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
        }

        // TODO

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var memberCount = reader.ReadInt32();
            for (var i = 0; i < memberCount; i++)
            {
                var objectId = reader.ReadUInt32();
                var unknownBool = reader.ReadBoolean();
            }

            for (var i = 0; i < 26; i++)
            {
                var unknownByte = reader.ReadByte();
                if (unknownByte != 0)
                {
                    throw new InvalidStateException();
                }
            }
        }
    }

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

        internal override AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new AssaultTransportAIUpdate(gameObject, this);
        }
    }
}
