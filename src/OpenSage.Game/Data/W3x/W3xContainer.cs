using System.IO;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;

namespace OpenSage.Data.W3x
{
    public sealed class W3xContainer
    {
        public W3xHierarchy Hierarchy { get; private set; }
        public W3xSubObject[] SubObjects { get; private set; }

        internal static W3xContainer Parse(BinaryReader reader, AssetImportCollection imports)
        {
            return new W3xContainer
            {
                Hierarchy = imports.GetImportedData<W3xHierarchy>(reader),
                SubObjects = reader.ReadArrayAtOffset(() => W3xSubObject.Parse(reader, imports))
            };
        }
    }

    public sealed class W3xSubObject
    {
        public uint BoneIndex { get; private set; }
        public string Name { get; private set; }
        public W3xRenderObject RenderObject { get; private set; }

        internal static W3xSubObject Parse(BinaryReader reader, AssetImportCollection imports)
        {
            return new W3xSubObject
            {
                BoneIndex = reader.ReadUInt32(),
                Name = reader.ReadUInt32PrefixedAsciiStringAtOffset(),
                RenderObject = imports.GetImportedData<W3xRenderObject>(reader)
            };
        }
    }

    public abstract class W3xRenderObject
    {

    }
}
