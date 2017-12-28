using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class MPPositionList : Asset
    {
        public const string AssetName = "MPPositionList";

        public MPPositionInfo[] Positions { get; private set; }

        internal static MPPositionList Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var positions = new List<MPPositionInfo>();

                ParseAssets(reader, context, assetName =>
                {
                    if (assetName != MPPositionInfo.AssetName)
                    {
                        throw new InvalidDataException();
                    }

                    positions.Add(MPPositionInfo.Parse(reader, context));
                });

                return new MPPositionList
                {
                    Positions = positions.ToArray()
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                foreach (var position in Positions)
                {
                    writer.Write(assetNames.GetOrCreateAssetIndex(MPPositionInfo.AssetName));
                    position.WriteTo(writer);
                }
            });
        }
    }
}
