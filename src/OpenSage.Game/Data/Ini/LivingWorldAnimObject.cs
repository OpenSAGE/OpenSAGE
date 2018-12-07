using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class LivingWorldAnimObject
    {
        internal static LivingWorldAnimObject Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldAnimObject> FieldParseTable = new IniParseTable<LivingWorldAnimObject>
        {
            { "Model", (parser, x) => x.Model = parser.ParseAssetReference() },
            { "Pos", (parser, x) => x.Pos = parser.ParseVector3() },
            { "Frame", (parser, x) => x.Frame = parser.ParseFloat() },
            { "HasAnim", (parser, x) => x.HasAnim = parser.ParseBoolean() },
            { "Xfer", (parser, x) => x.Xfer = parser.ParseBoolean() },
            { "OrientAngle", (parser, x) => x.OrientAngle = parser.ParseFloat() },
        };

        public string Name { get; private set; }

        public string Model { get; private set; }
        public Vector3 Pos { get; private set; }
        public float Frame { get; private set; }
        public bool HasAnim { get; private set; }
        public bool Xfer { get; private set; }
        public float OrientAngle { get; private set; }
    }
}
