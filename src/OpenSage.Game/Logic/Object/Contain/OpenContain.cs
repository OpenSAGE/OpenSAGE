using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class OpenContainModule : UpdateModule
    {
        private readonly List<uint> _containedObjectIds = new();
        private uint _unknownFrame1;
        private uint _unknownFrame2;
        private BitArray<ModelConditionFlag> _modelConditionFlags = new();
        private readonly Matrix4x3[] _unknownTransforms = new Matrix4x3[32];
        private uint _nextFirePointIndex;
        private uint _numFirePoints;
        private bool _hasNoFirePoints;
        private readonly List<OpenContainSomething> _unknownList = new();
        private int _unknownInt;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistListWithUInt32Count(
                _containedObjectIds,
                static (StatePersister persister, ref uint item) =>
                {
                    persister.PersistObjectIDValue(ref item);
                });

            reader.SkipUnknownBytes(2);

            reader.PersistFrame(ref _unknownFrame1);
            reader.PersistFrame(ref _unknownFrame2);

            reader.SkipUnknownBytes(8);

            reader.PersistBitArray(ref _modelConditionFlags);

            // Where does the 32 come from?
            reader.PersistArray(
                _unknownTransforms,
                static (StatePersister persister, ref Matrix4x3 item) =>
                {
                    persister.PersistMatrix4x3Value(ref item, readVersion: false);
                });

            var unknown6 = -1;
            reader.PersistInt32(ref unknown6);
            if (unknown6 != -1)
            {
                throw new InvalidStateException();
            }

            reader.PersistUInt32(ref _nextFirePointIndex);
            reader.PersistUInt32(ref _numFirePoints);
            reader.PersistBoolean(ref _hasNoFirePoints);

            reader.SkipUnknownBytes(13);

            reader.PersistList(
                _unknownList,
                static (StatePersister persister, ref OpenContainSomething item) =>
                {
                    persister.PersistObjectValue(ref item);
                });

            reader.PersistInt32(ref _unknownInt);
        }

        private struct OpenContainSomething : IPersistableObject
        {
            public uint ObjectId;
            public int Unknown;

            public void Persist(StatePersister persister)
            {
                persister.PersistObjectID(ref ObjectId);
                persister.PersistInt32(ref Unknown);
            }
        }
    }

    public abstract class OpenContainModuleData : UpdateModuleData
    {
        internal static readonly IniParseTable<OpenContainModuleData> FieldParseTable = new IniParseTable<OpenContainModuleData>
        {
            { "AllowInsideKindOf", (parser, x) => x.AllowInsideKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "ForbidInsideKindOf", (parser, x) => x.ForbidInsideKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "ContainMax", (parser, x) => x.ContainMax = parser.ParseInteger() },
            { "EnterSound", (parser, x) => x.EnterSound = parser.ParseAssetReference() },
            { "ExitSound", (parser, x) => x.ExitSound = parser.ParseAssetReference() },
            { "DamagePercentToUnits", (parser, x) => x.DamagePercentToUnits = parser.ParsePercentage() },
            { "PassengersInTurret", (parser, x) => x.PassengersInTurret = parser.ParseBoolean() },
            { "NumberOfExitPaths", (parser, x) => x.NumberOfExitPaths = parser.ParseInteger() },
            { "AllowAlliesInside", (parser, x) => x.AllowAlliesInside = parser.ParseBoolean() },
            { "AllowNeutralInside", (parser, x) => x.AllowNeutralInside = parser.ParseBoolean() },
            { "AllowEnemiesInside", (parser, x) => x.AllowEnemiesInside = parser.ParseBoolean() },
            { "ShouldDrawPips", (parser, x) => x.ShouldDrawPips = parser.ParseBoolean() },
        };

        public BitArray<ObjectKinds> AllowInsideKindOf { get; private set; }
        public BitArray<ObjectKinds> ForbidInsideKindOf { get; private set; }
        public int ContainMax { get; private set; }
        public string EnterSound { get; private set; }
        public string ExitSound { get; private set; }
        public Percentage DamagePercentToUnits { get; private set; }
        public bool PassengersInTurret { get; private set; }
        public int NumberOfExitPaths { get; private set; }
        public bool AllowAlliesInside { get; private set; }
        public bool AllowNeutralInside { get; private set; }
        public bool AllowEnemiesInside { get; private set; }
        public bool ShouldDrawPips { get; private set; }
    }
}
