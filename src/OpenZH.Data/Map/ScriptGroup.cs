using System.Collections.Generic;
using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class ScriptGroup : Asset
    {
        public const string AssetName = "ScriptGroup";

        public string Name { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsSubroutine { get; private set; }
        public Script[] Scripts { get; private set; }

        public static ScriptGroup Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                if (version != 2)
                {
                    throw new InvalidDataException();
                }

                var name = reader.ReadUInt16PrefixedAsciiString();
                var isActive = reader.ReadBoolean();
                var isSubroutine = reader.ReadBoolean();

                var scripts = new List<Script>();

                ParseAssets(reader, context, assetName =>
                {
                    switch (assetName)
                    {
                        case Script.AssetName:
                            scripts.Add(Script.Parse(reader, context));
                            break;

                        default:
                            throw new InvalidDataException($"Unexpected asset: {assetName}");
                    }
                });

                return new ScriptGroup
                {
                    Name = name,
                    IsActive = isActive,
                    IsSubroutine = isSubroutine,
                    Scripts = scripts.ToArray()
                };
            });
        }

        public void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                writer.WriteUInt16PrefixedAsciiString(Name);
                writer.Write(IsActive);
                writer.Write(IsSubroutine);

                foreach (var script in Scripts)
                {
                    writer.Write(assetNames.GetOrCreateAssetIndex(Script.AssetName));
                    script.WriteTo(writer, assetNames);
                }
            });
        }
    }
}
