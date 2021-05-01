using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data.Sav;

namespace OpenSage.Logic
{
    public sealed class TeamTemplate
    {
        private uint _playerId;
        private string _attackPriorityName;
        private TeamTemplateData _templateData;

        public readonly uint ID;
        public readonly string Name;
        public readonly Player Owner;
        public readonly bool IsSingleton;

        public TeamTemplate(uint id, string name, Player owner, bool isSingleton)
        {
            ID = id;
            Name = name;
            Owner = owner;
            IsSingleton = isSingleton;

            // TODO: Read this from map data.
            _templateData = new TeamTemplateData();
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            _playerId = reader.ReadUInt32();

            _attackPriorityName = reader.ReadAsciiString();

            var unknown2 = reader.ReadBoolean();

            _templateData.Load(reader);

            var teamCount = reader.ReadUInt16();
            for (var i = 0; i < teamCount; i++)
            {
                var id = reader.ReadUInt32();

                var team = new Team { ID = id };
                team.Load(reader);
            }
        }
    }

    internal sealed class TeamTemplateData
    {
        public int ProductionPriority;

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            ProductionPriority = reader.ReadInt32();
        }
    }

    internal sealed class Team
    {
        public uint ID;
        public List<uint> ObjectIds = new List<uint>();

        public readonly PlayerRelationships TeamToTeamRelationships = new PlayerRelationships();
        public readonly PlayerRelationships TeamToPlayerRelationships = new PlayerRelationships();

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var id = reader.ReadUInt32();
            if (id != ID)
            {
                throw new InvalidDataException();
            }

            var numObjects = reader.ReadUInt16();
            for (var i = 0; i < numObjects; i++)
            {
                ObjectIds.Add(reader.ReadUInt32());
            }

            var unknown7 = reader.ReadUInt16();

            var unknown8 = reader.ReadUInt32();
            if (unknown8 != 0 && unknown8 != 1)
            {
                throw new InvalidDataException();
            }

            var unknown9 = reader.ReadUInt32();
            if (unknown9 != 0)
            {
                throw new InvalidDataException();
            }

            var unknown10 = reader.ReadUInt32();
            if (unknown10 != 0)
            {
                throw new InvalidDataException();
            }

            var unknown11 = reader.ReadUInt32();
            if (unknown11 != 0)
            {
                throw new InvalidDataException();
            }

            var unknown12 = reader.ReadUInt16();
            if (unknown12 != 0)
            {
                throw new InvalidDataException();
            }

            var unknown13 = reader.ReadUInt32();
            if (unknown13 != 16)
            {
                throw new InvalidDataException();
            }

            for (var i = 0; i < 5; i++)
            {
                var unknown14 = reader.ReadUInt32();
                if (unknown14 != 0)
                {
                    throw new InvalidDataException();
                }
            }

            TeamToTeamRelationships.Load(reader.Inner);
            TeamToPlayerRelationships.Load(reader.Inner);
        }
    }
}
