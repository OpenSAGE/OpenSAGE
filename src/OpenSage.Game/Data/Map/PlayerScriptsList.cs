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

        internal void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistArrayWithUInt32Length("ScriptLists", ScriptLists, static (StatePersister persister, ref ScriptList item) =>
            {
                persister.BeginObject();

                var hasScripts = item.Scripts.Length > 0;
                persister.PersistBoolean("HasScripts", ref hasScripts);

                if (hasScripts)
                {
                    persister.PersistObject("ScriptList", item);
                }

                persister.EndObject();
            });
        }
    }
}
