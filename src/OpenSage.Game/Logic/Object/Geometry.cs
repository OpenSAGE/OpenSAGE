using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class Geometry
    {
        private ObjectGeometry _type;

        public Geometry() { }

        public Geometry(ObjectGeometry type)
        {
            _type = type;
        }

        internal static Geometry Parse(IniParser parser)
        {
            return new Geometry()
            {
                _type = parser.ParseAttributeEnum<ObjectGeometry>("GeomType"),
                IsSmall = parser.ParseAttributeBoolean("IsSmall"),
                Height = parser.ParseAttributeInteger("Height"),
                MajorRadius = parser.ParseAttributeInteger("MajorRadius"),
                MinorRadius = parser.ParseAttributeInteger("MinorRadius"),
                OffsetX = parser.ParseAttributeInteger("OffsetX")
            };
        }

        public string Name { get; set; }
        public ObjectGeometry Type => _type;
        public bool IsSmall { get; set; }
        public float Height { get; set; }
        public float MajorRadius { get; set; }
        public float MinorRadius { get; set; }
        public int OffsetX { get; set; }
        public Vector3 Offset { get; set; }
        public bool IsActive { get; set; }
        public float FrontAngle { get; set; }

        public void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);
            reader.ReadEnum(ref _type);
            IsSmall = reader.ReadBoolean();
            Height = reader.ReadSingle();
            MajorRadius = reader.ReadSingle();
            MinorRadius = reader.ReadSingle();
            var unknown1 = reader.ReadSingle();
            var unknown2 = reader.ReadSingle();
        }

        public Geometry Clone() => (Geometry) MemberwiseClone();
    }
}
