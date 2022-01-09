using System.Collections.Generic;

namespace OpenSage.Logic
{
    public sealed class Team : IPersistableObject
    {
        private bool _enteredOrExitedPolygonTrigger;
        private bool _isAlive;
        private uint _numDestroyedSomething;
        private uint _unknown1;
        private uint _waypointId;
        private readonly bool[] _unknownBools = new bool[16];

        public readonly TeamTemplate Template;
        public readonly uint Id;
        // TODO: Store actual objects here, not just IDs.
        public readonly List<uint> ObjectIds = new List<uint>();
        public uint TargetObjectID;

        public readonly PlayerRelationships TeamToTeamRelationships = new PlayerRelationships();
        public readonly PlayerRelationships TeamToPlayerRelationships = new PlayerRelationships();

        internal Team(TeamTemplate template, uint id)
        {
            Template = template;
            Id = id;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            var id = Id;
            reader.PersistUInt32(ref id);
            if (id != Id)
            {
                throw new InvalidStateException();
            }

            reader.PersistList(
                ObjectIds,
                static (StatePersister persister, ref uint item) =>
                {
                    persister.PersistObjectIDValue(ref item);
                });

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

            reader.PersistArrayWithUInt16Length(
                _unknownBools,
                static (StatePersister persister, ref bool item) =>
                {
                    persister.PersistBooleanValue(ref item);
                });

            reader.SkipUnknownBytes(2);

            reader.PersistObjectID(ref TargetObjectID);
            reader.PersistObject(TeamToTeamRelationships);
            reader.PersistObject(TeamToPlayerRelationships);
        }
    }
}
