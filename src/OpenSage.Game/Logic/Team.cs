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

        internal void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            var id = Id;
            reader.PersistUInt32(ref id);
            if (id != Id)
            {
                throw new InvalidStateException();
            }

            var numObjects = (ushort) ObjectIds.Count;
            reader.PersistUInt16(ref numObjects);

            for (var i = 0; i < numObjects; i++)
            {
                uint objectId = 0;
                reader.PersistObjectID(ref objectId);
                ObjectIds.Add(objectId);
            }

            reader.SkipUnknownBytes(1);

            reader.PersistBoolean(ref _enteredOrExitedPolygonTrigger);
            reader.PersistBoolean(ref _isAlive);

            reader.SkipUnknownBytes(5);

            reader.PersistUInt32(ref _numDestroyedSomething);

            reader.PersistUInt32(ref _unknown1);
            if (_unknown1 != 0 && _unknown1 != ObjectIds.Count)
            {
                throw new InvalidStateException();
            }

            reader.PersistUInt32(ref _waypointId);

            var unknownCount = (ushort) _unknownBools.Length;
            reader.PersistUInt16(ref unknownCount);

            if (unknownCount != _unknownBools.Length)
            {
                throw new InvalidStateException();
            }

            for (var i = 0; i < unknownCount; i++)
            {
                reader.PersistBoolean(ref _unknownBools[i]);
            }

            reader.SkipUnknownBytes(2);

            reader.PersistObjectID(ref TargetObjectID);

            TeamToTeamRelationships.Load(reader);
            TeamToPlayerRelationships.Load(reader);
        }
    }
}
