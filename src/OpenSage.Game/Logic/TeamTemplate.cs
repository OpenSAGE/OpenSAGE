using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic
{
    [DebuggerDisplay("TeamTemplate '{Name}'")]
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
        private uint _numDestroyedSomething;

        public uint ID;
        public List<uint> ObjectIds = new List<uint>();
        public uint TargetObjectID;

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
                ObjectIds.Add(reader.ReadObjectID());
            }

            for (var i = 0; i < 2; i++)
            {
                var unknown1 = reader.ReadBoolean();
                if (unknown1 != false)
                {
                    throw new InvalidDataException();
                }
            }

            var unknown2 = reader.ReadBoolean();
            if (unknown2 != true)
            {
                //throw new InvalidDataException();
            }

            for (var i = 0; i < 5; i++)
            {
                var unknown1 = reader.ReadBoolean();
                if (unknown1 != false)
                {
                    throw new InvalidDataException();
                }
            }

            _numDestroyedSomething = reader.ReadUInt32();

            var unknown11 = reader.ReadUInt32();
            if (unknown11 != 0 && unknown11 != ObjectIds.Count)
            {
                throw new InvalidDataException();
            }

            var waypointID = reader.ReadUInt32();

            var unknown13 = reader.ReadUInt16();
            if (unknown13 != 16)
            {
                throw new InvalidDataException();
            }

            for (var i = 0; i < unknown13; i++)
            {
                var unknown19 = reader.ReadBoolean();
                if (unknown19 != false)
                {
                    //throw new InvalidDataException();
                }
            }

            var unknown14 = reader.ReadBoolean();
            if (unknown14 != false)
            {
                throw new InvalidDataException();
            }

            var unknown15 = reader.ReadBoolean();
            if (unknown15 != false)
            {
                throw new InvalidDataException();
            }

            TargetObjectID = reader.ReadObjectID();

            TeamToTeamRelationships.Load(reader);
            TeamToPlayerRelationships.Load(reader);
        }
    }
}
