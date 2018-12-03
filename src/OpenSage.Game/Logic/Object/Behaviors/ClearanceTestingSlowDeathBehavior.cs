using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ClearanceTestingSlowDeathBehaviorModuleData : SlowDeathBehaviorModuleData
    {
        internal static new ClearanceTestingSlowDeathBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ClearanceTestingSlowDeathBehaviorModuleData> FieldParseTable = SlowDeathBehaviorModuleData.FieldParseTable
            .Concat(new IniParseTable<ClearanceTestingSlowDeathBehaviorModuleData>
            {
                { "MinKillerAngle", (parser, x) => x.MinKillerAngle = parser.ParseInteger() },
                { "MaxKillerAngle", (parser, x) => x.MaxKillerAngle = parser.ParseInteger() },
                { "ClearanceGeometry", (parser, x) => x.ClearanceGeometry = new Geometry(parser.ParseEnum<ObjectGeometry>()) },
                { "ClearanceGeometryMajorRadius", (parser, x) => x.ClearanceGeometry.MajorRadius = parser.ParseFloat() },
                { "ClearanceGeometryMinorRadius", (parser, x) => x.ClearanceGeometry.MinorRadius = parser.ParseFloat() },
                { "ClearanceGeometryHeight", (parser, x) => x.ClearanceGeometry.Height = parser.ParseFloat() },
                { "ClearanceGeometryIsSmall", (parser, x) => x.ClearanceGeometry.IsSmall = parser.ParseBoolean() },
                { "ClearanceGeometryOffset", (parser, x) => x.ClearanceGeometry.Offset = parser.ParseVector3() },
                { "ClearanceMaxHeight", (parser, x) => x.ClearanceMaxHeight = parser.ParseInteger() },
                { "ClearanceMinHeight", (parser, x) => x.ClearanceMinHeight = parser.ParseInteger() },
                { "ClearanceMaxHeightFraction", (parser, x) => x.ClearanceMaxHeightFraction = parser.ParseFloat() },
                { "ClearanceMinHeightFraction", (parser, x) => x.ClearanceMinHeightFraction = parser.ParseFloat() },
                { "DamageAmountRequired", (parser, x) => x.DamageAmountRequired = parser.ParseInteger() }
            });

        public int MinKillerAngle { get; private set; }
        public int MaxKillerAngle { get; private set; }
        public Geometry ClearanceGeometry { get; private set; }
        public int ClearanceMaxHeight { get; private set; }
        public int ClearanceMinHeight { get; private set; }
        public float ClearanceMinHeightFraction { get; private set; }
        public float ClearanceMaxHeightFraction { get; private set; }
        public int DamageAmountRequired { get; private set; }
    }
}
