using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Sav;
using OpenSage.Logic;

namespace OpenSage.Scripting
{
    public delegate bool ScriptingCondition(ScriptExecutionContext context, ScriptCondition condition);
    public delegate void ScriptingAction(ScriptExecutionContext context, ScriptAction action);

    public sealed class ScriptingSystem : GameSystem
    {
        public static NLog.Logger Logger => NLog.LogManager.GetCurrentClassLogger();

        // How many updates are performed per second?
        public readonly uint TickRate;

        private readonly ScriptExecutionContext _executionContext;

        internal CameraFadeOverlay CameraFadeOverlay;

        public Dictionary<string, bool> Flags { get; }
        public CounterCollection Counters { get; }
        public TimerCollection Timers { get; }

        public bool Active { get; set; }

        public event EventHandler<ScriptingSystem> OnUpdateFinished; 

        public ScriptingSystem(Game game)
            : base(game)
        {
            Flags = new Dictionary<string, bool>();
            Counters = new CounterCollection();
            Timers = new TimerCollection(Counters);

            CameraFadeOverlay = AddDisposable(new CameraFadeOverlay(game));

            _executionContext = new ScriptExecutionContext(game);

            TickRate = game.Definition.ScriptingTicksPerSecond;
        }

        internal override void OnSceneChanging()
        {
            Flags.Clear();
            Counters.Clear();
            Timers.Clear();
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

            Timers.Update();

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

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(5);

            var numSequentialScripts = reader.ReadUInt16();
            for (var i = 0; i < numSequentialScripts; i++)
            {
                var sequentialScript = new SequentialScript();
                sequentialScript.Load(reader);
            }

            var numTimersAndCounters = reader.ReadUInt32();

            var unknown2 = reader.ReadUInt32();
            if (unknown2 != 0)
            {
                throw new InvalidDataException();
            }

            for (var i = 1; i < numTimersAndCounters; i++)
            {
                var value = reader.ReadInt32();
                var name = reader.ReadAsciiString();
                var active = reader.ReadBoolean();
            }

            var numTimersAndCounters2 = reader.ReadUInt32();
            if (numTimersAndCounters2 != numTimersAndCounters)
            {
                throw new InvalidDataException();
            }

            var numFlags = reader.ReadUInt32();

            for (var i = 1; i < numFlags; i++)
            {
                var value = reader.ReadBoolean();
                var name = reader.ReadAsciiString();
            }

            var numFlags2 = reader.ReadUInt32();
            if (numFlags2 != numFlags)
            {
                throw new InvalidDataException();
            }

            var numAttackPrioritySets = reader.ReadUInt16();

            reader.__Skip(8); // TODO

            for (var i = 1; i < numAttackPrioritySets; i++)
            {
                var attackPriority = new AttackPriority();
                attackPriority.Load(reader);
            }

            var numAttackPrioritySets2 = reader.ReadUInt32();
            if (numAttackPrioritySets2 != numAttackPrioritySets)
            {
                throw new InvalidDataException();
            }

            var unknown7 = reader.ReadInt32();
            if (unknown7 != -1)
            {
                throw new InvalidDataException();
            }

            var unknown8 = reader.ReadInt32();
            if (unknown8 != -1)
            {
                throw new InvalidDataException();
            }

            var unknownCount = reader.ReadUInt16();
            for (var i = 0; i < unknownCount; i++)
            {
                var objectName = reader.ReadAsciiString();
                var someId = reader.ReadUInt32();
            }

            var unknown9 = reader.ReadByte();
            if (unknown9 != 0)
            {
                throw new InvalidDataException();
            }

            CameraFadeOverlay.Load(reader);

            reader.__Skip(12);

            var numSpecialPowerSets = reader.ReadUInt16(); // Maybe not sides, maybe player count?
            for (var i = 0; i < numSpecialPowerSets; i++)
            {
                var version = reader.ReadByte();

                var numSpecialPowers = reader.ReadUInt16();
                for (var j = 0; j < numSpecialPowers; j++)
                {
                    var name = reader.ReadAsciiString();
                    var timestamp = reader.ReadUInt32();
                }
            }

            var numUnknown1Sets = reader.ReadUInt16();
            for (var i = 0; i < numUnknown1Sets; i++)
            {
                var version = reader.ReadByte();

                var count = reader.ReadUInt16();
                if (count != 0)
                {
                    throw new InvalidDataException();
                }
            }

            var numUnknown2Sets = reader.ReadUInt16();
            for (var i = 0; i < numUnknown2Sets; i++)
            {
                var version = reader.ReadByte();

                var count = reader.ReadUInt16();
                if (count != 0)
                {
                    throw new InvalidDataException();
                }
            }

            var numUpgradeSets = reader.ReadUInt16();
            for (var i = 0; i < numUpgradeSets; i++)
            {
                var version = reader.ReadByte();

                var numUpgrades = reader.ReadUInt16();
                for (var j = 0; j < numUpgrades; j++)
                {
                    var name = reader.ReadAsciiString();
                    var timestamp = reader.ReadUInt32();
                }
            }

            var numScienceSets = reader.ReadUInt16();
            for (var i = 0; i < numScienceSets; i++)
            {
                var version = reader.ReadByte();

                var numSciences = reader.ReadUInt16();
                for (var j = 0; j < numSciences; j++)
                {
                    var name = reader.ReadAsciiString();
                }
            }

            var unknown14_1 = reader.ReadByte();
            if (unknown14_1 != 1)
            {
                throw new InvalidDataException();
            }

            var unknown14_2 = reader.ReadUInt16();
            if (unknown14_2 != 0)
            {
                throw new InvalidDataException();
            }

            for (var i = 0; i < 6; i++)
            {
                var unknown15 = reader.ReadSingle();
            }

            var unknown16 = reader.ReadUInt32();
            if (unknown16 != 150)
            {
                throw new InvalidDataException();
            }

            var unknown17 = reader.ReadUInt32();
            if (unknown17 != 0 && unknown17 != 1 && unknown17 != 2)
            {
                throw new InvalidDataException();
            }

            var unknown18 = reader.ReadByte();
            if (unknown18 != 0)
            {
                throw new InvalidDataException();
            }

            var numMapReveals = reader.ReadUInt16();
            for (var i = 0; i < numMapReveals; i++)
            {
                var revealName = reader.ReadAsciiString();
                var waypoint = reader.ReadAsciiString();
                var radius = reader.ReadSingle();
                var player = reader.ReadAsciiString();
            }

            var numObjectTypeLists = reader.ReadUInt16();
            for (var i = 0; i < numObjectTypeLists; i++)
            {
                var objectTypeList = new ObjectTypeList();
                objectTypeList.Load(reader);
            }

            var unknown20 = reader.ReadByte();
            if (unknown20 != 1)
            {
                throw new InvalidDataException();
            }

            var musicTrack = reader.ReadAsciiString();

            var unknown21 = reader.ReadByte();
            if (unknown21 != 0)
            {
                throw new InvalidDataException();
            }
        }
    }
}
