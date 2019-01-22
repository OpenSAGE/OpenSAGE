using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Mathematics;

namespace OpenSage.Data.Apt.FrameItems
{
    public enum ClipEventFlags : uint
    {
        KeyUp = 0x800000,
        KeyDown = 0x400000,
        MouseUp = 0x200000,
        MouseDown = 0x100000,
        MouseMove = 0x080000,
        Unload = 0x040000,
        EnterFrame = 0x020000,
        Load = 0x010000,
        DragOver = 0x008000,
        RollOut = 0x004000,
        RollOver = 0x002000,
        ReleaseOutside = 0x001000,
        Release = 0x000800,
        Press = 0x000400,
        DragOut = 0x000200,
        Data = 0x000100,
        Construct = 0x000004,
        KeyPress = 0x000002,
        Initialize = 0x000001,
    }

    public sealed class ClipEvent
    {
        public ClipEventFlags Flags { get; private set; }
        public InstructionCollection Instructions { get; private set; }

        public static ClipEvent Parse(BinaryReader reader)
        {
            var ev = new ClipEvent();           
            ev.Flags = reader.ReadUInt24AsEnum<ClipEventFlags>();
            var keycode = reader.ReadByte();
            var offsetToNext = reader.ReadUInt32();
            //var keycode = reader.ReadByte();
            ev.Instructions = new InstructionCollection(reader.BaseStream);
            ev.Instructions.Parse();
            return ev;
        }
    }

    [FlagsAttribute]
    public enum PlaceObjectFlags : byte
    {
        Move = 1,
        HasCharacter = 2,
        HasMatrix = 4,
        HasColorTransform = 8,
        HasRatio = 16,
        HasName = 32,
        HasClipDepth = 64,
        HasClipAction = 128,
    }

    public sealed class PlaceObject : FrameItem
    {
        public PlaceObjectFlags Flags { get; private set; }
        public int Depth { get; private set; }
        public int Character { get; private set; }
        public Matrix2x2 RotScale { get; private set; }
        public Vector2 Translation { get; private set; }
        public ColorRgba Color { get; private set; }
        public float Ratio { get; private set; }
        public string Name { get; private set; }
        public int ClipDepth { get; private set; }
        public List<ClipEvent> ClipEvents{ get; private set; }

        public static PlaceObject Parse(BinaryReader reader)
        {
            var placeobject = new PlaceObject();
            //read this as Uint32, because 3 bytes are reserved and always 0
            placeobject.Flags = reader.ReadUInt32AsEnumFlags<PlaceObjectFlags>();
            placeobject.Depth = reader.ReadInt32();

            //only read the values that are set by the flags
            if (placeobject.Flags.HasFlag(PlaceObjectFlags.HasCharacter))
                placeobject.Character = reader.ReadInt32();
            else
                reader.ReadInt32();

            if (placeobject.Flags.HasFlag(PlaceObjectFlags.HasMatrix))
            {
                placeobject.RotScale = reader.ReadMatrix2x2();
                placeobject.Translation = reader.ReadVector2();
            }
            else
            {
                reader.ReadMatrix2x2();
                reader.ReadVector2();
            }

            if (placeobject.Flags.HasFlag(PlaceObjectFlags.HasColorTransform))
                placeobject.Color = reader.ReadColorRgba();
            else
                reader.ReadColorRgba();

            var unknown = reader.ReadUInt32();

            if (placeobject.Flags.HasFlag(PlaceObjectFlags.HasRatio))
                placeobject.Ratio = reader.ReadSingle();
            else
                reader.ReadSingle();

            if (placeobject.Flags.HasFlag(PlaceObjectFlags.HasName))
                placeobject.Name = reader.ReadStringAtOffset();
            else
                reader.ReadUInt32();

            placeobject.ClipDepth = reader.ReadInt32();

            if (placeobject.Flags.HasFlag(PlaceObjectFlags.HasClipAction))
            {
                var poaOffset = reader.ReadUInt32();
                if (poaOffset!=0)
                {
                    var oldOffset = reader.BaseStream.Position;
                    reader.BaseStream.Seek(poaOffset, SeekOrigin.Begin);
                    placeobject.ClipEvents = reader.ReadListAtOffset<ClipEvent>(() => ClipEvent.Parse(reader));
                    reader.BaseStream.Seek(oldOffset, SeekOrigin.Begin);
                }
            }

            return placeobject;
        }
    }
}
