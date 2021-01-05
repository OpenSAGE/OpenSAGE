using System;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.FileFormats;
using OpenSage.Mathematics;
using OpenSage.Tools.AptEditor.Util;
using OpenSage.Utilities;

namespace OpenSage.Tools.AptEditor.Apt.Writer
{
    public static partial class DataWriter
    {
        static uint WriteImpl(MemoryPool memory, Button button)
        {
            var buttonMemory = memory.AllocatePadded(Constants.IntPtrSize * 16, Constants.IntPtrSize);
            using (var buttonWriter = GetWriter(buttonMemory))
            {
                buttonWriter.Write((UInt32)CharacterType.Button);
                buttonWriter.Write((UInt32)Character.SIGNATURE);
                buttonWriter.WriteBooleanUInt32(button.IsMenu);
                buttonWriter.Write((Vector4)button.Bounds);

                buttonWriter.Write((UInt32)button.Triangles.Length);
                buttonWriter.Write((UInt32)button.Vertices.Length);
                var verticesAddress = ArrayWriter.WritePlainArray(memory, button.Vertices);
                var trianglesAddress = ArrayWriter.WritePlainArray(memory, button.Triangles);
                buttonWriter.Write((UInt32)verticesAddress);
                buttonWriter.Write((UInt32)trianglesAddress);

                buttonWriter.Write((UInt32)button.Records.Count);
                var recordsAddress = ArrayWriter.WritePlainArray(memory, button.Records);
                buttonWriter.Write((UInt32)recordsAddress);

                buttonWriter.Write((UInt32)button.Actions.Count);
                var actionsAddress = ArrayWriter.WritePlainArray(memory, button.Actions);
                buttonWriter.Write((UInt32)actionsAddress);

                buttonWriter.Write((UInt32)0);
            }

            return buttonMemory.StartAddress;
        }

        static uint WriteImpl(MemoryPool memory, Font font)
        {
            var fontMemory = memory.AllocatePadded(Constants.IntPtrSize * 5, Constants.IntPtrSize);
            using (var fontWriter = GetWriter(fontMemory))
            {
                fontWriter.Write((UInt32)CharacterType.Font);
                fontWriter.Write((UInt32)Character.SIGNATURE);

                var nameAddress = Write(memory, font.Name);
                fontWriter.Write((UInt32)nameAddress);

                fontWriter.Write((UInt32)font.Glyphs.Count);
                var glyphsAddress = ArrayWriter.WritePlainArray(memory, font.Glyphs);
                fontWriter.Write((UInt32)glyphsAddress);
            }

            return fontMemory.StartAddress;
        }

        static uint WriteImpl(MemoryPool memory, Image image)
        {
            var imageMemory = memory.AllocatePadded(Constants.IntPtrSize * 3, Constants.IntPtrSize);
            using (var imageWriter = GetWriter(imageMemory))
            {
                imageWriter.Write((UInt32)CharacterType.Image);
                imageWriter.Write((UInt32)Character.SIGNATURE);
                imageWriter.Write((UInt32)image.TextureID);
            }

            return imageMemory.StartAddress;
        }

        static uint WriteImpl(MemoryPool memory, Movie movie)
        {
            var importsAddress = ArrayWriter.WritePlainArray(memory, movie.Imports);
            var exportsAddress = ArrayWriter.WritePlainArray(memory, movie.Exports);
            if(movie.Characters.First() != movie)
            {
                throw new ArgumentException("Shouldn't the first element of character array be movie itself?");
            }
            var characterArrayBegin = memory.AllocatePadded(Constants.IntPtrSize, Constants.IntPtrSize);
            var otherCharacters = movie.Characters.Skip(1).ToList();
            ArrayWriter.WriteArrayOfPointers(memory, otherCharacters);

            var movieMemory = memory.AllocatePadded(Constants.IntPtrSize * 15, Constants.IntPtrSize);
            using (var movieWriter = GetWriter(movieMemory))
            {
                movieWriter.Write((UInt32)CharacterType.Movie);
                movieWriter.Write((UInt32)Character.SIGNATURE);
                movieWriter.Write((UInt32)movie.Frames.Count);
                movieWriter.Write((UInt32)ArrayWriter.WritePlainArray(memory, movie.Frames));
                movieWriter.Write((UInt32)0); // pointer
                movieWriter.Write((UInt32)movie.Characters.Count);
                movieWriter.Write((UInt32)characterArrayBegin.StartAddress);
                movieWriter.Write((UInt32)movie.ScreenWidth);
                movieWriter.Write((UInt32)movie.ScreenHeight);
                movieWriter.Write((UInt32)movie.MillisecondsPerFrame);
                movieWriter.Write((UInt32)movie.Imports.Count);
                movieWriter.Write((UInt32)importsAddress);
                movieWriter.Write((UInt32)movie.Exports.Count);
                movieWriter.Write((UInt32)exportsAddress);
                movieWriter.Write((UInt32)0); // count
            }
            var entryOffset = movieMemory.StartAddress;
            
            BitConverter.GetBytes((UInt32)entryOffset).CopyTo(characterArrayBegin.Memory, 0);
            return entryOffset;
        }

        static uint WriteImpl(MemoryPool memory, Shape shape)
        {
            var shapeMemory = memory.AllocatePadded(Constants.IntPtrSize * 7, Constants.IntPtrSize);
            using (var shapeWriter = GetWriter(shapeMemory)) {
                shapeWriter.Write((UInt32)CharacterType.Shape);
                shapeWriter.Write((UInt32)Character.SIGNATURE);
                shapeWriter.Write((Vector4)shape.Bounds);
                shapeWriter.Write((UInt32)shape.Geometry);
            }
            return shapeMemory.StartAddress;
        }

        static uint WriteImpl(MemoryPool memory, Sprite sprite)
        {
            var spriteMemory = memory.AllocatePadded(Constants.IntPtrSize * 5, Constants.IntPtrSize);
            using (var spriteWriter = GetWriter(spriteMemory))
            {
                spriteWriter.Write((UInt32)CharacterType.Sprite);
                spriteWriter.Write((UInt32)Character.SIGNATURE);

                spriteWriter.Write((UInt32)sprite.Frames.Count);
                var framesAddress = ArrayWriter.WritePlainArray(memory, sprite.Frames);
                spriteWriter.Write((UInt32)framesAddress);

                spriteWriter.Write((UInt32)0); // pointer
            }
            return spriteMemory.StartAddress;
        }

        static uint WriteImpl(MemoryPool memory, Text text)
        {
            var textMemory = memory.AllocatePadded(Constants.IntPtrSize * 15, Constants.IntPtrSize);
            using (var textWriter = GetWriter(textMemory))
            {
                textWriter.Write((UInt32)CharacterType.Text);
                textWriter.Write((UInt32)Character.SIGNATURE);
                textWriter.Write((RectangleF)text.Bounds);
                textWriter.Write((UInt32)text.Font);
                textWriter.Write((UInt32)text.Alignment);
                textWriter.Write((ColorRgba)text.Color);
                textWriter.Write((Single)text.FontHeight);
                textWriter.WriteBooleanUInt32(text.ReadOnly);
                textWriter.WriteBooleanUInt32(text.Multiline);
                textWriter.WriteBooleanUInt32(text.WordWrap);

                var contentAddress = Write(memory, text.Content);
                textWriter.Write((UInt32)contentAddress);

                var textAddress = Write(memory, text.Value);
                textWriter.Write((UInt32)textAddress);
            }

            return textMemory.StartAddress;
        }
    }

    public static partial class PlainArrayElementWriter
    {
        public static uint GetElementSizeImpl(ButtonRecord record) => Constants.IntPtrSize * 17;
        public static void WriteInAllocatedMemoryImpl(BinaryWriter writer, MemoryPool memory, ButtonRecord record)
        {
            writer.Write((Byte)record.Flags);
            writer.WriteUInt24(0); // reserved
            writer.Write((UInt32)record.Character);
            writer.Write((Int32)record.Depth);
            writer.Write((Matrix2x2)record.RotScale);
            writer.Write((Vector2)record.Translation);
            writer.Write((ColorRgbaF)record.Color);
            writer.Write((Vector4)record.Unknown);
        }

        public static uint GetElementSizeImpl(ButtonAction action) => Constants.IntPtrSize * 2;
        public static void WriteInAllocatedMemoryImpl(BinaryWriter writer, MemoryPool memory, ButtonAction action)
        {
            writer.Write((Byte)action.Flags);
            writer.Write((UInt16)action.KeyCode);
            writer.Write((Byte)action.Reserved);

            var instructionsAddress = DataWriter.Write(memory, action.Instructions);
            writer.Write((UInt32)instructionsAddress);
        }

        public static uint GetElementSizeImpl(Frame frame) => Constants.IntPtrSize * 2;
        public static void WriteInAllocatedMemoryImpl(BinaryWriter writer, MemoryPool memory, Frame frame)
        {
            writer.Write((UInt32)frame.FrameItems.Count);

            var frameItemsAddress = ArrayWriter.WriteArrayOfPointers(memory, frame.FrameItems);
            writer.Write((UInt32)frameItemsAddress);
        }
    }
}
