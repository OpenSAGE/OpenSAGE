using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data.Map;

namespace OpenSage.Scripting
{
    public sealed class ScriptOrCondition : Asset
    {
        public const string AssetName = "OrCondition";

        public ScriptCondition[] Conditions { get; private set; }

        internal static ScriptOrCondition Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var conditions = new List<ScriptCondition>();

                ParseAssets(reader, context, assetName =>
                {
                    switch (assetName)
                    {
                        case ScriptCondition.AssetName:
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

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                foreach (var condition in Conditions)
                {
                    writer.Write(assetNames.GetOrCreateAssetIndex(ScriptCondition.AssetName));
                    condition.WriteTo(writer, assetNames);
                }
            });
        }

        public ScriptOrCondition Copy(string appendix)
        {
            return new ScriptOrCondition()
            {
                Conditions = Conditions.Select(c => c.Copy(appendix)).ToArray()
            };
        }
    }
}
