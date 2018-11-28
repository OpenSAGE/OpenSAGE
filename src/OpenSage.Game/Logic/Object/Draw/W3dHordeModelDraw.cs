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
                { "LodOptions", (parser, x) => x.LodOptions.Add(LodOption.Parse(parser)) },
                { "WadingParticleSys", (parser, x) => x.WadingParticleSys = parser.ParseAssetReference() }
            });

        public bool StaticModelLodMode { get; private set; }
        public List<LodOption> LodOptions { get; private set; } = new List<LodOption>();
        public string WadingParticleSys { get; private set; } 
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
        public int MaxRandomAnimations { get; private set; }
        public int MaxAnimFrameDelta { get; private set; }
    }
}
