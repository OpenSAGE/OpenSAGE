using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

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
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReferenceArray() },
            { "CustomAnimAndDuration", (parser, x) => x.CustomAnimAndDuration = AnimAndDuration.Parse(parser) },
            { "ActivationDelaySeconds", (parser, x) => x.ActivationDelaySeconds = parser.ParseFloat() },
            { "AboveWall", (parser, x) => x.AboveWall = parser.ParseInteger() },
            { "TopAttackPos", (parser, x) => x.TopAttackPos = parser.ParseVector3() },
            { "TopAttackRadius", (parser, x) => x.TopAttackRadius = parser.ParseInteger() },
            { "ObjectFilter", (parser, x) => x.ObjectFilter = ObjectFilter.Parse(parser) }
        };

        public bool GenerateNow { get; private set; }
        public BitArray<ObjectKinds> AllowKindOf { get; private set; }
        public BitArray<ObjectKinds> RejectKindOf { get; private set; }
        public bool AllowEnemies { get; private set; }
        public string BonePrefix { get; private set; }
        public int NumberOfBones { get; private set; }
        public List<WayPoint> WayPoints { get; private set; } = new List<WayPoint>();
        public List<Link> Links { get; private set; } = new List<Link>();
        public string[] TriggeredBy { get; private set; }
        public string[] ConflictsWith { get; private set; }
        public AnimAndDuration CustomAnimAndDuration { get; private set; }
        public float ActivationDelaySeconds { get; private set; }
        public int AboveWall { get; private set; }
        public Vector3 TopAttackPos { get; private set; }
        public int TopAttackRadius { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectFilter ObjectFilter { get; private set; }
    }

    public enum WayPointType
    {
        None = 0,

        [IniEnum("PreClimb")]
        PreClimb,

        [IniEnum("Climb")]
        Climb,

        [IniEnum("Walk")]
        Walk
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

        public int Index { get; private set; }
        public WayPointType Type { get; private set; }
    }

    public sealed class Link
    {
        internal static Link Parse(IniParser parser) => parser.ParseAttributeList(FieldParseTable);

        internal static readonly IniParseTable<Link> FieldParseTable = new IniParseTable<Link>
        {
            { "From", (parser, x) => x.From = parser.ParseInteger() },
            { "Via", (parser, x) => x.Vias.Add(parser.ParseInteger()) },
            { "To", (parser, x) => x.To = parser.ParseInteger() }
        };

        public int From { get; private set; }
        public List<int> Vias { get; } = new List<int>();
        public int To { get; private set; }
    }
}
