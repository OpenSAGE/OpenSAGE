using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class W3dHordeModelDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dHordeModelDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<W3dHordeModelDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dHordeModelDrawModuleData>
            {
                { "StaticModelLODMode", (parser, x) => x.StaticModelLodMode = parser.ParseBoolean() },
                { "LodOptions", (parser, x) => x.LodOptions.Add(LodOption.Parse(parser)) }
            });

        public bool StaticModelLodMode { get; internal set; }
        public List<LodOption> LodOptions { get; internal set; } = new List<LodOption>();
    }

    public sealed class LodOption
    {
        internal static LodOption Parse(IniParser parser)
        {
            var lod = parser.ParseEnum<ModelLevelOfDetail>();
            var result = parser.ParseBlock(FieldParseTable);
            result.Lod = lod;
            return result;
        }

        internal static readonly IniParseTable<LodOption> FieldParseTable = new IniParseTable<LodOption>
            {
                { "AllowMultipleModels", (parser, x) => x.AllowMultipleModels = parser.ParseBoolean() },
                { "MaxRandomTextures", (parser, x) => x.MaxRandomTextures = parser.ParseInteger() },
                { "MaxRandomAnimations", (parser, x) => x.MaxRandomAnimations = parser.ParseInteger() },
                { "MaxAnimFrameDelta", (parser, x) => x.MaxAnimFrameDelta = parser.ParseInteger() }
            };

        public ModelLevelOfDetail Lod { get; private set; }

        public bool AllowMultipleModels { get; private set; }
        public int MaxRandomTextures { get; private set; }
        public int MaxRandomAnimations { get; internal set; }
        public int MaxAnimFrameDelta { get; private set; }
    }
}
