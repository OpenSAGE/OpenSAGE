using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class FloodUpdateModuleData : UpdateModuleData
    {
        internal static FloodUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FloodUpdateModuleData> FieldParseTable = new IniParseTable<FloodUpdateModuleData>
        {
            { "AngleOfFlow", (parser, x) => x.AngleOfFlow = parser.ParseFloat() },
            { "DirectionIsRelative", (parser, x) => x.DirectionIsRelative = parser.ParseBoolean() },
            { "FloodMember", (parser, x) => x.FloodMembers.Add(FloodMember.Parse(parser)) }
        };

        public float AngleOfFlow { get; private set; }
        public bool DirectionIsRelative { get; private set; }
        public List<FloodMember> FloodMembers { get; } = new List<FloodMember>();
    }

    public sealed class FloodMember
    {
        internal static FloodMember Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FloodMember> FieldParseTable = new IniParseTable<FloodMember>
        {
            { "MemberTemplateName", (parser, x) => x.MemberTemplateName = parser.ParseAssetReference() },
            { "ControlPointOffsetOne", (parser, x) => x.ControlPointOffsetOne = parser.ParseVector3() },
            { "ControlPointOffsetTwo", (parser, x) => x.ControlPointOffsetTwo = parser.ParseVector3() },
            { "ControlPointOffsetThree", (parser, x) => x.ControlPointOffsetThree = parser.ParseVector3() },
            { "ControlPointOffsetFour", (parser, x) => x.ControlPointOffsetFour = parser.ParseVector3() },
            { "MemberSpeed", (parser, x) => x.MemberSpeed = parser.ParseInteger() }
        };

        public string MemberTemplateName { get; private set; }
        public Vector3 ControlPointOffsetOne { get; private set; }
        public Vector3 ControlPointOffsetTwo { get; private set; }
        public Vector3 ControlPointOffsetThree { get; private set; }
        public Vector3 ControlPointOffsetFour { get; private set; }
        public int MemberSpeed { get; private set; }
    }
}
