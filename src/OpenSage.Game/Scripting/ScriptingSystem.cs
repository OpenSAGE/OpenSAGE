using System;
using System.Collections.Generic;
using System.Text;
using OpenSage.Logic;

namespace OpenSage.Scripting
{
    public delegate bool ScriptingCondition(ScriptExecutionContext context, ScriptCondition condition);
    public delegate void ScriptingAction(ScriptExecutionContext context, ScriptAction action);

    public sealed class ScriptingSystem : GameSystem
    {
        private readonly List<SequentialScript> _sequentialScripts = new();

        public static NLog.Logger Logger => NLog.LogManager.GetCurrentClassLogger();

        // How many updates are performed per second?
        public readonly uint TickRate;

        private readonly ScriptExecutionContext _executionContext;

        internal CameraFadeOverlay CameraFadeOverlay;

        private readonly ScriptingFlag[] _flags = new ScriptingFlag[128];
        private ushort _numFlags;

        private readonly ScriptingCounter[] _counters = new ScriptingCounter[128];
        private ushort _numCounters;

        private readonly List<AttackPriority> _attackPriorities = new();
        private readonly List<ObjectNameAndId> _unknownSomethings = new();
        private readonly List<ObjectNameAndId>[] _specialPowers;
        private readonly List<ObjectNameAndId>[] _upgrades;
        private readonly ScienceSet[] _sciences;
        private readonly float[] _unknownFloats = new float[6];
        private uint _unknown17;
        private readonly List<MapReveal> _mapReveals = new();
        private readonly List<ObjectTypeList> _objectTypeLists = new();
        private string _musicTrackName;

        public bool Active { get; set; }

        public event EventHandler<ScriptingSystem> OnUpdateFinished; 

        public ScriptingSystem(Game game)
            : base(game)
        {
            CameraFadeOverlay = AddDisposable(new CameraFadeOverlay(game));

            _executionContext = new ScriptExecutionContext(game);

            TickRate = game.Definition.ScriptingTicksPerSecond;

            _specialPowers = new List<ObjectNameAndId>[Player.MaxPlayers];
            for (var i = 0; i < _specialPowers.Length; i++)
            {
                _specialPowers[i] = new List<ObjectNameAndId>();
            }

            _upgrades = new List<ObjectNameAndId>[Player.MaxPlayers];
            for (var i = 0; i < _upgrades.Length; i++)
            {
                _upgrades[i] = new List<ObjectNameAndId>();
            }

            _sciences = new ScienceSet[Player.MaxPlayers];
            for (var i = 0; i < _sciences.Length; i++)
            {
                _sciences[i] = new ScienceSet(game.AssetStore);
            }
        }

        internal override void OnSceneChanging()
        {
            _numFlags = 1;
            _numCounters = 1;
        }

        internal override void OnSceneChanged()
        {
            
        }

        public Script FindScript(string name)
        {
            if (Game.Scene3D?.PlayerScripts?.ScriptLists == null)
            {
                return null;
            }

            for (var i = 0; i < Game.Scene3D.PlayerScripts.ScriptLists.Length; i++)
            {
                var script = Game.Scene3D.PlayerScripts.ScriptLists[i].FindScript(name);
                if (script != null)
                {
                   return script;
                }
            }

            return null;
        }

        public bool GetFlagValue(string name)
        {
            ref var flag = ref GetFlag(name);

            return flag.Value;
        }

        public void SetFlagValue(string name, bool value)
        {
            ref var flag = ref GetFlag(name);

            flag.Value = value;
        }

        private ref ScriptingFlag GetFlag(string name)
        {
            for (var i = 0; i < _flags.Length; i++)
            {
                if (_flags[i].Name == name)
                {
                    return ref _flags[i];
                }
            }

            if (_numFlags == _flags.Length)
            {
                throw new InvalidOperationException();
            }

            ref var result = ref _flags[_numFlags];

            result.Name = name;
            result.Value = false;

            _numFlags++;

            return ref result;
        }

        public int GetCounterValue(string name)
        {
            ref var counter = ref GetCounter(name);

            return counter.Value;
        }

        public bool HasTimerExpired(string name)
        {
            ref var counter = ref GetCounter(name);

            if (!counter.IsTimer)
            {
                return false;
            }

            return counter.Value == -1;
        }

        public void SetCounterValue(string name, int value)
        {
            ref var counter = ref GetCounter(name);

            counter.Value = value;
        }

        public void AddCounterValue(string name, int valueToAdd)
        {
            ref var counter = ref GetCounter(name);

            counter.Value += valueToAdd;
        }

        public void SubtractCounterValue(string name, int valueToSubtract)
        {
            ref var counter = ref GetCounter(name);

            counter.Value -= valueToSubtract;
        }

        public void SetTimerValue(string name, int value)
        {
            ref var counter = ref GetCounter(name);

            counter.IsTimer = true;
            counter.Value = value;
        }

        private ref ScriptingCounter GetCounter(string name)
        {
            for (var i = 0; i < _counters.Length; i++)
            {
                if (_counters[i].Name == name)
                {
                    return ref _counters[i];
                }
            }

            if (_numCounters == _counters.Length)
            {
                throw new InvalidOperationException();
            }

            ref var result = ref _counters[_numCounters];

            result.Name = name;
            result.Value = 0;
            result.IsTimer = false;

            _numCounters++;

            return ref result;
        }

        public void ScriptingTick()
        {
            if (Game.Scene3D?.PlayerScripts?.ScriptLists == null)
            {
                return;
            }

            if (!Active)
            {
                return;
            }

            foreach (var playerScripts in Game.Scene3D.PlayerScripts.ScriptLists)
            {
                playerScripts?.Execute(_executionContext);
            }

            OnUpdateFinished?.Invoke(this, this);

            for (var i = 0; i < _numCounters; i++)
            {
                ref var counter = ref _counters[i];

                if (counter.IsTimer && counter.Value > -1)
                {
                    counter.Value -= 1;
                }
            }

            UpdateCameraFadeOverlay();
        }

        // For unit tests.
        internal bool EvaluateScriptCondition(ScriptCondition condition)
        {
            return ScriptConditions.Evaluate(_executionContext, condition);
        }

        internal void SetCameraFade(CameraFadeType type, float from, float to, uint framesIncrease, uint framesHold, uint framesDecrease)
        {
            CameraFadeOverlay.FadeType = type;
            CameraFadeOverlay.From = from;
            CameraFadeOverlay.To = to;
            CameraFadeOverlay.FramesIncrease = framesIncrease;
            CameraFadeOverlay.FramesHold = framesHold;
            CameraFadeOverlay.FramesDecrease = framesDecrease;

            CameraFadeOverlay.CurrentFrame = 0;
        }

        private void UpdateCameraFadeOverlay()
        {
            CameraFadeOverlay.Update();
        }

        internal void Load(StatePersister reader)
        {
            reader.PersistVersion(5);

            var numSequentialScripts = (ushort) _sequentialScripts.Count;
            reader.PersistUInt16(ref numSequentialScripts);

            for (var i = 0; i < numSequentialScripts; i++)
            {
                var sequentialScript = new SequentialScript();
                sequentialScript.Load(reader);
                _sequentialScripts.Add(sequentialScript);
            }

            reader.PersistUInt16(ref _numCounters);

            for (var i = 0; i < _numCounters; i++)
            {
                ref var counter = ref _counters[i];

                reader.PersistInt32(ref counter.Value);
                reader.PersistAsciiString(ref counter.Name);
                reader.PersistBoolean("IsTimer", ref counter.IsTimer);
            }

            var numTimersAndCounters2 = (uint)_numCounters;
            reader.PersistUInt32(ref numTimersAndCounters2);
            if (numTimersAndCounters2 != _numCounters)
            {
                throw new InvalidStateException();
            }

            reader.PersistUInt16(ref _numFlags);

            for (var i = 0; i < _numFlags; i++)
            {
                ref var flag = ref _flags[i];

                reader.PersistBoolean("Value", ref flag.Value);
                reader.PersistAsciiString(ref flag.Name);
            }

            var numFlags2 = (uint)_numFlags;
            reader.PersistUInt32(ref numFlags2);
            if (numFlags2 != _numFlags)
            {
                throw new InvalidStateException();
            }

            var numAttackPrioritySets = (ushort) _attackPriorities.Count;
            reader.PersistUInt16(ref numAttackPrioritySets);

            for (var i = 0; i < numAttackPrioritySets; i++)
            {
                var attackPriority = new AttackPriority();
                attackPriority.Load(reader);
                _attackPriorities.Add(attackPriority);
            }

            var numAttackPrioritySets2 = (uint)numAttackPrioritySets;
            reader.PersistUInt32(ref numAttackPrioritySets2);
            if (numAttackPrioritySets2 != numAttackPrioritySets)
            {
                throw new InvalidStateException();
            }

            var unknown1 = -1;
            reader.PersistInt32(ref unknown1);
            if (unknown1 != -1)
            {
                throw new InvalidStateException();
            }

            var unknown2 = -1;
            reader.PersistInt32(ref unknown2);
            if (unknown2 != -1)
            {
                throw new InvalidStateException();
            }

            var unknownCount = (ushort) _unknownSomethings.Count;
            reader.PersistUInt16(ref unknownCount);

            for (var i = 0; i < unknownCount; i++)
            {
                var objectNameAndId = new ObjectNameAndId();

                reader.PersistAsciiString(ref objectNameAndId.Name);
                reader.PersistObjectID(ref objectNameAndId.ObjectId);

                _unknownSomethings.Add(objectNameAndId);
            }

            reader.SkipUnknownBytes(1);

            CameraFadeOverlay.Load(reader);

            for (var i = 0; i < 4; i++)
            {
                reader.PersistVersion(1);

                reader.SkipUnknownBytes(2);
            }

            var numSpecialPowerSets = (ushort) _specialPowers.Length;
            reader.PersistUInt16(ref numSpecialPowerSets);

            if (numSpecialPowerSets != _specialPowers.Length)
            {
                throw new InvalidStateException();
            }

            for (var i = 0; i < numSpecialPowerSets; i++)
            {
                reader.PersistObjectNameAndIdList(_specialPowers[i]);
            }

            ushort numUnknown1Sets = 0;
            reader.PersistUInt16(ref numUnknown1Sets);

            for (var i = 0; i < numUnknown1Sets; i++)
            {
                reader.PersistVersion(1);

                reader.SkipUnknownBytes(2);
            }

            ushort numUnknown2Sets = 0;
            reader.PersistUInt16(ref numUnknown2Sets);

            for (var i = 0; i < numUnknown2Sets; i++)
            {
                reader.PersistVersion(1);

                reader.SkipUnknownBytes(2);
            }

            var numUpgradeSets = (ushort)_upgrades.Length;
            reader.PersistUInt16(ref numUpgradeSets);

            if (numUpgradeSets != _upgrades.Length)
            {
                throw new InvalidStateException();
            }

            for (var i = 0; i < numUpgradeSets; i++)
            {
                reader.PersistObjectNameAndIdList(_upgrades[i]);
            }

            var numScienceSets = (ushort) _sciences.Length;
            reader.PersistUInt16(ref numScienceSets);

            if (numScienceSets != _sciences.Length)
            {
                throw new InvalidStateException();
            }

            for (var i = 0; i < numScienceSets; i++)
            {
                _sciences[i].Load(reader);
            }

            byte unknown14_1 = 1;
            reader.PersistByte("Unknown14_1", ref unknown14_1);
            if (unknown14_1 != 1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(2);

            for (var i = 0; i < 6; i++)
            {
                reader.PersistSingle(ref _unknownFloats[i]);
            }

            var unknown16 = 150u;
            reader.PersistUInt32(ref unknown16);
            if (unknown16 != 150)
            {
                throw new InvalidStateException();
            }

            reader.PersistUInt32(ref _unknown17);
            if (_unknown17 != 0 && _unknown17 != 1 && _unknown17 != 2)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(1);

            var numMapReveals = (ushort) _mapReveals.Count;
            reader.PersistUInt16(ref numMapReveals);

            for (var i = 0; i < numMapReveals; i++)
            {
                var mapReveal = new MapReveal();

                reader.PersistAsciiString(ref mapReveal.Name);
                reader.PersistAsciiString(ref mapReveal.Waypoint);
                reader.PersistSingle(ref mapReveal.Radius);
                reader.PersistAsciiString(ref mapReveal.Player);

                _mapReveals.Add(mapReveal);
            }

            var numObjectTypeLists = (ushort) _objectTypeLists.Count;
            reader.PersistUInt16(ref numObjectTypeLists);

            for (var i = 0; i < numObjectTypeLists; i++)
            {
                var objectTypeList = new ObjectTypeList();
                objectTypeList.Load(reader);
                _objectTypeLists.Add(objectTypeList);
            }

            byte unknown20 = 1;
            reader.PersistByte("Unknown20", ref unknown20);
            if (unknown20 != 1)
            {
                throw new InvalidStateException();
            }

            reader.PersistAsciiString(ref _musicTrackName);

            reader.SkipUnknownBytes(1);
        }

        internal void Dump(StringBuilder sb)
        {
            sb.AppendLine("Counters:");

            foreach (var kv in _counters)
            {
                sb.AppendFormat("  {0}: {1} (IsTimer: {2})\n", kv.Name, kv.Value, kv.IsTimer);
            }

            sb.AppendLine("Flags:");

            foreach (var kv in _flags)
            {
                sb.AppendFormat("  {0}: {1}\n", kv.Name, kv.Value);
            }
        }
    }

    internal struct ScriptingFlag
    {
        public bool Value;
        public string Name;
    }

    internal struct ScriptingCounter
    {
        public int Value;
        public string Name;
        public bool IsTimer;
    }

    internal struct MapReveal
    {
        public string Name;
        public string Waypoint;
        public float Radius;
        public string Player;
    }
}
