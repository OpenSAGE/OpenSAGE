using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class PartTheHeavensUpdateModuleData : UpdateModuleData
    {
        internal static PartTheHeavensUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PartTheHeavensUpdateModuleData> FieldParseTable = new IniParseTable<PartTheHeavensUpdateModuleData>
            {
                { "Texture", (parser, x) => x.Texture = parser.ParseAssetReference() },
                { "Color", (parser, x) => x.Color = parser.ParseColorRgba() },
                { "Radius", (parser, x) => x.Radius = FCurve.Parse(parser) },
                { "Opacity", (parser, x) => x.Opacity = FCurve.Parse(parser) },
                { "Angle", (parser, x) => x.Angle = FCurve.Parse(parser) },
            };

        public string Texture { get; private set; }
        public ColorRgba Color { get; private set; }
        public FCurve Radius { get; private set; }
        public FCurve Opacity { get; private set; }
        public FCurve Angle { get; private set; }
    }

    public sealed class FCurve
    {
        internal static FCurve Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<FCurve> FieldParseTable = new IniParseTable<FCurve>
        {
            { "Key", (parser, x) => x.Keys.Add(Key.Parse(parser)) },
            { "InPadding", (parser, x) => x.InPadding = parser.ParseEnum<Padding>() },
            { "OutPadding", (parser, x) => x.OutPadding = parser.ParseEnum<Padding>() },
        };

        public List<Key> Keys { get; } = new List<Key>();
        public Padding InPadding { get; private set; }
        public Padding OutPadding { get; private set; }
    }

    public sealed class Key
    {
        internal static Key Parse(IniParser parser) => parser.ParseAttributeList(FieldParseTable);

        internal static readonly IniParseTable<Key> FieldParseTable = new IniParseTable<Key>
        {
            { "T", (parser, x) => x.T = parser.ParseFloat() },
            { "V", (parser, x) => x.V = parser.ParseFloat() },
            { "I", (parser, x) => x.I = parser.ParseFloat() },
            { "O", (parser, x) => x.O = parser.ParseFloat() },
        };

        public float T { get; private set; }
        public float V { get; private set; }
        public float I { get; private set; }
        public float O { get; private set; }
    }

    public enum Padding
    {
        [IniEnum("HOLD")]
        Hold,

        [IniEnum("CYCLE")]
        Cycle,
    }
}
