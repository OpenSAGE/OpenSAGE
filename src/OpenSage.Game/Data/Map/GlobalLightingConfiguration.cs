using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class GlobalLightingConfiguration
    {
        public GlobalLight TerrainSun { get; private set; }
        public GlobalLight TerrainAccent1 { get; private set; }
        public GlobalLight TerrainAccent2 { get; private set; }

        public GlobalLight ObjectSun { get; private set; }
        public GlobalLight ObjectAccent1 { get; private set; }
        public GlobalLight ObjectAccent2 { get; private set; }

        public GlobalLight InfantrySun { get; private set; }
        public GlobalLight InfantryAccent1 { get; private set; }
        public GlobalLight InfantryAccent2 { get; private set; }

        internal static GlobalLightingConfiguration Parse(BinaryReader reader, uint version)
        {
            var terrainSun = GlobalLight.Parse(reader);
            var objectSun = GlobalLight.Parse(reader);

            GlobalLight infantrySun = null;
            if (version >= 7)
            {
                infantrySun = GlobalLight.Parse(reader);
            }

            var terrainAccent1 = GlobalLight.Parse(reader);
            var objectAccent1 = GlobalLight.Parse(reader);

            GlobalLight infantryAccent1 = null;
            if (version >= 7)
            {
                infantryAccent1 = GlobalLight.Parse(reader);
            }

            var terrainAccent2 = GlobalLight.Parse(reader);
            var objectAccent2 = GlobalLight.Parse(reader);

            GlobalLight infantryAccent2 = null;
            if (version >= 7)
            {
                infantryAccent2 = GlobalLight.Parse(reader);
            }

            return new GlobalLightingConfiguration
            {
                TerrainSun = terrainSun,
                ObjectSun = objectSun,
                InfantrySun = infantrySun,

                TerrainAccent1 = terrainAccent1,
                TerrainAccent2 = terrainAccent2,
                InfantryAccent1 = infantryAccent1,

                ObjectAccent1 = objectAccent1,
                ObjectAccent2 = objectAccent2,
                InfantryAccent2 = infantryAccent2
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            TerrainSun.WriteTo(writer);
            ObjectSun.WriteTo(writer);

            TerrainAccent1.WriteTo(writer);
            TerrainAccent2.WriteTo(writer);

            ObjectAccent1.WriteTo(writer);
            ObjectAccent2.WriteTo(writer);
        }
    }
}
