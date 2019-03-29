using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using ValueType = OpenSage.Gui.Apt.ActionScript.ValueType;

namespace OpenSage.Tools.AptEditor.Apt.Editor
{
    internal class LogicalDestination : InstructionBase
    {
        public override InstructionType Type => throw new InvalidOperationException();
        public override void Execute(ActionContext context) => throw new InvalidOperationException();
        public string Name { get; set; }
        public LogicalBranch LogicalBranch { get; private set; }

        public LogicalDestination(LogicalBranch logicalBranch, string labelName)
        {
            LogicalBranch = logicalBranch;
            Name = labelName;
        }
    }

    internal class LogicalEndOfFunction : InstructionBase
    {
        public override InstructionType Type => throw new InvalidOperationException();
        public override void Execute(ActionContext context) => throw new InvalidOperationException();
        public LogicalDefineFunction LogicalDefineFunction { get; private set; }

        public LogicalEndOfFunction(LogicalDefineFunction logicalDefineFunction)
        {
            LogicalDefineFunction = logicalDefineFunction;
        }
    }

    internal class LogicalBranch : InstructionBase
    {
        public override InstructionType Type => InnerInstruction.Type;
        public override void Execute(ActionContext context) => throw new InvalidOperationException();
        public override List<Value> Parameters => InnerInstruction.Parameters;
        public LogicalDestination Destination { get; private set; }
        public InstructionBase InnerInstruction { get; private set; }

        public LogicalBranch(InstructionBase instruction)
        {
            InnerInstruction = instruction;
            Destination = null;
        }

        public void SetDestination(LogicalDestination destination)
        {
            if(Destination != null)
            {
                throw new InvalidOperationException();
            }
            Destination = destination;
        }
    }

    internal class LogicalDefineFunction : InstructionBase
    {
        public override InstructionType Type => InnerInstruction.Type;
        public override void Execute(ActionContext context) => throw new InvalidOperationException();
        public override List<Value> Parameters => InnerInstruction.Parameters;
        public LogicalEndOfFunction EndOfFunction { get; private set; }
        public InstructionBase InnerInstruction { get; private set; }

        public LogicalDefineFunction(InstructionBase instruction)
        {
            InnerInstruction = instruction;
            EndOfFunction = null;
        }

        public void SetEndOfFunction(LogicalEndOfFunction endOfFunction)
        {
            if(EndOfFunction != null)
            {
                throw new InvalidOperationException();
            }
            EndOfFunction = endOfFunction;
        }
    }

    internal class LogicalInstructions
    {
        public List<InstructionBase> Items { get; set; }
        public Dictionary<string, LogicalDestination> BranchDestinations { get; set; }

        public LogicalInstructions(InstructionCollection existingCollection)
        {
            var destinationAndBranches = new Dictionary<int, List<LogicalBranch>>();
            var endFunctionAndDefineFunctions = new Dictionary<int, LogicalDefineFunction>();

            // First pass: create deep copy of instructions, and fill defineFunctions and branches
            var firstPassProcessed = new List<(int, InstructionBase)>();
            /// Sometimes we need to know the end address of current instruction, 
            /// which is the start address of next instruction
            var tasksForNextStep = new Stack<Action<int>>();
            foreach(var (position, instruction) in existingCollection.GetPositionedInstructions())
            {
                while(tasksForNextStep.Any())
                {
                    tasksForNextStep.Pop()(position);
                }

                var instructionToBeAdded = DeepCopyInstruction(instruction);
                switch(instructionToBeAdded)
                {
                    case BranchAlways _:
                    case BranchIfTrue _:
                        var logicalBranch = new LogicalBranch(instructionToBeAdded);
                        instructionToBeAdded = logicalBranch;

                        var offset = instructionToBeAdded.Parameters.First().ToInteger();
                        tasksForNextStep.Push(nextPosition => {
                            var key = nextPosition + offset;
                            if(!destinationAndBranches.ContainsKey(key))
                            {
                                destinationAndBranches[key] = new List<LogicalBranch>();
                            }
                            
                            destinationAndBranches[key].Add(logicalBranch);
                        });
                        break;
                    case DefineFunction _:
                    case DefineFunction2 _:
                        var logicalDefineFunction = new LogicalDefineFunction(instructionToBeAdded);
                        instructionToBeAdded = logicalDefineFunction;

                        var bodySize = instructionToBeAdded.Parameters.Last().ToInteger();
                        tasksForNextStep.Push(nextPosition => {
                            endFunctionAndDefineFunctions[nextPosition + bodySize] = logicalDefineFunction;
                        });
                        break;
                }

                firstPassProcessed.Add((position, instructionToBeAdded));
            }

            // Second pass: create LogicalDestinations
            var labelIndex = 0;
            Items = new List<InstructionBase>();
            foreach(var (position, instruction) in firstPassProcessed)
            {
                if(endFunctionAndDefineFunctions.ContainsKey(position))
                {
                    var logicalDefineFunction = endFunctionAndDefineFunctions[position];
                    logicalDefineFunction.SetEndOfFunction(new LogicalEndOfFunction(logicalDefineFunction));
                    Items.Add(logicalDefineFunction.EndOfFunction);
                }

                if(destinationAndBranches.ContainsKey(position))
                {
                    
                    foreach(var logicalBranch in destinationAndBranches[position])
                    {
                        logicalBranch.SetDestination(new LogicalDestination(logicalBranch, $"location{++labelIndex}"));
                        Items.Add(logicalBranch.Destination);
                    }
                }

                Items.Add(instruction);
            }
        }

        private static InstructionBase DeepCopyInstruction(InstructionBase instruction)
        {
            var copy = (InstructionBase)Activator.CreateInstance(instruction.GetType());
            copy.Parameters = instruction.Parameters.Select(value => DeepInstructionParameters(value)).ToList();
            return copy;
        }

        private static Value DeepInstructionParameters(Value existing)
        {
            switch(existing.Type)
            {
                case ValueType.Constant:
                    return Value.FromConstant(existing.GetIDValue());
                case ValueType.Float:
                    return Value.FromFloat((float)existing.ToReal());
                case ValueType.Integer:
                    return Value.FromInteger(existing.ToInteger());
                case ValueType.Register:
                    return Value.FromRegister(existing.GetIDValue());
                case ValueType.String:
                    return Value.FromString(existing.ToString());
                default:
                    throw new InvalidOperationException();
            }
        }

        private int IndexOfNextRealInstruction(int currentIndex)
        {
            for (var i = currentIndex + 1; i < Items.Count; ++i)
            {
                switch(Items[i])
                {
                    case LogicalDestination _:
                    case LogicalEndOfFunction _:
                        continue;
                    default:
                        return i;
                }
            }
            throw new IndexOutOfRangeException();
        }

        public InstructionCollection ConvertToRealInstructions()
        {
            // A pretty stupid method...
            var positionOfBranches = new Dictionary<LogicalBranch, int>();
            var positionOfDefineFunctions = new Dictionary<LogicalDefineFunction, int>();
            var sortedList = new SortedList<int, InstructionBase>();
            for (var i = 0; i < Items.Count; ++i)
            {
                var current = Items[i];
                switch(current)
                {
                    case LogicalDestination logicalDestination:
                        {
                            var sourceIndex = positionOfBranches[logicalDestination.LogicalBranch];
                            var sourceNext = IndexOfNextRealInstruction(sourceIndex);
                            var destination = IndexOfNextRealInstruction(i);
                            var offset = destination - sourceNext;
                            var parameters = logicalDestination.LogicalBranch.InnerInstruction.Parameters;
                            parameters[0] = Value.FromInteger(offset);
                        }
                        break;
                    case LogicalEndOfFunction logicalEndOfFunction:
                        {
                            var sourceIndex = positionOfDefineFunctions[logicalEndOfFunction.LogicalDefineFunction];
                            var sourceNext = IndexOfNextRealInstruction(sourceIndex);
                            var destination = IndexOfNextRealInstruction(i);
                            var size = destination - sourceNext;
                            var parameters = logicalEndOfFunction.LogicalDefineFunction.InnerInstruction.Parameters;
                            parameters[parameters.Count - 1] = Value.FromInteger(size);
                        }
                        break;
                    case LogicalBranch logicalBranch:
                        positionOfBranches.Add(logicalBranch, i);
                        sortedList.Add(i, logicalBranch.InnerInstruction);
                        break;
                    case LogicalDefineFunction defineFunction:
                        positionOfDefineFunctions.Add(defineFunction, i);
                        sortedList.Add(i, defineFunction.InnerInstruction);
                        break;
                    default:
                        sortedList.Add(i, current);
                        break;
                }
            }
            return new InstructionCollection(sortedList);
        }
    }
}