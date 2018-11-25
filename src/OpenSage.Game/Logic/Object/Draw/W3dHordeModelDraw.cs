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
                { "StaticModelLODMode", (parser, x) => x.StaticModelLODMode = parser.ParseBoolean() },
                { "LodOptions", (parser, x) => x.LODOptions.Add(LODOption.Parse(parser)) }
            });

        public bool StaticModelLODMode { get; internal set; }
        public List<LODOption> LODOptions { get; internal set; } = new List<LODOption>();
    }

    public sealed class LODOption
    {
        internal static LODOption Parse(IniParser parser)
        {
            var lod = parser.ParseEnum<ModelLevelOfDetail>();
            var result = parser.ParseBlock(FieldParseTable);
            result.LOD = lod;
            return result;
        }

        internal static readonly IniParseTable<LODOption> FieldParseTable = new IniParseTable<LODOption>
            {
                { "AllowMultipleModels", (parser, x) => x.AllowMultipleModels = parser.ParseBoolean() },
                { "MaxRandomTextures", (parser, x) => x.MaxRandomTextures = parser.ParseInteger() },
                { "MaxRandomAnimations", (parser, x) => x.MaxRandomAnimations = parser.ParseInteger() },
                { "MaxAnimFrameDelta", (parser, x) => x.MaxAnimFrameDelta = parser.ParseInteger() }
            };

        public ModelLevelOfDetail LOD { get; internal set; }

        public bool AllowMultipleModels { get; internal set; }
        public int MaxRandomTextures { get; internal set; }
        public int MaxRandomAnimations { get; internal set; }
        public int MaxAnimFrameDelta { get; internal set; }
    }
}
