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
            reader.PersistUInt32("Id", ref id);
            if (id != Id)
            {
                throw new InvalidStateException();
            }

            reader.PersistList("ObjectIds", ObjectIds, static (StatePersister persister, ref uint item) =>
            {
                persister.PersistObjectIDValue(ref item);
            });

            reader.SkipUnknownBytes(1);

            reader.PersistBoolean("EnteredOrExitedPolygonTrigger", ref _enteredOrExitedPolygonTrigger);
            reader.PersistBoolean("IsAlive", ref _isAlive);

            reader.SkipUnknownBytes(5);

            reader.PersistUInt32("NumDestroyedSomething", ref _numDestroyedSomething);

            reader.PersistUInt32("Unknown1", ref _unknown1);
            if (_unknown1 != 0 && _unknown1 != ObjectIds.Count)
            {
                throw new InvalidStateException();
            }

            reader.PersistUInt32("WaypointId", ref _waypointId);

            reader.PersistArrayWithUInt16Length("UnknownBools", _unknownBools, static (StatePersister persister, ref bool item) =>
            {
                persister.PersistBooleanValue(ref item);
            });

            reader.SkipUnknownBytes(2);

            reader.PersistObjectID("TargetObjectId", ref TargetObjectID);
            reader.PersistObject("TeamToTeamRelationships", TeamToTeamRelationships);
            reader.PersistObject("TeamToPlayerRelationships", TeamToPlayerRelationships);
        }
    }
}
