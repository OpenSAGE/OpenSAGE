using System.Collections.Generic;

namespace OpenSage.Logic
{
    public sealed class Team
    {
        private bool _enteredOrExitedPolygonTrigger;
        private bool _isAlive;
        private uint _numDestroyedSomething;
        private uint _unknown1;
        private uint _waypointId;
        private readonly bool[] _unknownBools = new bool[16];

        public readonly TeamTemplate Template;
        public uint Id { get; internal set; }
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
                throw new InvalidStateException();
            }

            var numObjects = reader.ReadUInt16();
            for (var i = 0; i < numObjects; i++)
            {
                ObjectIds.Add(reader.ReadObjectID());
            }

            reader.SkipUnknownBytes(1);

            _enteredOrExitedPolygonTrigger = reader.ReadBoolean();

            _isAlive = reader.ReadBoolean();

            reader.SkipUnknownBytes(5);

            _numDestroyedSomething = reader.ReadUInt32();

            _unknown1 = reader.ReadUInt32();
            if (_unknown1 != 0 && _unknown1 != ObjectIds.Count)
            {
                throw new InvalidStateException();
            }

            _waypointId = reader.ReadUInt32();

            var unknownCount = reader.ReadUInt16();
            if (unknownCount != _unknownBools.Length)
            {
                throw new InvalidStateException();
            }

            for (var i = 0; i < unknownCount; i++)
            {
                _unknownBools[i] = reader.ReadBoolean();
            }

            reader.SkipUnknownBytes(2);

            TargetObjectID = reader.ReadObjectID();

            TeamToTeamRelationships.Load(reader);
            TeamToPlayerRelationships.Load(reader);
        }
    }
}
