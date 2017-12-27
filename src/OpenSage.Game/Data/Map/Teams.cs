using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class Teams : Asset
    {
        public const string AssetName = "Teams";

        public Team[] Items { get; private set; }

        internal static Teams Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var numTeams = reader.ReadUInt32();
                var teams = new Team[numTeams];

                for (var i = 0; i < numTeams; i++)
                {
                    teams[i] = Team.Parse(reader, context);
                }

                return new Teams
                {
                    Items = teams
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write((uint) Items.Length);

                foreach (var team in Items)
                {
                    team.WriteTo(writer, assetNames);
                }
            });
        }
    }
}
