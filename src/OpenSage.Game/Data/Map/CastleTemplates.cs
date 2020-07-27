using System.IO;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.Bfme)]
    public sealed class CastleTemplates : Asset
    {
        public const string AssetName = "CastleTemplates";

        public AssetPropertyKey PropertyKey { get; private set; }

        public CastleTemplate[] Templates { get; private set; }

        public CastlePerimeter Perimeter { get; private set; }

        internal static CastleTemplates Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var propertyKey = AssetPropertyKey.Parse(reader, context);

                var count = reader.ReadUInt32();
                var result = new CastleTemplate[count];

                for (var i = 0; i < count; i++)
                {
                    result[i] = CastleTemplate.Parse(reader, version);
                }

                CastlePerimeter perimeter = null;
                if (version >= 2)
                {
                    perimeter = CastlePerimeter.Parse(reader, version);
                }

                return new CastleTemplates
                {
                    PropertyKey = propertyKey,
                    Templates = result,
                    Perimeter = perimeter
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                PropertyKey.WriteTo(writer, assetNames);

                writer.Write((uint)Templates.Length);
                for (var i = 0; i < Templates.Length; i++)
                {
                    Templates[i].WriteTo(writer, Version);
                }

                if (Version >= 2)
                {
                    Perimeter.WriteTo(writer, Version);
                }
            });
        }
    }
}
