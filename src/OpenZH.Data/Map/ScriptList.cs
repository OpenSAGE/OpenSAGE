using System.Collections.Generic;
using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class ScriptList : Asset
    {
        public ScriptGroup[] ScriptGroups { get; private set; }
        public Script[] Scripts { get; private set; }

        public static ScriptList Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                if (version != 1)
                {
                    throw new InvalidDataException();
                }

                var scriptGroups = new List<ScriptGroup>();
                var scripts = new List<Script>();

                ParseAssets(reader, context, assetName =>
                {
                    switch (assetName)
                    {
                        case "ScriptGroup":
                            scriptGroups.Add(ScriptGroup.Parse(reader, context));
                            break;

                        case "Script":
                            scripts.Add(Script.Parse(reader, context));
                            break;

                        default:
                            throw new InvalidDataException($"Unexpected asset: {assetName}");
                    }
                });
                
                return new ScriptList
                {
                    ScriptGroups = scriptGroups.ToArray(),
                    Scripts = scripts.ToArray()
                };
            });
        }
    }
}
