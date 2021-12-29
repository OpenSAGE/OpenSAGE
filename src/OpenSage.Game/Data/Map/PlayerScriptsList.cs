using System.Collections.Generic;
using System.IO;
using OpenSage.Scripting;

namespace OpenSage.Data.Map
{
    public sealed class PlayerScriptsList : Asset
    {
        public const string AssetName = "PlayerScriptsList";

        public ScriptList[] ScriptLists { get; internal set; }

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

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var numSides = (uint)ScriptLists.Length;
            reader.ReadUInt32(ref numSides);
            if (numSides != ScriptLists.Length)
            {
                throw new InvalidStateException();
            }

            for (var i = 0; i < numSides; i++)
            {
                var hasScripts = ScriptLists.Length > 0;
                reader.ReadBoolean(ref hasScripts);

                if (hasScripts)
                {
                    ScriptLists[i].Load(reader);
                }
            }
        }
    }
}
