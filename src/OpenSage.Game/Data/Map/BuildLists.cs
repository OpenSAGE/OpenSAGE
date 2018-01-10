using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class BuildLists : Asset
    {
        public const string AssetName = "BuildLists";

        public BuildList[] Items { get; private set; }

        internal static BuildLists Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numBuildLists = reader.ReadUInt32();
                var buildLists = new BuildList[numBuildLists];
                
                for (var i = 0; i < numBuildLists; i++)
                {
                    buildLists[i] = BuildList.Parse(reader, context, version);
                }

                return new BuildLists
                {
                    Items = buildLists
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) Items.Length);

                foreach (var buildList in Items)
                {
                    buildList.WriteTo(writer, assetNames, Version);
                }
            });
        }
    }
}
