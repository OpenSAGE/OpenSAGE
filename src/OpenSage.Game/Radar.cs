#nullable enable

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage
{
    public sealed class Radar : IPersistableObject
    {
        public RadarItemCollection VisibleItems => _visibleItems;
        public RadarItemCollection HiddenItems => _hiddenItems;

        // fixed-length array
        public IReadOnlyList<RadarEvent> RadarEvents => _radarEvents;

        private readonly RadarItemCollection _visibleItems = [];
        private readonly RadarItemCollection _hiddenItems = [];

        // in generals, this is fixed at 64 items
        // todo: check if these should be structs with default values?
        private readonly RadarEvent[] _radarEvents = new RadarEvent[64];

        private bool _unknown1;
        private uint _nextRadarEventIndex; // todo: test at start (0 events) and end (64 events)
        private uint _lastRadarEventIndex;

        internal Radar()
        {
            // TODO: Bridges
            // TODO: Fog of war / shroud
        }

        // TODO: Update item color when it changes owner - or remove/add.
        public void AddGameObject(GameObject gameObject)
        {
            switch (gameObject.Definition.RadarPriority)
            {
                case RadarPriority.Invalid:
                case RadarPriority.NotOnRadar:
                    return;
            }

            // TODO: Check whether this object is visible to the local player.
            var isVisibleToLocalPlayer = true;

            var items = isVisibleToLocalPlayer
                ? _visibleItems
                : _hiddenItems;

            items.Add(new RadarItem
            {
                ObjectId = gameObject.ID,
                Color = gameObject.Owner.Color.ToColorRgba()
            });
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            var objectId = gameObject.ID;

            _visibleItems.Remove(objectId);
            _hiddenItems.Remove(objectId);
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.SkipUnknownBytes(1);

            reader.PersistBoolean(ref _unknown1);
            reader.PersistObject(_visibleItems);
            reader.PersistObject(_hiddenItems);

            reader.PersistArrayWithUInt16Length(
                _radarEvents,
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                static (StatePersister persister, ref RadarEvent? item) =>
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
                {
                    item ??= new RadarEvent();
                    persister.PersistObjectValue(item);
                });

            reader.PersistUInt32(ref _nextRadarEventIndex);
            reader.PersistUInt32(ref _lastRadarEventIndex);
        }
    }

    public sealed class RadarItemCollection : KeyedCollection<uint, RadarItem>, IPersistableObject
    {
        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            var count = (ushort) Count;
            reader.PersistUInt16(ref count);

            reader.BeginArray("Items");
            if (reader.Mode == StatePersistMode.Read)
            {
                Clear();

                for (var i = 0; i < count; i++)
                {
                    var item = new RadarItem();
                    reader.PersistObjectValue(item);
                    Add(item);
                }
            }
            else
            {
                foreach (var item in this)
                {
                    reader.PersistObjectValue(item);
                }
            }
            reader.EndArray();
        }

        protected override uint GetKeyForItem(RadarItem item) => item.ObjectId;
    }

    public sealed class RadarItem : IPersistableObject
    {
        public uint ObjectId;
        public ColorRgba Color;

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistObjectID(ref ObjectId);
            reader.PersistColorRgba(ref Color);
        }
    }

    public sealed class RadarEvent : IPersistableObject
    {
        public RadarEventType Type;
        public Vector3 Position;

        private bool _unknown1;
        private uint _unknown2;
        private uint _unknown3;
        private uint _unknown4;
        private ColorRgba _color1;
        private ColorRgba _color2;
        private uint _unknown5;
        private uint _unknown6;
        private bool _unknown7;

        public void Persist(StatePersister reader)
        {
            reader.PersistEnum(ref Type);
            reader.PersistBoolean(ref _unknown1);
            reader.PersistUInt32(ref _unknown2);
            reader.PersistUInt32(ref _unknown3);
            reader.PersistUInt32(ref _unknown4);
            reader.PersistColorRgbaInt(ref _color1);
            reader.PersistColorRgbaInt(ref _color2);
            reader.PersistVector3(ref Position);
            reader.PersistUInt32(ref _unknown5);
            reader.PersistUInt32(ref _unknown6);
            reader.PersistBoolean(ref _unknown7);
        }
    }

    internal enum RadarEventType
    {
        Invalid = 0,
        Construction = 1,
        Upgrade = 2,
        UnderAttack = 3,
        Information = 4,
        BattlePlanInitiated = 7,
        StealUnitDiscovered = 8,
        UnitLost = 10,
    }
}
