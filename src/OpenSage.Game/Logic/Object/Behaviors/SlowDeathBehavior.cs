using System;
using System.Collections.Generic;
using ImGuiNET;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Diagnostics.Util;
using OpenSage.FX;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class SlowDeathBehavior : UpdateModule
    {
        private readonly SlowDeathBehaviorModuleData _moduleData;

        private bool _isDying;
        private SlowDeathPhase? _phase;
        private bool _passedMidpoint;
        private LogicFrame _sinkStartTime;
        private LogicFrame _midpointTime;
        private LogicFrame _destructionTime;

        private uint _frameSinkStart;
        private uint _frameMidpoint;
        private uint _frameDestruction;
        private float _slowDeathScale;
        private SlowDeathBehaviorFlags _flags;

        public int ProbabilityModifier => _moduleData.ProbabilityModifier;

        internal SlowDeathBehavior(SlowDeathBehaviorModuleData moduleData)
        {
            _moduleData = moduleData;
        }

        internal bool IsApplicable(DeathType deathType) => _moduleData.DeathTypes?.Get(deathType) ?? true;

        internal override void OnDie(BehaviorUpdateContext context, DeathType deathType)
        {
            if (!IsApplicable(deathType))
            {
                return;
            }

            _isDying = true;

            context.GameObject.ModelConditionFlags.Set(ModelConditionFlag.Dying, true);

            // TODO: ProbabilityModifier

            var destructionDelay = GetDelayWithVariance(context, _moduleData.DestructionDelay, _moduleData.DestructionDelayVariance);
            _midpointTime = context.LogicFrame + (destructionDelay / 2.0f);
            _destructionTime = context.LogicFrame + destructionDelay;

            _sinkStartTime = context.LogicFrame + GetDelayWithVariance(context, _moduleData.SinkDelay, _moduleData.SinkDelayVariance);

            // TODO: Decay
            // TODO: Fling

            ExecutePhaseActions(context, SlowDeathPhase.Initial);
        }

        private static LogicFrameSpan GetDelayWithVariance(BehaviorUpdateContext context, LogicFrameSpan delay, LogicFrameSpan variance)
        {
            var randomMultiplier = (context.GameContext.Random.NextDouble() * 2.0) - 1.0;
            return delay + (variance * (float)randomMultiplier);
        }

        private void ExecutePhaseActions(BehaviorUpdateContext context, SlowDeathPhase phase)
        {
            _phase = phase;

            if (_moduleData.OCLs.TryGetValue(phase, out var ocl))
            {
                context.GameContext.ObjectCreationLists.Create(ocl.Value, context);
            }

            if (_moduleData.FXs.TryGetValue(phase, out var fx))
            {
                fx.Value.Execute(
                    new FXListExecutionContext(
                        context.GameObject.Rotation,
                        context.GameObject.Translation,
                        context.GameContext));
            }

            // TODO: Weapon
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            if (!_isDying)
            {
                return;
            }

            // TODO: SlowDeathPhase.HitGround

            // Midpoint
            if (!_passedMidpoint && context.LogicFrame >= _midpointTime)
            {
                ExecutePhaseActions(context, SlowDeathPhase.Midpoint);
                _passedMidpoint = true;
            }

            // Destruction
            if (context.LogicFrame >= _destructionTime)
            {
                ExecutePhaseActions(context, SlowDeathPhase.Final);
                context.GameObject.ModelConditionFlags.Set(ModelConditionFlag.Dying, false);
                context.GameContext.GameObjects.DestroyObject(context.GameObject);
                _isDying = false;
            }

            // Sinking
            if (context.LogicFrame >= _sinkStartTime)
            {
                context.GameObject.VerticalOffset -= _moduleData.SinkRate;
            }
        }

        internal override void DrawInspector()
        {
            if (_phase == null)
            {
                ImGui.LabelText("Phase", "<not dying>");
            }
            else
            {
                var phase = _phase.Value;
                if (ImGuiUtility.ComboEnum("Phase", ref phase))
                {
                    _phase = phase;
                }
            }
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistFrame(ref _frameSinkStart);
            reader.PersistFrame(ref _frameMidpoint);
            reader.PersistFrame(ref _frameDestruction);
            reader.PersistSingle(ref _slowDeathScale);
            reader.PersistEnumFlags(ref _flags);
        }

        [Flags]
        private enum SlowDeathBehaviorFlags
        {
            None = 0,
            BegunSlowDeath = 1,
            ReachedMidpoint = 2,
        }
    }

    public class SlowDeathBehaviorModuleData : UpdateModuleData
    {
        public override ModuleKinds ModuleKinds => ModuleKinds.Update | ModuleKinds.Die;

        internal static SlowDeathBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<SlowDeathBehaviorModuleData> FieldParseTable = new IniParseTable<SlowDeathBehaviorModuleData>
        {
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            { "DeathFlags", (parser, x) => x.DeathFlags = parser.ParseEnumFlags<DeathFlags>() },
            { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnumBitArray<ObjectStatus>() },
            { "ExemptStatus", (parser, x) => x.ExemptStatus = parser.ParseEnumBitArray<ObjectStatus>() },
            { "ProbabilityModifier", (parser, x) => x.ProbabilityModifier = parser.ParseInteger() },
            { "ModifierBonusPerOverkillPercent", (parser, x) => x.ModifierBonusPerOverkillPercent = parser.ParsePercentage() },
            { "SinkRate", (parser, x) => x.SinkRate = parser.ParseVelocityToLogicFrames() },
            { "SinkDelay", (parser, x) => x.SinkDelay = parser.ParseTimeMillisecondsToLogicFrames() },
            { "SinkDelayVariance", (parser, x) => x.SinkDelayVariance = parser.ParseTimeMillisecondsToLogicFrames() },
            { "DestructionDelay", (parser, x) => x.DestructionDelay = parser.ParseTimeMillisecondsToLogicFrames() },
            { "DestructionDelayVariance", (parser, x) => x.DestructionDelayVariance = parser.ParseTimeMillisecondsToLogicFrames() },
            { "FlingForce", (parser, x) => x.FlingForce = parser.ParseInteger() },
            { "FlingForceVariance", (parser, x) => x.FlingForceVariance = parser.ParseInteger() },
            { "FlingPitch", (parser, x) => x.FlingPitch = parser.ParseInteger() },
            { "FlingPitchVariance", (parser, x) => x.FlingPitchVariance = parser.ParseInteger() },
            { "OCL", (parser, x) => x.OCLs[parser.ParseEnum<SlowDeathPhase>()] = parser.ParseObjectCreationListReference() },
            { "FX", (parser, x) => x.FXs[parser.ParseEnum<SlowDeathPhase>()] = parser.ParseFXListReference() },
            { "Weapon", (parser, x) => x.Weapons[parser.ParseEnum<SlowDeathPhase>()] = parser.ParseAssetReference() },
            { "FadeDelay", (parser, x) => x.FadeDelay = parser.ParseInteger() },
            { "FadeTime", (parser, x) => x.FadeTime = parser.ParseInteger() },
            { "Sound", (parser, x) => x.Sound = parser.ParseString() },
            { "DecayBeginTime", (parser, x) => x.DecayBeginTime = parser.ParseInteger() },
            { "ShadowWhenDead", (parser, x) => x.ShadowWhenDead = parser.ParseBoolean() },
            { "DoNotRandomizeMidpoint", (parser, x) => x.DoNotRandomizeMidpoint = parser.ParseBoolean() }
        };

        public BitArray<DeathType> DeathTypes { get; private set; }
        public BitArray<ObjectStatus> RequiredStatus { get; private set; }
        public BitArray<ObjectStatus> ExemptStatus { get; private set; }
        public int ProbabilityModifier { get; private set; }
        public Percentage ModifierBonusPerOverkillPercent { get; private set; }
        public float SinkRate { get; private set; }
        public LogicFrameSpan SinkDelay { get; private set; }
        public LogicFrameSpan SinkDelayVariance { get; private set; }
        public LogicFrameSpan DestructionDelay { get; private set; }
        public LogicFrameSpan DestructionDelayVariance { get; private set; }
        public int FlingForce { get; private set; }
        public int FlingForceVariance { get; private set; }
        public int FlingPitch { get; private set; }
        public int FlingPitchVariance { get; private set; }

        public Dictionary<SlowDeathPhase, LazyAssetReference<ObjectCreationList>> OCLs { get; } = new Dictionary<SlowDeathPhase, LazyAssetReference<ObjectCreationList>>();
        public Dictionary<SlowDeathPhase, LazyAssetReference<FXList>> FXs { get; } = new Dictionary<SlowDeathPhase, LazyAssetReference<FXList>>();
        public Dictionary<SlowDeathPhase, string> Weapons { get; } = new Dictionary<SlowDeathPhase, string>();

        [AddedIn(SageGame.Bfme)]
        public DeathFlags DeathFlags { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int FadeDelay { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int FadeTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string Sound { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DecayBeginTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ShadowWhenDead { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool DoNotRandomizeMidpoint { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new SlowDeathBehavior(this);
        }
    }

    public enum SlowDeathPhase
    {
        [IniEnum("INITIAL")]
        Initial,

        [IniEnum("MIDPOINT")]
        Midpoint,

        [IniEnum("FINAL")]
        Final,

        [IniEnum("HIT_GROUND")]
        HitGround
    }

    [AddedIn(SageGame.Bfme)]
    [Flags]
    public enum DeathFlags
    {
        None = 0,

        [IniEnum("DEATH_1")]
        Death1 = 1 << 0,

        [IniEnum("DEATH_2")]
        Death2 = 1 << 1,

        [IniEnum("DEATH_3")]
        Death3 = 1 << 2,

        [IniEnum("DEATH_4")]
        Death4 = 1 << 3,

        [IniEnum("DEATH_5")]
        Death5 = 1 << 4,
    }
}
