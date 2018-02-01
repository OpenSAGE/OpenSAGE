using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using OpenSage.Mathematics;

namespace OpenSage.Data.Apt.Characters
{

    [Flags]
    public enum ButtonRecordFlags : uint
    {
        StateUp = 1,
        StateOver = 2,
        StateDown = 4,
        StateHit = 8
    }

    public struct ButtonRecord
    {
        public ButtonRecordFlags Flags;
        public uint Character;
        public int Depth;
        public Matrix2x2 RotScale;
        public Vector2 Translation;
        public ColorRgba Color;
        public Vector4 Unknown;

        public ButtonRecord(ButtonRecordFlags flags, uint character, int depth, Matrix2x2 rotscale,
            Vector2 translation, ColorRgba color, Vector4 unknown)
        {
            Flags = flags;
            Character = character;
            Depth = depth;
            RotScale = rotscale;
            Translation = translation;
            Color = color;
            Unknown = unknown;
        }

        public static ButtonRecord Parse(BinaryReader reader)
        {
            return new ButtonRecord(
                reader.ReadUInt32AsEnumFlags<ButtonRecordFlags>(),
                reader.ReadUInt32(),
                reader.ReadInt32(),
                reader.ReadMatrix2x2(),
                reader.ReadVector2(),
                reader.ReadColorRgba(),
                reader.ReadVector4()
                );
        }
    }

    [Flags]
    public enum ButtonActionFlags : byte
    {
        IdleToOverUp = 1,
        OverUpToIdle = 2,
        OverUpToOverDown = 4,
        OverDownToOverUp = 8,
        OverDownToOutDown = 16,
        OutDownToOverDown = 32,
        IdleToOverDown = 64,
        OverDownToIdle = 128
    }

    public enum ButtonInput : ushort
    {
        MouseButton0 = 0,
        Left = 1,
        Right = 2,
        Home = 3,
        End = 4,
        Insert = 5,
        Delete = 6,
        BackSpace = 8,
        Enter = 13,
        Unknown = 240,
    }

    public struct ButtonAction
    {
        public ButtonActionFlags Flags;
        public ButtonInput KeyCode;
        public Byte Reserved;
        public InstructionCollection Instructions;

        public ButtonAction(ButtonActionFlags flags, ButtonInput input, Byte res,
            InstructionCollection instructions)
        {
            Flags = flags;
            KeyCode = input;
            Reserved = res;
            Instructions = instructions;
        }

        public static ButtonAction Parse(BinaryReader reader)
        {
            var flags = reader.ReadByteAsEnumFlags<ButtonActionFlags>();
            var input = reader.ReadUInt16AsEnum<ButtonInput>();
            var reserved = reader.ReadByte();
            var instructions = new InstructionCollection(reader.BaseStream);
            instructions.Parse();

            return new ButtonAction(flags, input, reserved, instructions);
        }
    }

    public sealed class Button : Character
    {
        public Vector4 Bounds { get; private set; }
        public IndexedTriangle[] Triangles { get; private set; }
        public Vector2[] Vertices { get; private set; }
        public bool IsMenu { get; private set; }
        public List<ButtonRecord> Records { get; private set; }
        public List<ButtonAction> Actions { get; private set; }

        public static Button Parse(BinaryReader reader)
        {
            var button = new Button();

            button.IsMenu = reader.ReadBooleanUInt32Checked();
            button.Bounds = reader.ReadVector4();
            var tc = reader.ReadUInt32();
            var vc = reader.ReadUInt32();
            button.Vertices = reader.ReadFixedSizeArrayAtOffset<Vector2>(() => reader.ReadVector2(), vc);
            button.Triangles = reader.ReadFixedSizeArrayAtOffset<IndexedTriangle>(() => reader.ReadIndexedTri(), tc);

            //TODO: read actionscript related stuff and buttonrecords
            button.Records = reader.ReadListAtOffset<ButtonRecord>(() => ButtonRecord.Parse(reader));

            button.Actions = reader.ReadListAtOffset<ButtonAction>(() => ButtonAction.Parse(reader));
            return button;
        }
    }
}
