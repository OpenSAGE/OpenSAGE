using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class AssetPropertyCollection : Collection<AssetProperty>
    {
        public static AssetPropertyCollection Parse(BinaryReader reader, MapParseContext context)
        {
            var numProperties = reader.ReadUInt16();
            var result = new AssetProperty[numProperties];

            for (var i = 0; i < numProperties; i++)
            {
                result[i] = AssetProperty.Parse(reader, context);
            }

            return new AssetPropertyCollection(result);
        }

        public AssetPropertyCollection(IList<AssetProperty> list)
            : base(list)
        {

        }

        public void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            writer.Write((ushort) Count);

            foreach (var property in this)
            {
                property.WriteTo(writer, assetNames);
            }
        }
    }
}
