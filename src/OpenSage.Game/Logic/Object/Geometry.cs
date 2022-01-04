using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class Geometry : IPersistableObject
    {
        private float _unknownFloat1;
        private float _unknownFloat2;

        public Geometry() { }

        public Geometry(ObjectGeometry type)
        {
            Type = type;
        }

        internal static Geometry Parse(IniParser parser)
        {
            return new Geometry()
            {
                Type = parser.ParseAttributeEnum<ObjectGeometry>("GeomType"),
                IsSmall = parser.ParseAttributeBoolean("IsSmall"),
                Height = parser.ParseAttributeInteger("Height"),
                MajorRadius = parser.ParseAttributeInteger("MajorRadius"),
                MinorRadius = parser.ParseAttributeInteger("MinorRadius"),
                OffsetX = parser.ParseAttributeInteger("OffsetX")
            };
        }

        public string Name;
        public ObjectGeometry Type;
        public bool IsSmall;
        public float Height;
        public float MajorRadius;
        public float MinorRadius;
        public int OffsetX;
        public Vector3 Offset;
        public bool IsActive;
        public float FrontAngle;

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistEnum(ref Type);
            reader.PersistBoolean("IsSmall", ref IsSmall);
            reader.PersistSingle("Height", ref Height);
            reader.PersistSingle("MajorRadius", ref MajorRadius);
            reader.PersistSingle("MinorRadius", ref MinorRadius);
            reader.PersistSingle("UnknownFloat1", ref _unknownFloat1);
            reader.PersistSingle("UnknonwFloat2", ref _unknownFloat2);
        }

        public Geometry Clone() => (Geometry) MemberwiseClone();
    }
}
