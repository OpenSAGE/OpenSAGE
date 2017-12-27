using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class LibraryMapLists : Asset
    {
        public const string AssetName = "LibraryMapLists";

        public LibraryMaps[] Lists { get; private set; }

        internal static LibraryMapLists Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var libraryMaps = new List<LibraryMaps>();

                ParseAssets(reader, context, assetName =>
                {
                    if (assetName != LibraryMaps.AssetName)
                    {
                        throw new InvalidDataException();
                    }

                    libraryMaps.Add(LibraryMaps.Parse(reader, context));
                });

                return new LibraryMapLists
                {
                    Lists = libraryMaps.ToArray()
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                foreach (var list in Lists)
                {
                    writer.Write(assetNames.GetOrCreateAssetIndex(LibraryMaps.AssetName));
                    list.WriteTo(writer, assetNames);
                }
            });
        }
    }
}
