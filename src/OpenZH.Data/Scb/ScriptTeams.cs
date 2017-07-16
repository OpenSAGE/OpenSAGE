using System.Collections.Generic;
using System.IO;
using OpenZH.Data.Map;

namespace OpenZH.Data.Scb
{
    public sealed class ScriptTeams : Asset
    {
        public const string AssetName = "ScriptTeams";

        public Team[] Teams { get; private set; }

        public static ScriptTeams Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var teams = new List<Team>();

                while (reader.BaseStream.Position < context.CurrentEndPosition)
                {
                    teams.Add(Team.Parse(reader, context));
                }

                return new ScriptTeams
                {
                    Teams = teams.ToArray()
                };
            });
        }

        public void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                foreach (var team in Teams)
                {
                    team.WriteTo(writer, assetNames);
                }
            });
        }
    }
}
