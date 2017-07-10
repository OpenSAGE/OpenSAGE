using System.Collections.Generic;
using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class ScriptOrCondition : Asset
    {
        public ScriptCondition[] Conditions { get; private set; }

        public static ScriptOrCondition Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var conditions = new List<ScriptCondition>();

                ParseAssets(reader, context, assetName =>
                {
                    switch (assetName)
                    {
                        case "Condition":
                            conditions.Add(ScriptCondition.Parse(reader, context));
                            break;

                        default:
                            throw new InvalidDataException($"Unexpected asset: {assetName}");
                    }
                });

                return new ScriptOrCondition
                {
                    Conditions = conditions.ToArray()
                };
            });
        }
    }
}
