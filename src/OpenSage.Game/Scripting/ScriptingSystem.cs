using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data.Map;
using OpenSage.Data.Scb;
using OpenSage.FileFormats;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
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

        private ScriptList[] _playerScripts;

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
            _playerScripts = Game.Scene3D?.PlayerScripts;
        }

        public Script FindScript(string name)
        {
            if (_playerScripts == null)
            {
                return null;
            }

            for (var i = 0; i < _playerScripts.Length; i++)
            {
                var script = _playerScripts[i].FindScript(name);
                if (script != null)
                {
                   return script;
                }
            }

            return null;
        }

        public void ScriptingTick()
        {
            if (_playerScripts == null)
            {
                return;
            }

            if (!Active)
            {
                return;
            }

            foreach (var playerScripts in _playerScripts)
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

        internal void Load(BinaryReader reader)
        {
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
                var name = reader.ReadBytePrefixedAsciiString();
                var active = reader.ReadBooleanChecked();
            }

            var numTimersAndCounters2 = reader.ReadUInt32();
            if (numTimersAndCounters2 != numTimersAndCounters)
            {
                throw new InvalidDataException();
            }

            var numFlags = reader.ReadUInt32();

            for (var i = 1; i < numFlags; i++)
            {
                var value = reader.ReadBooleanChecked();
                var name = reader.ReadBytePrefixedAsciiString();
            }

            var numFlags2 = reader.ReadUInt32();
            if (numFlags2 != numFlags)
            {
                throw new InvalidDataException();
            }

            var numAttackPrioritySets = reader.ReadUInt16();

            var unknown3 = reader.ReadBytes(8); // TODO

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
                var objectName = reader.ReadBytePrefixedAsciiString();
                var someId = reader.ReadUInt32();
            }

            var unknown9 = reader.ReadByte();
            if (unknown9 != 0)
            {
                throw new InvalidDataException();
            }

            CameraFadeOverlay.Load(reader);

            var unknown14 = reader.ReadBytes(12);

            var numSpecialPowerSets = reader.ReadUInt16(); // Maybe not sides, maybe player count?
            for (var i = 0; i < numSpecialPowerSets; i++)
            {
                var version = reader.ReadByte();

                var numSpecialPowers = reader.ReadUInt16();
                for (var j = 0; j < numSpecialPowers; j++)
                {
                    var name = reader.ReadBytePrefixedAsciiString();
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
                    var name = reader.ReadBytePrefixedAsciiString();
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
                    var name = reader.ReadBytePrefixedAsciiString();
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
                var revealName = reader.ReadBytePrefixedAsciiString();
                var waypoint = reader.ReadBytePrefixedAsciiString();
                var radius = reader.ReadSingle();
                var player = reader.ReadBytePrefixedAsciiString();
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

            var musicTrack = reader.ReadBytePrefixedAsciiString();

            var unknown21 = reader.ReadByte();
            if (unknown21 != 0)
            {
                throw new InvalidDataException();
            }
        }

        /// <summary>
        /// Initializes the skirmish game.
        /// </summary>
        public void InitializeSkirmishGame()
        {
            var playerNames = Game.Scene3D.MapFile.SidesList.Players.Select(p => p.Properties.GetPropOrNull("playerName")?.Value.ToString()).ToArray();

            _playerScripts = new ScriptList[Game.Scene3D.Players.Count + 1]; // + 1 for neutral player @ index 0
            CopyScripts(Game.Scene3D.PlayerScripts, playerNames, string.Empty, 0, appendIndex: false); // neutral
            CopyScripts(Game.Scene3D.PlayerScripts, playerNames, Game.CivilianPlayer.Name, 1, appendIndex: false); // Civilian

            var skirmishScriptsEntry = Game.ContentManager.GetScriptEntry(@"Data\Scripts\SkirmishScripts.scb");
            if (skirmishScriptsEntry != null)
            {
                using (var stream = skirmishScriptsEntry.Open())
                {
                    var skirmishScripts = ScbFile.FromStream(stream);

                    // skip civilian player
                    for (int i = 1; i < Game.Scene3D.Players.Count; i++)
                    {
                        var player = Game.Scene3D.Players[i];

                        if (player.IsHuman)
                        {
                            // copy the scripts from the civilian player to all human players
                            CopyScripts(skirmishScripts.PlayerScripts.ScriptLists, skirmishScripts.Players.PlayerNames, Game.CivilianPlayer.Name, i + 1, appendIndex: true);
                        }
                        else
                        {
                            // copy the scripts from the according skirmish player for all AI players
                            CopyScripts(skirmishScripts.PlayerScripts.ScriptLists, skirmishScripts.Players.PlayerNames, "Skirmish" + player.Side, i + 1, appendIndex: true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copies source scripts to a target player, optionally renaming variables, script and team names based on the target player index.
        /// </summary>
        /// <param name="scriptsList">The scripts list.</param>
        /// <param name="playerNames">The source player names.</param>
        /// <param name="sourcePlayerName">The name of the source player to copy. This is used to find the index in the <paramref name="playerNames"/> array, which is then used to access the <paramref name="scriptsList"/> array.</param>
        /// <param name="targetPlayerIndex">The index in the <see cref="_playerScripts"/> array the scripts should be copied to.
        /// 0 .. Neutral
        /// 1 .. Civilian
        /// 2 .. player 1
        /// 3 .. player 2
        /// ...
        /// </param>.
        /// <param name="appendIndex">If set to <c>true</c>, the player index will be appended to all script, team and variable names in order to create unique names.</param>
        private void CopyScripts(ScriptList[] scriptsList, string[] playerNames, string sourcePlayerName, int targetPlayerIndex, bool appendIndex)
        {
            var sourcePlayerIndex = Array.FindIndex(playerNames, p => p.Equals(sourcePlayerName, StringComparison.OrdinalIgnoreCase));
            if (sourcePlayerIndex >= 0)
            {
                // In script files, the neutral player at index 0 is not included in the player names list
                if (scriptsList.Length > playerNames.Length)
                {
                    sourcePlayerIndex++;
                }

                // For player 1, we want to append "0" to all script names and variables, but his position in the array is 2.
                var appendix = appendIndex ? (targetPlayerIndex - 2).ToString() : null;
                _playerScripts[targetPlayerIndex] = scriptsList[sourcePlayerIndex].Copy(appendix);
            }
        }
    }
}
