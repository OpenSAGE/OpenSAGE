using System.IO;
using OpenSage.Data.StreamFS;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3x
{
    public sealed class FXShaderMaterial
    {
        public string ShaderName { get; private set; }
        public string TechniqueName { get; private set; }
        public FXShaderConstant[] Constants { get; private set; }
        public uint TechniqueIndex { get; private set; }

        internal static FXShaderMaterial Parse(BinaryReader reader, AssetImportCollection imports)
        {
            return new FXShaderMaterial
            {
                ShaderName = reader.ReadUInt32PrefixedAsciiStringAtOffset(),
                TechniqueName = reader.ReadUInt32PrefixedAsciiStringAtOffset(),

                Constants = reader.ReadArrayAtOffset(() => FXShaderConstant.Parse(reader, imports), true),

                TechniqueIndex = reader.ReadUInt32()
            };
        }
    }
}
