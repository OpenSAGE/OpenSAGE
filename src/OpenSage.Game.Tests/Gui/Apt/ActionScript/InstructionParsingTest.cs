using System.Collections.Generic;
using System.IO;
using OpenSage.FileFormats.Apt;
using OpenAS2.Base;
using OpenSage.Gui.Apt;
using OpenAS2.Runtime;
using OpenAS2.Runtime.Dom.Default;
using Xunit;
using Xunit.Abstractions;
using static System.Text.Encoding;

namespace OpenSage.Tests.Gui.Apt.ActionScript
{
    static class Utilities
    {
        public static void Align(this BinaryWriter writer)
        {
            while (writer.BaseStream.Position % 4 != 0)
            {
                writer.Write((byte) 0);
            }
        }

        public static void WriteSimpleInstruction(this BinaryWriter writer, InstructionType type, int? argument = null)
        {
            writer.Write((byte) type);
            if (argument.HasValue)
            {
                if (Definition.IsAlignmentRequired(type))
                {
                    Align(writer);
                }
                writer.Write(argument.Value);
            }
        }

        public static int WriteNullTerminatedString(this BinaryWriter writer, string value)
        {
            writer.Align();
            var position = (int) writer.BaseStream.Position;
            writer.Write(UTF8.GetBytes(value + '\0'));
            return position;
        }
    }

    public class InstructionParsingTest
    {
        private readonly ITestOutputHelper _output;

        public InstructionParsingTest(ITestOutputHelper output)
        {
            _output = output;
        }

        // variableName should be parameter2
        // instruction count should be 19 instead of 20
        private Stream Get19SimpleInstructions(string variableName, string wrongValue, string rightValue, int afterInstructionPosition)
        {
            var instructionStream = new MemoryStream();
            using (var instructionWriter = new BinaryWriter(instructionStream, UTF8, true))
            {
                var variableNamePosition = instructionWriter.WriteNullTerminatedString(variableName);
                var wrongValuePosition = instructionWriter.WriteNullTerminatedString(wrongValue);
                var rightValuePosition = instructionWriter.WriteNullTerminatedString(rightValue);

                instructionWriter.Align();
                var instructionOffset = (uint) instructionStream.Position;
                instructionWriter.WriteSimpleInstruction(InstructionType.EA_PushOne);
                instructionWriter.WriteSimpleInstruction(InstructionType.EA_PushZero);
                instructionWriter.WriteSimpleInstruction(InstructionType.Add2);
                instructionWriter.WriteSimpleInstruction(InstructionType.BranchIfTrue, 18);
                instructionWriter.WriteSimpleInstruction(InstructionType.EA_PushString, variableNamePosition);
                instructionWriter.WriteSimpleInstruction(InstructionType.EA_PushString, wrongValuePosition);
                instructionWriter.WriteSimpleInstruction(InstructionType.SetVariable);
                instructionWriter.WriteSimpleInstruction(InstructionType.End);
                instructionWriter.WriteSimpleInstruction(InstructionType.EA_PushOne);
                instructionWriter.WriteSimpleInstruction(InstructionType.LogicalNot);
                instructionWriter.WriteSimpleInstruction(InstructionType.BranchIfTrue, 18);
                instructionWriter.WriteSimpleInstruction(InstructionType.EA_PushString, variableNamePosition);
                instructionWriter.WriteSimpleInstruction(InstructionType.EA_PushString, rightValuePosition);
                instructionWriter.WriteSimpleInstruction(InstructionType.SetVariable);
                instructionWriter.WriteSimpleInstruction(InstructionType.End);
                instructionWriter.WriteSimpleInstruction(InstructionType.EA_PushString, variableNamePosition);
                instructionWriter.WriteSimpleInstruction(InstructionType.EA_PushString, wrongValuePosition);
                instructionWriter.WriteSimpleInstruction(InstructionType.SetVariable);
                instructionWriter.WriteSimpleInstruction(InstructionType.End);

                // pathological case
                instructionWriter.WriteSimpleInstruction(InstructionType.BranchIfTrue, -79);

                var instructionOffsetPosition = (int) instructionStream.Position;
                instructionWriter.Write(instructionOffset);
                instructionWriter.Write(afterInstructionPosition);
                instructionWriter.Write("DummyData");

                instructionWriter.Seek(instructionOffsetPosition, SeekOrigin.Begin);
            }
            return instructionStream;
        }

        [Fact]
        public void ParseAndExecuteSimpleInstructions()
        {
            var magic = 123456789;
            var paramName = "name";
            var rightValue = "test2";
            var stream = Get19SimpleInstructions(paramName, "test1", rightValue, magic);
            var reader = new BinaryReader(stream, UTF8, true);
            var collection = InstructionStorage.Parse(stream, reader.ReadUInt32());
            // Assert that the last pathological branch instruction was not read
            Assert.True(collection.Count == 19);

            var afterInstructions = reader.ReadInt32();
            // Assert that after parsing instructions, stream will be sought back
            Assert.True(afterInstructions == magic);

            var vm = new VirtualMachine(new SimpleDomHandler());
            var context = new ESObject(vm);
            // collection, null, context, "Parsing Test"
            vm.EnqueueContext(vm.CreateContext(null, null, context, 4, null, collection.CreateStream()));
            vm.ExecuteUntilHalt();
            // Assert that during execution of instructions, the right value is set
            context.IGet(vm.GlobalContext, paramName).AddRecallCode(res =>
            {
                Assert.True(res.ToString().Equals(rightValue));
                return null;
            });
       
        }
    }
}
