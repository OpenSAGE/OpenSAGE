using System;
using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Mathematics;

namespace OpenSage.Data.Apt.FrameItems
{
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

            //TODO: add clip actions

            return placeobject;
        }
    }
}
