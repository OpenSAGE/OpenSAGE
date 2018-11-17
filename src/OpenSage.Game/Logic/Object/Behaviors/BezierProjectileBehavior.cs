using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class BezierProjectileBehaviorData : BehaviorModuleData
    {
        internal static BezierProjectileBehaviorData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<BezierProjectileBehaviorData> FieldParseTable = new IniParseTable<BezierProjectileBehaviorData>
        {
            { "FirstHeight", (parser, x) => x.FirstHeight = parser.ParseInteger() },
            { "SecondHeight", (parser, x) => x.SecondHeight = parser.ParseInteger() },
            { "FirstPercentIndent", (parser, x) => x.FirstPercentIndent = parser.ParsePercentage() },
            { "SecondPercentIndent", (parser, x) => x.SecondPercentIndent = parser.ParsePercentage() },
            { "TumbleRandomly", (parser, x) => x.TumbleRandomly = parser.ParseBoolean() },
            { "CrushStyle", (parser, x) => x.CrushStyle = parser.ParseBoolean() },
            { "DieOnImpact", (parser, x) => x.DieOnImpact = parser.ParseBoolean() },
            { "BounceCount", (parser, x) => x.BounceCount = parser.ParseInteger() },
            { "BounceDistance", (parser, x) => x.BounceDistance = parser.ParseInteger() },
            { "BounceFirstHeight", (parser, x) => x.BounceFirstHeight = parser.ParseInteger() },
            { "BounceSecondHeight", (parser, x) => x.BounceSecondHeight = parser.ParseInteger() },
            { "BounceFirstPercentIndent", (parser, x) => x.BounceFirstPercentIndent = parser.ParsePercentage() },
            { "BounceSecondPercentIndent", (parser, x) => x.BounceSecondPercentIndet = parser.ParsePercentage() },
            { "GroundHitFX", (parser, x) => x.GroundHitFX = parser.ParseAssetReference() },
            { "GroundBounceFX", (parser, x) => x.GroundBounceFX = parser.ParseAssetReference() },
        };

        public int FirstHeight { get; private set; }
        public int SecondHeight { get; private set; }
        public float FirstPercentIndent { get; private set; }
        public float SecondPercentIndent { get; private set; }
        public bool TumbleRandomly { get; private set; }

        public bool CrushStyle { get; private set; }
        public bool DieOnImpact { get; private set; }
        public int BounceCount { get; private set; }
        public int BounceDistance { get; private set; }
        public int BounceFirstHeight { get; private set; }
        public int BounceSecondHeight { get; private set; }
        public float BounceFirstPercentIndent { get; private set; }
        public float BounceSecondPercentIndet { get; private set; }
        public string GroundHitFX { get; private set; }
        public string GroundBounceFX { get; private set; }
    }
}
