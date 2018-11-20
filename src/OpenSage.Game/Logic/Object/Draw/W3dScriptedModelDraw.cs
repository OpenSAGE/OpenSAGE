using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class W3dScriptedModelDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dScriptedModelDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dScriptedModelDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dScriptedModelDrawModuleData>
            {
                { "StaticModelLODMode", (parser, x) => x.StaticModelLODMode = parser.ParseBoolean() },
                { "ShowShadowWhileContained", (parser, x) => x.ShowShadowWhileContained = parser.ParseBoolean() },
                { "RandomTexture", (parser, x) => x.RandomTextures.Add(RandomTexture.Parse(parser)) },
                { "WallBoundsMesh", (parser, x) => x.WallBoundsMesh = parser.ParseAssetReference() },
                { "UseStandardModelNames", (parser, x) => x.UseStandardModelNames = parser.ParseBoolean() },
                { "RampMesh1", (parser, x) => x.RampMesh1 = parser.ParseAssetReference() },
                { "RampMesh2", (parser, x) => x.RampMesh2 = parser.ParseAssetReference() },
                { "MultiPlayerOnly", (parser, x) => x.MultiPlayerOnly = parser.ParseBoolean() },
            });

        public bool StaticModelLODMode { get; private set; }
        public bool ShowShadowWhileContained { get; private set; }
        public List<RandomTexture> RandomTextures { get; private set; } = new List<RandomTexture>();
        public string WallBoundsMesh { get; private set; }
        public bool UseStandardModelNames { get; private set; }
        public string RampMesh1 { get; private set; }
        public string RampMesh2 { get; private set; }
        public bool MultiPlayerOnly { get; private set; }
    }

    public sealed class RandomTexture
    {
        internal static RandomTexture Parse(IniParser parser)
        {
            var result = new RandomTexture();
            result.First = parser.ParseAssetReference();
            result.Unknown = parser.ParseInteger();
            if (result.Unknown != 0)
                throw new Exception();
            var second = parser.GetNextTokenOptional();
            if (second.HasValue)
            {
                result.Second = parser.ScanAssetReference(second.Value);
            }
            return result;
        }

        public string First { get; private set; }
        public int Unknown { get; private set; }
        public string Second { get; private set; }}
}
