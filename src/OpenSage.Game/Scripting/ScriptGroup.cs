﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data.Map;
using OpenSage.FileFormats;

namespace OpenSage.Scripting
{
    public sealed class ScriptGroup : Asset, IPersistableObject
    {
        public const string AssetName = "ScriptGroup";

        private bool _isActive;

        public string Name { get; private set; }
        public bool IsActive => _isActive;
        public bool IsSubroutine { get; private set; }
        public Script[] Scripts { get; private set; }

        public ScriptGroup[] Groups { get; private set; }

        public void Execute(ScriptExecutionContext context)
        {
            if (IsSubroutine || !IsActive)
            {
                return;
            }

            foreach (var script in Scripts)
            {
                script.Execute(context);
            }
        }

        public Script FindScript(string name)
        {
            // TODO: Use a dictionary?

            foreach (var script in Scripts)
            {
                if (script.Name == name)
                {
                    return script;
                }
            }

            foreach (var scriptGroup in Groups)
            {
                var result = scriptGroup.FindScript(name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        internal static ScriptGroup Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var name = reader.ReadUInt16PrefixedAsciiString();
                var isActive = reader.ReadBooleanChecked();
                var isSubroutine = reader.ReadBooleanChecked();

                var scripts = new List<Script>();
                var groups = new List<ScriptGroup>();

                ParseAssets(reader, context, assetName =>
                {
                    switch (assetName)
                    {
                        case ScriptGroup.AssetName:
                            if (version < 3)
                            {
                                goto default;
                            }
                            groups.Add(ScriptGroup.Parse(reader, context));
                            break;

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
                    _isActive = isActive,
                    IsSubroutine = isSubroutine,
                    Scripts = scripts.ToArray(),
                    Groups = groups.ToArray()
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                writer.WriteUInt16PrefixedAsciiString(Name);
                writer.Write(IsActive);
                writer.Write(IsSubroutine);

                foreach (var scriptGroup in Groups)
                {
                    writer.Write(assetNames.GetOrCreateAssetIndex(ScriptGroup.AssetName));
                    scriptGroup.WriteTo(writer, assetNames);
                }

                foreach (var script in Scripts)
                {
                    writer.Write(assetNames.GetOrCreateAssetIndex(Script.AssetName));
                    script.WriteTo(writer, assetNames);
                }
            });
        }

        public void Persist(StatePersister reader)
        {
            var version = reader.PersistVersion(2);

            if (version >= 2)
            {
                reader.PersistBoolean(ref _isActive);
            }

            reader.PersistArrayWithUInt16Length(
                Scripts,
                static (StatePersister persister, ref Script item) =>
                {
                    persister.PersistObjectValue(item);
                });
        }

        public ScriptGroup Copy(string appendix)
        {
            return new ScriptGroup()
            {
                Groups = Groups.Select(g => g.Copy(appendix)).ToArray(),
                _isActive = _isActive,
                IsSubroutine = IsSubroutine,
                Name = Name + appendix,
                Scripts = Scripts.Select(s => s.Copy(appendix)).ToArray()
            };
        }
    }
}
