using System.IO;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;
using OpenSage.Graphics;

namespace OpenSage.Data.W3x
{
    public sealed class W3xSubObject
    {
        public uint BoneIndex { get; private set; }
        public string Name { get; private set; }
        public ModelMesh RenderObject { get; private set; } // TODO: Could also be W3xBox

        internal static W3xSubObject Parse(BinaryReader reader, AssetImportCollection imports)
        {
            return new W3xSubObject
            {
                BoneIndex = reader.ReadUInt32(),
                Name = reader.ReadUInt32PrefixedAsciiStringAtOffset(),
                RenderObject = imports.GetImportedData<ModelMesh>(reader).Value
            };
        }
    }
}
