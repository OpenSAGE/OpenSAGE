using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class W3dTreeDrawModuleData : DrawModuleData
    {
        internal static W3dTreeDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dTreeDrawModuleData> FieldParseTable = new IniParseTable<W3dTreeDrawModuleData>
        {
            { "ModelName", (parser, x) => x.ModelName = parser.ParseFileName() },
            { "TextureName", (parser, x) => x.TextureName = parser.ParseFileName() },

            { "DoTopple", (parser, x) => x.DoTopple = parser.ParseBoolean() },
            { "DoShadow", (parser, x) => x.DoShadow = parser.ParseBoolean() },
            { "ToppleFX", (parser, x) => x.ToppleFX = parser.ParseAssetReference() },
            { "BounceFX", (parser, x) => x.BounceFX = parser.ParseAssetReference() },
            { "KillWhenFinishedToppling", (parser, x) => x.KillWhenFinishedToppling = parser.ParseBoolean() },
            { "SinkDistance", (parser, x) => x.SinkDistance = parser.ParseInteger() },
            { "SinkTime", (parser, x) => x.SinkTime = parser.ParseInteger() },
        };

        public string ModelName { get; private set; }
        public string TextureName { get; private set; }

        public bool DoTopple { get; private set; }
        public bool DoShadow { get; private set; }
        public string ToppleFX { get; private set; }
        public string BounceFX { get; private set; }
        public bool KillWhenFinishedToppling { get; private set; }
        public int SinkDistance { get; private set; }
        public int SinkTime { get; private set; }
    }
}
