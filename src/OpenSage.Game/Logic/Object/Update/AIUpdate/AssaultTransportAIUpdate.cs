using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class AssaultTransportAIUpdate : AIUpdate
    {
        private readonly List<AssaultTransportMember> _members = new();

        internal AssaultTransportAIUpdate(GameObject gameObject, AIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
        }

        // TODO

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var memberCount = _members.Count;
            reader.ReadInt32(ref memberCount);

            for (var i = 0; i < memberCount; i++)
            {
                var member = new AssaultTransportMember();
                reader.ReadObjectID(ref member.ObjectId);
                reader.ReadBoolean(ref member.Unknown);
                _members.Add(member);
            }

            reader.SkipUnknownBytes(26);
        }

        private struct AssaultTransportMember
        {
            public uint ObjectId;
            public bool Unknown;
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
