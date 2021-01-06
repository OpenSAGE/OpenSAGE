using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Utilities;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.FileFormats;
using OpenSage.Mathematics;
using OpenSage.Tools.AptEditor.Util;
using Action = OpenSage.Data.Apt.FrameItems.Action;

namespace OpenSage.Tools.AptEditor.Apt.Writer {
    public static partial class DataWriter
    {
        static uint WriteImpl(MemoryPool memory, Action action)
        {
            var actionMemory = memory.AllocatePadded(Constants.IntPtrSize * 2, Constants.IntPtrSize);
            using (var actionWriter = GetWriter(actionMemory))
            {
                actionWriter.Write((UInt32)FrameItemType.Action);

                var instructionStartAddress = Write(memory, action.Instructions);
                actionWriter.Write((UInt32)instructionStartAddress);
            }
            return actionMemory.StartAddress;
        }

        static uint WriteImpl(MemoryPool memory, BackgroundColor backgroundColor)
        {
            var backgroundColorMemory = memory.AllocatePadded(Constants.IntPtrSize * 2, Constants.IntPtrSize);
            using (var backgroundColorWriter = GetWriter(backgroundColorMemory))
            {
                backgroundColorWriter.Write((UInt32)FrameItemType.BackgroundColor);
                backgroundColorWriter.Write((ColorRgba)backgroundColor.Color);
            }
            return backgroundColorMemory.StartAddress;
        }

        static uint WriteImpl(MemoryPool memory, FrameLabel label)
        {
            var labelMemory = memory.AllocatePadded(Constants.IntPtrSize * 4, Constants.IntPtrSize);
            using (var labelWriter = GetWriter(labelMemory))
            {
                labelWriter.Write((UInt32)FrameItemType.FrameLabel);

                var labelNameAddress = Write(memory, label.Name);
                labelWriter.Write((UInt32)labelNameAddress);
                labelWriter.Write((UInt32)label.Flags);
                labelWriter.Write((UInt32)label.FrameId);
            }
            return labelMemory.StartAddress;
        }

        static uint WriteImpl(MemoryPool memory, InitAction initAction)
        {
            var initActionMemory = memory.AllocatePadded(Constants.IntPtrSize * 3, Constants.IntPtrSize);
            using (var initActionWriter = GetWriter(initActionMemory))
            {
                initActionWriter.Write((UInt32)FrameItemType.InitAction);
                initActionWriter.Write((UInt32)initAction.Sprite);

                var instructionStartAddress = Write(memory, initAction.Instructions);
                initActionWriter.Write((UInt32)instructionStartAddress);
            }
            return initActionMemory.StartAddress;
        }

        static uint WriteImpl(MemoryPool memory, PlaceObject placeObject)
        {
            var placeObjectMemory = memory.AllocatePadded(Constants.IntPtrSize * 16, Constants.IntPtrSize);
            using (var placeObjectWriter = GetWriter(placeObjectMemory))
            {
                placeObjectWriter.Write((UInt32)FrameItemType.PlaceObject);
                placeObjectWriter.Write((UInt32)placeObject.Flags);
                placeObjectWriter.Write((Int32)placeObject.Depth);
                placeObjectWriter.Write((Int32)placeObject.Character);
                placeObjectWriter.Write((Matrix2x2)placeObject.RotScale);
                placeObjectWriter.Write((Vector2)placeObject.Translation);
                placeObjectWriter.Write((ColorRgba)placeObject.Color);
                placeObjectWriter.Write((UInt32)0); // unknown
                placeObjectWriter.Write((Single)placeObject.Ratio);

                var nameAddress = placeObject.Flags.HasFlag(PlaceObjectFlags.HasName) ? Write(memory, placeObject.Name) : 0;
                placeObjectWriter.Write((UInt32)nameAddress);

                placeObjectWriter.Write((Int32)placeObject.ClipDepth);

                var clipEventArrayAddress = placeObject.Flags.HasFlag(PlaceObjectFlags.HasClipAction) ? WriteClipEventArray(memory, placeObject.ClipEvents) : 0;
                placeObjectWriter.Write((UInt32)clipEventArrayAddress);
            }
            return placeObjectMemory.StartAddress;
        }

        static uint WriteClipEventArray(MemoryPool memory, ICollection<ClipEvent> clipEvents)
        {
            var clipEventArrayMemory = memory.AllocatePadded(Constants.IntPtrSize * 2, Constants.IntPtrSize);
            using (var clipEventArrayWriter = GetWriter(clipEventArrayMemory))
            {
                clipEventArrayWriter.Write((UInt32)clipEvents.Count);

                var arrayAddress = ArrayWriter.WritePlainArray(memory, clipEvents);
                clipEventArrayWriter.Write((UInt32)arrayAddress);
            }
            return clipEventArrayMemory.StartAddress;
        }
        static uint WriteImpl(MemoryPool memory, RemoveObject removeObject)
        {
            var removeObjectMemory = memory.AllocatePadded(Constants.IntPtrSize * 2, Constants.IntPtrSize);
            using (var removeObjectWriter = GetWriter(removeObjectMemory))
            {
                removeObjectWriter.Write((UInt32)FrameItemType.RemoveObject);
                removeObjectWriter.Write((Int32)removeObject.Depth);
            }
            return removeObjectMemory.StartAddress;
        }
    }

    public static partial class PlainArrayElementWriter
    {
        public static uint GetElementSizeImpl(ClipEvent clipEvent) => Constants.IntPtrSize * 3;
        public static void WriteInAllocatedMemoryImpl(BinaryWriter writer, MemoryPool memory, ClipEvent clipEvent)
        {
            writer.WriteUInt24((UInt32)clipEvent.Flags);
            writer.Write((Byte)clipEvent.KeyCode);
            writer.Write((UInt32)0); // offsetToNext? 

            var clipActionsAddress = DataWriter.Write(memory, clipEvent.Instructions);
            writer.Write((UInt32)clipActionsAddress);
        }
    }
}