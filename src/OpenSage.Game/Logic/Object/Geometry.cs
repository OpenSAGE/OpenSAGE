using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class Geometry
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

        public void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);
            reader.ReadEnum(ref Type);
            reader.ReadBoolean(ref IsSmall);
            Height = reader.ReadSingle();
            MajorRadius = reader.ReadSingle();
            MinorRadius = reader.ReadSingle();
            _unknownFloat1 = reader.ReadSingle();
            _unknownFloat2 = reader.ReadSingle();
        }

        public Geometry Clone() => (Geometry) MemberwiseClone();
    }
}
