using System;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Logic.AI.AIStates;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class HackInternetAIUpdate : AIUpdate
    {
        private PackingUpData? _packingUpData;

        private readonly GameContext _context;
        private readonly HackInternetAIUpdateModuleData _moduleData;

        internal HackInternetAIUpdate(GameObject gameObject, GameContext context, HackInternetAIUpdateModuleData moduleData) : base(gameObject, moduleData)
        {
            _context = context;
            _moduleData = moduleData;
        }

        private protected override void RunUpdate(BehaviorUpdateContext context)
        {
            switch (StateMachine.CurrentState)
            {
                case StartHackingInternetState start:
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, true);
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, false);
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, false);
                    if (start.FramesUntilHackingBegins-- == LogicFrameSpan.Zero)
                    {
                        // update state to hack
                        StateMachine.SetState(1001);
                        ResetFramesUntilNextHack(StateMachine.CurrentState as HackInternetState);
                    }
                    break;
                case HackInternetState hack:
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, false);
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, true);
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, false);
                    if (hack.FramesUntilNextHack-- == LogicFrameSpan.Zero)
                    {
                        hack.FramesUntilNextHack = GameObject.ContainerId != 0 ? _moduleData.CashUpdateDelayFast : _moduleData.CashUpdateDelay;
                        var amount = GetCashGrant();
                        GameObject.Owner.BankAccount.Deposit((uint)amount);
                        _context.AudioSystem.PlayAudioEvent(GameObject, GameObject.Definition.UnitSpecificSounds.UnitCashPing?.Value);
                        GameObject.ActiveCashEvent = new CashEvent(amount, new ColorRgb(0, 255, 0), new Vector3(0, 0, 20));
                        GameObject.GainExperience(_moduleData.XpPerCashUpdate);
                    }
                    break;
                case StopHackingInternetState stop:
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.Unpacking, false);
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.FiringA, false);
                    GameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, true);
                    if (stop.FramesUntilFinishedPacking-- == LogicFrameSpan.Zero)
                    {
                        // update state to idle
                        StateMachine.SetState(0);
                        GameObject.ModelConditionFlags.Set(ModelConditionFlag.Packing, false);
                        if (_packingUpData != null && _packingUpData.MoveTarget != default)
                        {
                            SetTargetPoint(_packingUpData.MoveTarget);
                        }
                        _packingUpData = null;
                    }
                    break;
            }
        }

        private void ResetFramesUntilNextHack(HackInternetState hack)
        {
            hack.FramesUntilNextHack = GameObject.ContainerId != 0 ? _moduleData.CashUpdateDelayFast : _moduleData.CashUpdateDelay;
        }

        private int GetCashGrant()
        {
            return GameObject.Rank switch
            {
                0 => _moduleData.RegularCashAmount,
                1 => _moduleData.VeteranCashAmount,
                2 => _moduleData.EliteCashAmount,
                3 => _moduleData.HeroicCashAmount,
                _ => throw new ArgumentOutOfRangeException(nameof(GameObject.Rank)),
            };
        }

        public void StartHackingInternet()
        {
            Stop();

            // todo: adjust animation duration
            var frames = GetVariableFrames(_moduleData.UnpackTime, _moduleData.PackUnpackVariationFactor);

            StateMachine.SetState(1000);
            if (StateMachine.CurrentState is not StartHackingInternetState start)
            {
                throw new InvalidStateException();
            }

            _context.AudioSystem.PlayAudioEvent(GameObject, GameObject.Definition.UnitSpecificSounds.UnitUnpack?.Value);

            start.FramesUntilHackingBegins = frames;
        }

        internal override void SetTargetPoint(Vector3 targetPoint)
        {
            Stop();

            if (StateMachine.CurrentState is StopHackingInternetState)
            {
                // we can't move just yet
                _packingUpData = new PackingUpData { MoveTarget = targetPoint };
            }
            else
            {
                base.SetTargetPoint(targetPoint);
            }
        }

        internal override void Stop()
        {
            switch (StateMachine.CurrentState)
            {
                case StartHackingInternetState:
                    // this takes effect immediately
                    StateMachine.SetState(0);
                    break;
                case HackInternetState:
                    if (StateMachine.CurrentState is HackInternetState)
                    {
                        // todo: adjust animation duration
                        var frames = GetVariableFrames(_moduleData.PackTime, _moduleData.PackUnpackVariationFactor);

                        StateMachine.SetState(1002);
                        if (StateMachine.CurrentState is not StopHackingInternetState start)
                        {
                            throw new InvalidStateException();
                        }

                        _context.AudioSystem.PlayAudioEvent(GameObject, GameObject.Definition.UnitSpecificSounds.UnitPack?.Value);

                        start.FramesUntilFinishedPacking = frames;
                    }
                    break;
                // If we're in StopHackingInternetState, we need to see that through
            }

            base.Stop();
        }

        private LogicFrameSpan GetVariableFrames(LogicFrameSpan time, float variance)
        {
            // take a random float, *2 for 0 - 2, -1 for -1 - 1, *variance for our actual variance factor
            return new LogicFrameSpan((uint)(time.Value + time.Value * ((_context.Random.NextSingle() * 2 - 1) * variance)));
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            var hasPackingUpData = _packingUpData != null;
            reader.PersistBoolean(ref hasPackingUpData);
            if (hasPackingUpData)
            {
                _packingUpData ??= new PackingUpData();
                reader.PersistObject(_packingUpData);
            }
        }

        private sealed class PackingUpData : IPersistableObject
        {
            private int _unknownInt1;
            private int _unknownInt2;

            public Vector3 MoveTarget;
            private uint _enterTargetObjectId;

            private uint _unknownUInt1;
            private uint _unknownUInt2;

            private bool _unknownBool1;
            private bool _unknownBool2;
            private bool _unknownBool3;

            public void Persist(StatePersister persister)
            {
                // I think these have something to do with the next state machine to use, but I'm not sure
                // as far as I can tell, this object doesn't support alt-clicking for multiple actions though
                persister.PersistInt32(ref _unknownInt1);
                persister.PersistInt32(ref _unknownInt2);

                if (_unknownInt1 != _unknownInt2)
                {
                    throw new InvalidStateException();
                }

                persister.PersistVector3(ref MoveTarget);
                persister.PersistObjectID(ref _enterTargetObjectId);

                persister.SkipUnknownBytes(9);

                persister.PersistUInt32(ref _unknownUInt1);
                if (_unknownUInt1 != 0x7FFFFFFF)
                {
                    throw new InvalidStateException();
                }

                persister.SkipUnknownBytes(1);

                persister.PersistUInt32(ref _unknownUInt2);
                if (_unknownUInt2 != 0 && _unknownUInt2 != 0x7FFFFFFF) // 0x7FFFFFFF when attack move?
                {
                    throw new InvalidStateException();
                }

                persister.PersistBoolean(ref _unknownBool1);
                if (!_unknownBool1)
                {
                    throw new InvalidStateException();
                }

                persister.PersistBoolean(ref _unknownBool2);
                if (!_unknownBool2)
                {
                    throw new InvalidStateException();
                }

                persister.SkipUnknownBytes(18);

                persister.PersistBoolean(ref _unknownBool3);
                if (!_unknownBool3)
                {
                    throw new InvalidStateException();
                }

                persister.SkipUnknownBytes(11);
            }
        }
    }

    /// <summary>
    /// Allows use of UnitPack, UnitUnpack, and UnitCashPing within the UnitSpecificSounds section
    /// of the object.
    /// Also allows use of PACKING and UNPACKING condition states.
    /// </summary>
    public sealed class HackInternetAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static HackInternetAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<HackInternetAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<HackInternetAIUpdateModuleData>
            {
                { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseTimeMillisecondsToLogicFrames() },
                { "PackTime", (parser, x) => x.PackTime = parser.ParseTimeMillisecondsToLogicFrames() },
                { "CashUpdateDelay", (parser, x) => x.CashUpdateDelay = parser.ParseTimeMillisecondsToLogicFrames() },
                { "CashUpdateDelayFast", (parser, x) => x.CashUpdateDelayFast = parser.ParseTimeMillisecondsToLogicFrames() },
                { "RegularCashAmount", (parser, x) => x.RegularCashAmount = parser.ParseInteger() },
                { "VeteranCashAmount", (parser, x) => x.VeteranCashAmount = parser.ParseInteger() },
                { "EliteCashAmount", (parser, x) => x.EliteCashAmount = parser.ParseInteger() },
                { "HeroicCashAmount", (parser, x) => x.HeroicCashAmount = parser.ParseInteger() },
                { "XpPerCashUpdate", (parser, x) => x.XpPerCashUpdate = parser.ParseInteger() },
                { "PackUnpackVariationFactor", (parser, x) => x.PackUnpackVariationFactor = parser.ParseFloat() },
            });

        public LogicFrameSpan UnpackTime { get; private set; }
        public LogicFrameSpan PackTime { get; private set; }
        public LogicFrameSpan CashUpdateDelay { get; private set; }

        /// <summary>
        /// Hack speed when in a container (presumably with <see cref="InternetHackContainModuleData"/>).
        /// </summary>
        /// <remarks>
        /// The ini comments say "Fast speed used inside a container (can only hack inside an Internet Center)", however
        /// other mods will use this inside of e.g. listening outposts ("hacker vans"), so this can definitely be used
        /// in <i>any</i> container, not just internet centers.
        /// </remarks>
        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public LogicFrameSpan CashUpdateDelayFast { get; private set; }

        public int RegularCashAmount { get; private set; }
        public int VeteranCashAmount { get; private set; }
        public int EliteCashAmount { get; private set; }
        public int HeroicCashAmount { get; private set; }
        public int XpPerCashUpdate { get; private set; }

        /// <summary>
        /// Adds +/- the factor to the pack and unpack time, randomly.
        /// </summary>
        /// <example>
        /// If this is 0.5 and the unpack time is 1000ms, the actual unpack time may be anywhere between 500 and 1500ms.
        /// </example>
        public float PackUnpackVariationFactor { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new HackInternetAIUpdate(gameObject, context, this);
        }
    }
}
