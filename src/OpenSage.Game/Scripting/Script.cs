using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.Data.Map;
using OpenSage.FileFormats;

namespace OpenSage.Scripting
{
    public sealed class Script : Asset
    {
        public const string AssetName = "Script";

        private uint _framesUntilNextEvaluation;

        public string Name { get; private set; }

        public string Comment { get; private set; }
        public string ConditionsComment { get; private set; }
        public string ActionsComment { get; private set; }

        public bool IsActive { get; set; } // TODO: Make this private.
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

        /// <summary>
        /// True if script uses of the new evaluation interval types:
        /// every X Operations, Move Forces, etc.
        /// </summary>
        [AddedIn(SageGame.Cnc3)]
        public bool UsesEvaluationIntervalType { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public EvaluationIntervalType EvaluationIntervalType { get; private set; }

        public bool ActionsFireSequentially { get; private set; }
        public bool LoopActions { get; private set; }
        public int LoopCount { get; private set; }
        public SequentialScriptTarget SequentialTargetType { get; private set; }
        public string SequentialTargetName { get; private set; }

        public string Unknown { get; private set; }

        [AddedIn(SageGame.Ra3Uprising)]
        public int Unknown2 { get; private set; }

        [AddedIn(SageGame.Ra3Uprising)]
        public ushort Unknown3 { get; private set; }

        public ScriptOrCondition[] OrConditions { get; private set; }

        public ScriptAction[] ActionsIfTrue { get; private set; }
        public ScriptAction[] ActionsIfFalse { get; private set; }

        public void Execute(ScriptExecutionContext context)
        {
            var shouldExecute =
                !IsSubroutine &&
                IsActive &&
                (EvaluationInterval == 0 || _framesUntilNextEvaluation == 0);

            if (EvaluationInterval != 0)
            {
                // TODO: Is there an off-by-one error here?
                _framesUntilNextEvaluation = _framesUntilNextEvaluation == 0
                    ? EvaluationInterval * context.Scripting.TickRate
                    : _framesUntilNextEvaluation--;
            }

            if (!shouldExecute)
            {
                return;
            }

            RunActions(context);
        }

        public void ExecuteAsSubroutine(ScriptExecutionContext context)
        {
            // Note: _evaluationInterval is checked here for compatiblity.
            // See Systems >> Scripting >> Subroutines in OpenSAGE docs for more information.
            var shouldExecute = IsSubroutine && IsActive && EvaluationInterval == 0;

            if (shouldExecute)
            {
                RunActions(context);
            }
        }

        private void RunActions(ScriptExecutionContext context)
        {
            var conditionValue = EvaluateConditions(context);

            var actions = conditionValue
                ? ActionsIfTrue
                : ActionsIfFalse;

            foreach (var action in actions)
            {
                ScriptActions.Execute(context, action);
            }

            var shouldDeactivate = DeactivateUponSuccess && actions.Length > 0;

            if (shouldDeactivate)
            {
                IsActive = false;
            }
        }

        private bool EvaluateConditions(ScriptExecutionContext context)
        {
            bool AllConditionsTrue(ScriptCondition[] conditions)
            {
                foreach (var condition in conditions)
                {
                    var result = ScriptConditions.Evaluate(context, condition);
                    if (!result)
                    {
                        return false;
                    }
                }

                return true;
            }

            foreach (var orCondition in OrConditions)
            {
                var result = AllConditionsTrue(orCondition.Conditions);
                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        internal static Script Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var result = new Script
                {
                    Name = reader.ReadUInt16PrefixedAsciiString(),

                    Comment = reader.ReadUInt16PrefixedAsciiString(),
                    ConditionsComment = reader.ReadUInt16PrefixedAsciiString(),
                    ActionsComment = reader.ReadUInt16PrefixedAsciiString(),

                    IsActive = reader.ReadBooleanChecked(),
                    DeactivateUponSuccess = reader.ReadBooleanChecked(),

                    ActiveInEasy = reader.ReadBooleanChecked(),
                    ActiveInMedium = reader.ReadBooleanChecked(),
                    ActiveInHard = reader.ReadBooleanChecked(),

                    IsSubroutine = reader.ReadBooleanChecked()
                };

                if (version >= 2)
                {
                    result.EvaluationInterval = reader.ReadUInt32();

                    if (version == 5)
                    {
                        result.UsesEvaluationIntervalType = reader.ReadBooleanChecked();

                        result.EvaluationIntervalType = reader.ReadUInt32AsEnum<EvaluationIntervalType>();
                    }
                    else
                    {
                        result.EvaluationIntervalType = EvaluationIntervalType.FrameOrSeconds;
                    }
                }

                if (version >= 3)
                {
                    result.ActionsFireSequentially = reader.ReadBooleanChecked();

                    result.LoopActions = reader.ReadBooleanChecked();

                    result.LoopCount = reader.ReadInt32();

                    result.SequentialTargetType = reader.ReadByteAsEnum<SequentialScriptTarget>();

                    result.SequentialTargetName = reader.ReadUInt16PrefixedAsciiString();
                }

                if (version >= 4)
                {
                    result.Unknown = reader.ReadUInt16PrefixedAsciiString();
                    if (result.Unknown != "ALL" && result.Unknown != "Planning" && result.Unknown != "X")
                    {
                        throw new InvalidDataException();
                    }
                }

                if (version >= 6)
                {
                    result.Unknown2 = reader.ReadInt32();

                    result.Unknown3 = reader.ReadUInt16();
                    if (result.Unknown3 != 0)
                    {
                        throw new InvalidDataException();
                    }
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

                result.OrConditions = orConditions.ToArray();
                result.ActionsIfTrue = actionsIfTrue.ToArray();
                result.ActionsIfFalse = actionsIfFalse.ToArray();

                return result;
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

                if (Version >= 2)
                {
                    writer.Write(EvaluationInterval);

                    if (Version == 5)
                    {
                        writer.Write(UsesEvaluationIntervalType);
                        writer.Write((uint) EvaluationIntervalType);
                    }
                }

                if (Version >= 3)
                {
                    writer.Write(ActionsFireSequentially);
                    writer.Write(LoopActions);
                    writer.Write(LoopCount);
                    writer.Write((byte) SequentialTargetType);
                    writer.WriteUInt16PrefixedAsciiString(SequentialTargetName);
                }

                if (Version >= 4)
                {
                    writer.WriteUInt16PrefixedAsciiString(Unknown);
                }

                if (Version >= 6)
                {
                    writer.Write(Unknown2);
                    writer.Write(Unknown3);
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

        internal void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            IsActive = reader.ReadBooleanChecked();
        }

        public Script Copy(string appendix)
        {
            return new Script()
            {
                ActionsComment = ActionsComment,
                ActionsFireSequentially = ActionsFireSequentially,
                ActionsIfFalse = ActionsIfFalse.Select(a => a.Copy(appendix)).ToArray(),
                ActionsIfTrue = ActionsIfTrue.Select(a => a.Copy(appendix)).ToArray(),
                ActiveInEasy = ActiveInEasy,
                ActiveInHard = ActiveInHard,
                ActiveInMedium = ActiveInMedium,
                Comment = Comment,
                ConditionsComment = ConditionsComment,
                DeactivateUponSuccess = DeactivateUponSuccess,
                EvaluationInterval = EvaluationInterval,
                EvaluationIntervalType = EvaluationIntervalType,
                IsActive = IsActive,
                IsSubroutine = IsSubroutine,
                LoopActions = LoopActions,
                LoopCount = LoopCount,
                Name = Name + appendix,
                OrConditions = OrConditions.Select(c => c.Copy(appendix)).ToArray(),
                SequentialTargetName = SequentialTargetName, // TODO: appendix?
                SequentialTargetType = SequentialTargetType,
                Unknown = Unknown,
                Unknown2 = Unknown2,
                Unknown3 = Unknown3,
                UsesEvaluationIntervalType = UsesEvaluationIntervalType,
            };
        }
    }

    public enum SequentialScriptTarget
    {
        Team = 0,
        Unit = 1
    }

    public enum EvaluationIntervalType
    {
        Operation = 0,
        MoveForces = 1,
        Battle = 2,
        Upkeep = 3,
        Complete = 4,
        Any = 5,
        FrameOrSeconds = 6
    }
}
