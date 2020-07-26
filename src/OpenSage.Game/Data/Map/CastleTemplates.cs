using System.IO;

namespace OpenSage.Data.Map
{
    [AddedIn(SageGame.Bfme)]
    public sealed class CastleTemplates : Asset
    {
        public const string AssetName = "CastleTemplates";

        public CastleTemplate[] Templates { get; private set; }

        internal static CastleTemplates Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numTemplates = reader.ReadByte();

                //TODO: figure out
                var missing = context.CurrentEndPosition - reader.BaseStream.Position;
                reader.ReadBytes((int)missing);

                return new CastleTemplates
                {
                    Templates = new CastleTemplate[numTemplates]
                };
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) Templates.Length);
            });
        }
    }
}
