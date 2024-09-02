#nullable enable

using System;
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
        public ReadOnlySpan<RadarEvent> RadarEvents => _radarEvents;

        private readonly RadarItemCollection _visibleItems = [];
        private readonly RadarItemCollection _hiddenItems = [];

        // in generals, this is fixed at 64 items and acts as a circular buffer
        private const int RadarEventsBufferSize = 64;
        private readonly RadarEvent[] _radarEvents = new RadarEvent[RadarEventsBufferSize];

        private bool _unknown1;
        private uint _nextRadarEventIndex;
        private uint _lastRadarEventIndex = uint.MaxValue; // at max value, there is no last radar event

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
            // this only pertains to fog of war - if a player has no radar at all, items still appear here
            var isVisibleToLocalPlayer = true;

            var items = isVisibleToLocalPlayer
                ? _visibleItems
                : _hiddenItems;

            items.Add(new RadarItem(gameObject.ID, gameObject.Owner.Color.ToColorRgba()));
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            var objectId = gameObject.ID;

            _visibleItems.Remove(objectId);
            _hiddenItems.Remove(objectId);
        }

        private const uint EventFrames = 120; // all radar events seem to play for 120 frames in generals
        private const uint FramesUntilFade = EventFrames - 15; // and fade out for the last 15 frames

        public void AddRadarEvent(RadarEventType eventType, in Vector3 position, in LogicFrame now, uint mapTileXCoordinate, uint mapTileYCoordinate)
        {
            var (color1, color2) = RadarEvent.DefaultColors[eventType];

            var radarEvent = new RadarEvent(eventType, true, now, now + new LogicFrameSpan(EventFrames),
                now + new LogicFrameSpan(FramesUntilFade), color1, color2, in position,
                mapTileXCoordinate, mapTileYCoordinate, false);

            _radarEvents[_nextRadarEventIndex] = radarEvent;

            _lastRadarEventIndex = _nextRadarEventIndex;
            _nextRadarEventIndex = (_nextRadarEventIndex + 1) % RadarEventsBufferSize;
        }

        public bool TryGetLastEventLocation(out Vector3 location)
        {
            location = default;

            if (_lastRadarEventIndex < uint.MaxValue)
            {
                location = _radarEvents[_lastRadarEventIndex].Position;
                return true;
            }

            return false;
        }

        public void StopAnimationForEvent(int index)
        {
            _radarEvents[index] = _radarEvents[index] with { ActivelyAnimating = false };
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
                static (StatePersister persister, ref RadarEvent item) =>
                {
                    persister.PersistObjectValue(ref item);
                });

            if (_radarEvents.Length != RadarEventsBufferSize)
            {
                throw new InvalidStateException();
            }

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

    public class RadarItem : IPersistableObject
    {
        public uint ObjectId => _objectId;
        public ColorRgba Color => _color;

        private uint _objectId;
        private ColorRgba _color;

        public RadarItem() : this(default, default)
        {
        }

        public RadarItem(uint objectId, ColorRgba color)
        {
            _objectId = objectId;
            _color = color;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistObjectID(ref _objectId);
            reader.PersistColorBgra(ref _color);
        }
    }

    public struct RadarEvent : IPersistableObject
    {
        public RadarEventType Type => _type;

        public bool ActivelyAnimating
        {
            get => _activelyAnimating;
            set => _activelyAnimating = value;
        }

        public LogicFrame StartFrame => _startFrame;
        public LogicFrame EndFrame => _endFrame;
        public LogicFrame FadeBeginFrame => _fadeBeginFrame;
        public ColorRgba Color1 => _color1;
        public ColorRgba Color2 => _color2;
        public Vector3 Position => _position;
        public uint MapTileXCoordinate => _mapTileXCoordinate;
        public uint MapTileYCoordinate => _mapTileYCoordinate;

        // todo: it's not entirely clear when this field should be set, but can change from false to true if radar is upgraded while the event is still playing (but not after)
        public bool Visible
        {
            get => _visible;
            set => _visible = value;
        }

        internal static FrozenDictionary<RadarEventType, RadarEventColors> DefaultColors { get; } =
            new Dictionary<RadarEventType, RadarEventColors>
            {
                [RadarEventType.Construction] = new(new ColorRgba(128, 128, 255, 255), new ColorRgba(128, 255, 255, 255)),
                [RadarEventType.Upgrade] = new(new ColorRgba(128, 0, 64, 255), new ColorRgba(255, 185, 220, 255)),
                [RadarEventType.UnderAttack] = new(new ColorRgba(255, 0, 0, 255), new ColorRgba(255, 128, 128, 255)),
                [RadarEventType.EnemyInfiltrationDetected] = new(new ColorRgba(0, 255, 255, 255), new ColorRgba(128, 255, 255, 255)),
                [RadarEventType.BattlePlanInitiated] = new(new ColorRgba(255, 0, 0, 255), new ColorRgba(64, 0, 0, 255)),
                [RadarEventType.StealthUnitDiscovered] = new(new ColorRgba(0, 255, 0, 255), new ColorRgba(0, 128, 0, 255)),
                [RadarEventType.StealthUnitNeutralized] = new(new ColorRgba(0, 255, 0, 255), new ColorRgba(0, 128, 0, 255)),
                [RadarEventType.UnitLost] = new(new ColorRgba(0, 0, 0, 0), new ColorRgba(0, 0, 0, 0)), // this technically plays, it's just invisible
            }.ToFrozenDictionary();

        private RadarEventType _type;
        private bool _activelyAnimating;
        private LogicFrame _startFrame;
        private LogicFrame _endFrame;
        private LogicFrame _fadeBeginFrame;
        // these colors form a gradient along the edges of the triangle that appears on the radar for a given event
        private ColorRgba _color1;
        private ColorRgba _color2;
        private Vector3 _position;
        private uint _mapTileXCoordinate;
        private uint _mapTileYCoordinate;
        private bool _visible; // false when the user doesn't have any radar, true when they do have radar?

        internal RadarEvent(RadarEventType type, bool activelyAnimating, in LogicFrame startFrame, in LogicFrame endFrame,
            in LogicFrame fadeBeginFrame, in ColorRgba color1, in ColorRgba color2, in Vector3 position,
            uint mapTileXCoordinate, uint mapTileYCoordinate, bool visible)
        {
            _type = type;
            _activelyAnimating = activelyAnimating;
            _startFrame = startFrame;
            _endFrame = endFrame;
            _fadeBeginFrame = fadeBeginFrame;
            _color1 = color1;
            _color2 = color2;
            _position = position;
            _mapTileXCoordinate = mapTileXCoordinate;
            _mapTileYCoordinate = mapTileYCoordinate;
            _visible = visible;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistEnum(ref _type);
            reader.PersistBoolean(ref _activelyAnimating);
            reader.PersistLogicFrame(ref _startFrame);
            reader.PersistLogicFrame(ref _endFrame);
            reader.PersistLogicFrame(ref _fadeBeginFrame);
            reader.PersistColorRgbaInt(ref _color1);
            reader.PersistColorRgbaInt(ref _color2);
            reader.PersistVector3(ref _position);
            reader.PersistUInt32(ref _mapTileXCoordinate);
            reader.PersistUInt32(ref _mapTileYCoordinate);
            reader.PersistBoolean(ref _visible);
        }
    }

    public enum RadarEventType
    {
        Invalid = 0,
        Construction = 1,
        Upgrade = 2,
        UnderAttack = 3,
        Information = 4,
        EnemyInfiltrationDetected = 6, // building captured
        BattlePlanInitiated = 7, // strategy center battle plan changed
        StealthUnitDiscovered = 8, // discovered enemy stealth unit
        StealthUnitNeutralized = 9, // your stealth unit has been discovered
        UnitLost = 10,
    }

    internal readonly record struct RadarEventColors(
        ColorRgba Color1,
        ColorRgba Color2
    );
}
