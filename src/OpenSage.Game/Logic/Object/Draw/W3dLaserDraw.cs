﻿using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Wnd;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Interdependent with the <see cref="LaserUpdateModuleData"/> module and requires the object to have 
    /// KindOf = INERT IMMOBILE.
    /// </summary>
    public sealed class W3dLaserDrawModuleData : DrawModuleData
    {
        internal static W3dLaserDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dLaserDrawModuleData> FieldParseTable = new IniParseTable<W3dLaserDrawModuleData>
        {
            { "Texture", (parser, x) => x.Textures.Add(parser.ParseFileName()) },
            { "NumBeams", (parser, x) => x.NumBeams = parser.ParseInteger() },
            { "InnerBeamWidth", (parser, x) => x.InnerBeamWidth = parser.ParseFloat() },
            { "InnerColor", (parser, x) => x.InnerColor = parser.ParseColorRgba() },
            { "OuterBeamWidth", (parser, x) => x.OuterBeamWidth = parser.ParseFloat() },
            { "OuterColor", (parser, x) => x.OuterColor = parser.ParseColorRgba() },
            { "Tile", (parser, x) => x.Tile = parser.ParseBoolean() },
            { "ScrollRate", (parser, x) => x.ScrollRate = parser.ParseFloat() },
            { "Segments", (parser, x) => x.Segments = parser.ParseInteger() },
            { "ArcHeight", (parser, x) => x.ArcHeight = parser.ParseFloat() },
            { "SegmentOverlapRatio", (parser, x) => x.SegmentOverlapRatio = parser.ParseFloat() },
            { "TilingScalar", (parser, x) => x.TilingScalar = parser.ParseFloat() },
            { "FanWidth", (parser, x) => x.FanWidth = parser.ParseFloat() },
            { "Envelope", (parser, x) => x.Envelope = Envelope.Parse(parser) }
        };

        public List<string> Textures { get; } = new List<string>();
        public int NumBeams { get; private set; }
        public float InnerBeamWidth { get; private set; }
        public ColorRgba InnerColor { get; private set; }
        public float OuterBeamWidth { get; private set; }
        public ColorRgba OuterColor { get; private set; }
        public bool Tile { get; private set; }
        public float ScrollRate { get; private set; }
        public int Segments { get; private set; }
        public float ArcHeight { get; private set; }
        public float SegmentOverlapRatio { get; private set; }
        public float TilingScalar { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float FanWidth { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Envelope Envelope { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class Envelope
    {
        internal static Envelope Parse(IniParser parser)
        {
            return new Envelope
            {
                InitialOpacity = parser.ParseAttributeFloat("InitialOpacity"),
                PeakOpacity = parser.ParseAttributeFloat("PeakOpacity"),
                SustainOpacity = parser.ParseAttributeFloat("SustainOpacity"),
                InitialDelay = parser.ParseAttributeFloat("InitialDelay"),
                AttackTime = parser.ParseAttributeInteger("AttackTime"),
                DecayTime = parser.ParseAttributeInteger("DecayTime"),
                SustainTime = parser.ParseAttributeInteger("SustainTime"),
                ReleaseTime = parser.ParseAttributeInteger("ReleaseTime")
            };
        }

        public float InitialOpacity { get; private set; }
        public float PeakOpacity { get; private set; }
        public float SustainOpacity { get; private set; }
        public float InitialDelay { get; private set; }
        public int AttackTime { get; private set; }
        public int DecayTime { get; private set; }
        public int SustainTime { get; private set; }
        public int ReleaseTime { get; private set; }
    }
}
