using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAS2.Base;

namespace OpenAS2.Compilation
{
    public static class SyntaxParsingUtils
    {
        public static SyntaxNode Parse(NodePool pool, RawInstruction Instruction)
        {
            List<SNExpression> Expressions = new();
            var instruction = Instruction;
            if (Instruction is LogicalTaggedInstruction itag)
                instruction = itag.MostInner;
            // special process and overriding regular process
            var flagSpecialProc = true;
            switch (instruction.Type)
            {
                // type 1: peek but no pop
                case InstructionType.SetRegister:
                    Expressions.Add(pool.PopExpression(false));
                    break;
                case InstructionType.PushDuplicate:
                    Expressions.Add(pool.PopExpression(false));
                    break;

                // type 2: need to read args
                case InstructionType.InitArray:
                    Expressions.Add(pool.PopArray());
                    break;
                case InstructionType.ImplementsOp:
                case InstructionType.CallFunction:
                case InstructionType.EA_CallFuncPop:
                case InstructionType.NewObject:
                    Expressions.Add(pool.PopExpression());
                    Expressions.Add(pool.PopArray());
                    break;
                case InstructionType.CallMethod:
                case InstructionType.EA_CallMethod:
                case InstructionType.EA_CallMethodPop:
                case InstructionType.NewMethod:
                    Expressions.Add(pool.PopExpression());
                    Expressions.Add(pool.PopExpression());
                    Expressions.Add(pool.PopArray());
                    break;
                case InstructionType.EA_CallNamedFuncPop:
                case InstructionType.EA_CallNamedFunc:
                    Expressions.Add(new NodeValue(instruction));
                    Expressions.Add(pool.PopArray());
                    break;
                case InstructionType.EA_CallNamedMethodPop:
                    Expressions.Add(new NodeValue(instruction));
                    Expressions.Add(pool.PopExpression());
                    Expressions.Add(pool.PopArray());
                    break;
                case InstructionType.EA_CallNamedMethod:
                    Expressions.Add(new NodeValue(instruction));
                    Expressions.Add(pool.PopExpression());
                    Expressions.Add(pool.PopArray());
                    Expressions.Add(pool.PopExpression());
                    break;
                case InstructionType.InitObject:
                    Expressions.Add(pool.PopArray(true));
                    break;

                // type 3: constant resolve needed
                case InstructionType.EA_GetNamedMember:
                    Expressions.Add(new NodeValue(instruction));
                    Expressions.Add(pool.PopExpression());
                    break;
                case InstructionType.EA_PushValueOfVar:
                    Expressions.Add(new NodeValue(instruction));
                    break;
                case InstructionType.EA_PushGlobalVar:
                case InstructionType.EA_PushThisVar:
                case InstructionType.EA_PushGlobal:
                case InstructionType.EA_PushThis:
                    break; // nothing needed
                case InstructionType.EA_PushConstantByte:
                case InstructionType.EA_PushConstantWord:
                    Expressions.Add(new NodeValue(instruction));
                    break;
                case InstructionType.EA_PushRegister:
                    Expressions.Add(new NodeValue(instruction));
                    break;

                // type 4: variable output count
                case InstructionType.PushData:
                    // TODO
                    break;
                case InstructionType.Enumerate:
                    // TODO
                    break;
                case InstructionType.Enumerate2:
                    // TODO
                    break;

                // no hits
                default:
                    flagSpecialProc = false;
                    break;
            }
            if ((!flagSpecialProc) && instruction is InstructionEvaluable inst)
            {
                // TODO string output
                for (int i = 0; i < inst.StackPop; ++i)
                    Expressions.Add(pool.PopExpression());
            }
            else if (!flagSpecialProc) // not implemented instructions
            {
                throw new NotImplementedException(instruction.Type.ToString());
            }
        }
}
