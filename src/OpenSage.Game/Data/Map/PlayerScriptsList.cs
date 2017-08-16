using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class PlayerScriptsList : Asset
    {
        public const string AssetName = "PlayerScriptsList";

        public ScriptList[] ScriptLists { get; private set; }

        internal static PlayerScriptsList Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var scriptLists = new List<ScriptList>();

                ParseAssets(reader, context, assetName =>
                {
                    if (assetName != ScriptList.AssetName)
                    {
                        throw new InvalidDataException();
                    }

                    scriptLists.Add(ScriptList.Parse(reader, context));
                });

                return new PlayerScriptsList
                {
                    ScriptLists = scriptLists.ToArray()
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                foreach (var scriptList in ScriptLists)
                {
                    writer.Write(assetNames.GetOrCreateAssetIndex(ScriptList.AssetName));
                    scriptList.WriteTo(writer, assetNames);
                }
            });
        }
    }
}
