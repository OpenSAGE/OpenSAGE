using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Sav;

namespace OpenSage.Logic
{
    public sealed class Team
    {
        private uint _numDestroyedSomething;

        public readonly TeamTemplate Template;
        public readonly uint Id;
        public List<uint> ObjectIds = new List<uint>();
        public uint TargetObjectID;

        public readonly PlayerRelationships TeamToTeamRelationships = new PlayerRelationships();
        public readonly PlayerRelationships TeamToPlayerRelationships = new PlayerRelationships();

        internal Team(TeamTemplate template, uint id)
        {
            Template = template;
            Id = id;
        }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var id = reader.ReadUInt32();
            if (id != Id)
            {
                //throw new InvalidDataException();
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
