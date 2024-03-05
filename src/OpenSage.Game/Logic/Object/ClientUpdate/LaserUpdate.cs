using System.Numerics;
using OpenSage.Client;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.Logic.Object
{
    public sealed class LaserUpdate : ClientUpdateModule
    {
        private Vector3 _flare1Position;
        private Vector3 _flare2Position;
        private uint _flare1SystemId;
        private uint _flare2SystemId;

        private bool _unknownBool1;
        private bool _unknownBool2;
        private LogicFrame _unknownFrame1;
        private LogicFrame _unknownFrame2;

        private float _unknownFloat;

        private LogicFrame _unknownFrame3;
        private LogicFrame _unknownFrame4;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistVector3(ref _flare1Position);
            reader.PersistVector3(ref _flare2Position);
            reader.SkipUnknownBytes(1);
            reader.PersistUInt32(ref _flare1SystemId);
            reader.PersistUInt32(ref _flare2SystemId);

            reader.PersistBoolean(ref _unknownBool1);
            reader.PersistBoolean(ref _unknownBool2);
            reader.PersistLogicFrame(ref _unknownFrame1); // perhaps start/stop frames?
            reader.PersistLogicFrame(ref _unknownFrame2);

            reader.PersistSingle(ref _unknownFloat);

            reader.PersistLogicFrame(ref _unknownFrame3);
            reader.PersistLogicFrame(ref _unknownFrame4);
        }
    }

    public sealed class LaserUpdateModuleData : ClientUpdateModuleData
    {
        internal static LaserUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LaserUpdateModuleData> FieldParseTable = new IniParseTable<LaserUpdateModuleData>
        {
            { "MuzzleParticleSystem", (parser, x) => x.MuzzleParticleSystem = parser.ParseFXParticleSystemTemplateReference() },
            { "ParentFireBoneName", (parser, x) => x.ParentFireBoneName = parser.ParseBoneName() },
            { "ParentFireBoneOnTurret", (parser, x) => x.ParentFireBoneOnTurret = parser.ParseBoolean() },
            { "TargetParticleSystem", (parser, x) => x.TargetParticleSystem = parser.ParseFXParticleSystemTemplateReference() },
            { "PunchThroughScalar", (parser, x) => x.PunchThroughScalar = parser.ParseFloat() },
            { "LaserLifetime", (parser, x) => x.LaserLifetime = parser.ParseTimeMillisecondsToLogicFrames() }
        };

        public LazyAssetReference<FXParticleSystemTemplate> MuzzleParticleSystem { get; private set; }
        public string ParentFireBoneName { get; private set; }
        public bool ParentFireBoneOnTurret { get; private set; }
        public LazyAssetReference<FXParticleSystemTemplate> TargetParticleSystem { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float PunchThroughScalar { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LogicFrameSpan LaserLifetime { get; private set; }

        internal override LaserUpdate CreateModule(Drawable drawable, GameContext context)
        {
            return new LaserUpdate();
        }
    }
}
