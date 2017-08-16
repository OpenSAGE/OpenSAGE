using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class Script : Asset
    {
        public const string AssetName = "Script";

        public string Name { get; private set; }

        public string Comment { get; private set; }
        public string ConditionsComment { get; private set; }
        public string ActionsComment { get; private set; }

        public bool IsActive { get; private set; }
        public bool DeactivateUponSuccess { get; private set; }

        public bool ActiveInEasy { get; private set; }
        public bool ActiveInMedium { get; private set; }
        public bool ActiveInHard { get; private set; }

        public bool IsSubroutine { get; private set; }

        /// <summary>
        /// How often the script should be evaluated, in seconds.
        /// If zero, script should be evaluated every frame.
        /// </summary>
        public uint EvaluationInterval { get; private set; }

        public ScriptOrCondition[] OrConditions { get; private set; }

        public ScriptAction[] ActionsIfTrue { get; private set; }
        public ScriptAction[] ActionsIfFalse { get; private set; }

        internal static Script Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                if (version != 1 && version != 2)
                {
                    throw new InvalidDataException();
                }

                var name = reader.ReadUInt16PrefixedAsciiString();

                var comment = reader.ReadUInt16PrefixedAsciiString();
                var conditionsComment = reader.ReadUInt16PrefixedAsciiString();
                var actionsComment = reader.ReadUInt16PrefixedAsciiString();

                var isActive = reader.ReadBooleanChecked();
                var deactivateUponSuccess = reader.ReadBooleanChecked();

                var activeInEasy = reader.ReadBooleanChecked();
                var activeInMedium = reader.ReadBooleanChecked();
                var activeInHard = reader.ReadBooleanChecked();

                var isSubroutine = reader.ReadBooleanChecked();

                var evaluationInterval = 0u;
                if (version > 1)
                {
                    evaluationInterval = reader.ReadUInt32();
                }

                var orConditions = new List<ScriptOrCondition>();
                var actionsIfTrue = new List<ScriptAction>();
                var actionsIfFalse = new List<ScriptAction>();

                ParseAssets(reader, context, assetName =>
                {
                    switch (assetName)
                    {
                        case ScriptOrCondition.AssetName:
                            orConditions.Add(ScriptOrCondition.Parse(reader, context));
                            break;

                        case ScriptAction.AssetNameTrue:
                            actionsIfTrue.Add(ScriptAction.Parse(reader, context));
                            break;

                        case ScriptAction.AssetNameFalse:
                            actionsIfFalse.Add(ScriptAction.Parse(reader, context));
                            break;

                        default:
                            throw new InvalidDataException($"Unexpected asset: {assetName}");
                    }
                });
                
                return new Script
                {
                    Name = name,
                    Comment = comment,
                    ConditionsComment = conditionsComment,
                    ActionsComment = actionsComment,
                    IsActive = isActive,
                    DeactivateUponSuccess = deactivateUponSuccess,
                    ActiveInEasy = activeInEasy,
                    ActiveInMedium = activeInMedium,
                    ActiveInHard = activeInHard,
                    IsSubroutine = isSubroutine,
                    EvaluationInterval = evaluationInterval,

                    OrConditions = orConditions.ToArray(),
                    ActionsIfTrue = actionsIfTrue.ToArray(),
                    ActionsIfFalse = actionsIfFalse.ToArray()
                };
            });
        }

        internal void WriteTo(BinaryWriter writer, AssetNameCollection assetNames)
        {
            WriteAssetTo(writer, () =>
            {
                writer.WriteUInt16PrefixedAsciiString(Name);

                writer.WriteUInt16PrefixedAsciiString(Comment);
                writer.WriteUInt16PrefixedAsciiString(ConditionsComment);
                writer.WriteUInt16PrefixedAsciiString(ActionsComment);

                writer.Write(IsActive);
                writer.Write(DeactivateUponSuccess);

                writer.Write(ActiveInEasy);
                writer.Write(ActiveInMedium);
                writer.Write(ActiveInHard);

                writer.Write(IsSubroutine);

                if (Version > 1)
                {
                    writer.Write(EvaluationInterval);
                }

                foreach (var orCondition in OrConditions)
                {
                    writer.Write(assetNames.GetOrCreateAssetIndex(ScriptOrCondition.AssetName));
                    orCondition.WriteTo(writer, assetNames);
                }

                foreach (var scriptAction in ActionsIfTrue)
                {
                    writer.Write(assetNames.GetOrCreateAssetIndex(ScriptAction.AssetNameTrue));
                    scriptAction.WriteTo(writer, assetNames);
                }

                foreach (var scriptAction in ActionsIfFalse)
                {
                    writer.Write(assetNames.GetOrCreateAssetIndex(ScriptAction.AssetNameFalse));
                    scriptAction.WriteTo(writer, assetNames);
                }
            });
        }
    }
}
