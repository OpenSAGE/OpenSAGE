using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Logic.Object;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class DynamicPortalBehaviorModuleData : BehaviorModuleData
    {
        internal static DynamicPortalBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<DynamicPortalBehaviorModuleData> FieldParseTable = new IniParseTable<DynamicPortalBehaviorModuleData>
        {
            { "GenerateNow", (parser, x) => x.GenerateNow = parser.ParseBoolean() },
            { "AllowKindOf", (parser, x) => x.AllowKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "RejectKindOf", (parser, x) => x.RejectKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "AllowEnemies", (parser, x) => x.AllowEnemies = parser.ParseBoolean() },
            { "BonePrefix", (parser, x) => x.BonePrefix = parser.ParseString() },
            { "NumberOfBones", (parser, x) => x.NumberOfBones = parser.ParseInteger() },
            { "WayPoint", (parser, x) => x.WayPoints.Add(WayPoint.Parse(parser)) },
            { "Link", (parser, x) => x.Links.Add(Link.Parse(parser)) },
        };

        public bool GenerateNow { get; internal set; }
		public BitArray<ObjectKinds> AllowKindOf { get; internal set; }
		public BitArray<ObjectKinds> RejectKindOf { get; internal set; }
		public bool AllowEnemies { get; internal set; }
		public string BonePrefix { get; internal set; }
		public int NumberOfBones { get; internal set; }
		public List<WayPoint> WayPoints { get; internal set; } = new List<WayPoint>();
        public List<Link> Links { get; internal set; } = new List<Link>();
    }

    public enum WayPointType
    {
        None = 0,

        [IniEnum("PreClimb")]
        PreClimb,

        [IniEnum("Climb")]
        Climb
    }

    public sealed class WayPoint
    {
        internal static WayPoint Parse(IniParser parser)
        {
            return new WayPoint()
            {
                Index = parser.ParseAttributeInteger("Index"),
                Type = parser.ParseAttributeEnum<WayPointType>("Type")
            };
        }

        public int Index { get; internal set; }
        public WayPointType Type { get; internal set; }
    }

    public sealed class Link
    {
        internal static Link Parse(IniParser parser)
        {
            return new Link()
            {
                From = parser.ParseAttributeInteger("From"),
                Via1 = parser.ParseAttributeInteger("Via"),
                Via2 = parser.ParseAttributeInteger("Via"),
                To = parser.ParseAttributeInteger("To")
            };
        }

        public int From { get; internal set; }
        public int Via1 { get; internal set; }
        public int Via2 { get; internal set; }
        public int To { get; internal set; }
    }
}
