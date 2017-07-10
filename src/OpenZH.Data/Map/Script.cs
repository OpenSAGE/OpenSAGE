using System.Collections.Generic;
using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed partial class Script : Asset
    {
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

        public static Script Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                if (version != 2)
                {
                    throw new InvalidDataException();
                }

                var name = reader.ReadUInt16PrefixedAsciiString();

                var comment = reader.ReadUInt16PrefixedAsciiString();
                var conditionsComment = reader.ReadUInt16PrefixedAsciiString();
                var actionsComment = reader.ReadUInt16PrefixedAsciiString();

                var isActive = reader.ReadBoolean();
                var deactivateUponSuccess = reader.ReadBoolean();

                var activeInEasy = reader.ReadBoolean();
                var activeInMedium = reader.ReadBoolean();
                var activeInHard = reader.ReadBoolean();

                var isSubroutine = reader.ReadBoolean();

                var evaluationInterval = reader.ReadUInt32();

                var orConditions = new List<ScriptOrCondition>();
                var actionsIfTrue = new List<ScriptAction>();
                var actionsIfFalse = new List<ScriptAction>();

                ParseAssets(reader, context, assetName =>
                {
                    switch (assetName)
                    {
                        case "OrCondition":
                            orConditions.Add(ScriptOrCondition.Parse(reader, context));
                            break;

                        case "ScriptAction":
                            actionsIfTrue.Add(ScriptAction.Parse(reader, context));
                            break;

                        case "ScriptActionFalse":
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
    }
}
