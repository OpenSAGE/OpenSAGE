using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class AssetPropertyCollection : KeyedCollection<string, AssetProperty>
    {
        internal static AssetPropertyCollection Parse(BinaryReader reader, MapParseContext context)
        {
            var numProperties = reader.ReadUInt16();
            var result = new AssetProperty[numProperties];

            for (var i = 0; i < numProperties; i++)
            {
                result[i] = AssetProperty.Parse(reader, context);
            }

            return new AssetPropertyCollection(result);
        }

        internal AssetPropertyCollection(IList<AssetProperty> list)
        {
            foreach (var property in list)
            {
                Add(property);
            }
        }

        protected override string GetKeyForItem(AssetProperty item)
        {
            return item.Key.Name;
        }

        // TODO: Remove this if we retarget the library to .NET Core 2.0
        public bool TryGetValue(string key, out AssetProperty value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        public AssetProperty GetPropOrNull(string key)
        {
            return TryGetValue(key, out var value) ? value : null;
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            writer.Write((ushort) Count);

            foreach (var property in this)
            {
                property.WriteTo(writer, assetNames);
            }
        }
    }
}
