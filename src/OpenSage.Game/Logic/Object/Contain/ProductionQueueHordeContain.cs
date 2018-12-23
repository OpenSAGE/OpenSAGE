using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public class ProductionQueueHordeContainModuleData : BehaviorModuleData
    {
        internal static ProductionQueueHordeContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<ProductionQueueHordeContainModuleData> FieldParseTable = new IniParseTable<ProductionQueueHordeContainModuleData>
        {
            { "ObjectStatusOfContained", (parser, x) => x.ObjectStatusOfContained = parser.ParseEnumBitArray<ObjectStatus>() },
            { "ContainMax", (parser, x) => x.ContainMax = parser.ParseInteger() },
            { "DamagePercentToUnits", (parser, x) => x.DamagePercentToUnits = parser.ParsePercentage() },
            { "PassengerFilter", (parser, x) => x.PassengerFilter = ObjectFilter.Parse(parser) },
            { "AllowEnemiesInside", (parser, x) => x.AllowEnemiesInside = parser.ParseBoolean() },
            { "AllowNeutralInside", (parser, x) => x.AllowNeutralInside = parser.ParseBoolean() },
            { "AllowAlliesInside", (parser, x) => x.AllowAlliesInside = parser.ParseBoolean() },
            { "NumberOfExitPaths", (parser, x) => x.NumberOfExitPaths = parser.ParseInteger() },
            { "PassengerBonePrefix", (parser, x) => x.PassengerBonePrefix = PassengerBonePrefix.Parse(parser) },
            { "EntryPosition", (parser, x) => x.EntryPosition = parser.ParseVector3() },
            { "EntryOffset", (parser, x) => x.EntryOffset = parser.ParseVector3() },
            { "ExitOffset", (parser, x) => x.ExitOffset = parser.ParseVector3() },
            { "EnterSound", (parser, x) => x.EnterSound = parser.ParseAssetReference() }
        };

        public BitArray<ObjectStatus> ObjectStatusOfContained { get; private set; }
        public int ContainMax { get; private set; }
        public float DamagePercentToUnits { get; private set; }
        public ObjectFilter PassengerFilter { get; private set; }
        public bool AllowEnemiesInside { get; private set; }
        public bool AllowNeutralInside { get; private set; }
        public bool AllowAlliesInside { get; private set; }
        public int NumberOfExitPaths { get; private set; }
        public PassengerBonePrefix PassengerBonePrefix { get; private set; }
        public Vector3 EntryPosition { get; private set; }
        public Vector3 EntryOffset { get; private set; }
        public Vector3 ExitOffset { get; private set; }
        public string EnterSound { get; private set; }
    }
}
