using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class GlobalLightingConfiguration
    {
        public GlobalLight TerrainSun { get; private set; }
        public GlobalLight TerrainAccent1 { get; private set; }
        public GlobalLight TerrainAccent2 { get; private set; }

        public GlobalLight ObjectSun { get; private set; }
        public GlobalLight ObjectAccent1 { get; private set; }
        public GlobalLight ObjectAccent2 { get; private set; }

        public static GlobalLightingConfiguration Parse(BinaryReader reader)
        {
            return new GlobalLightingConfiguration
            {
                TerrainSun = GlobalLight.Parse(reader),
                ObjectSun = GlobalLight.Parse(reader),

                TerrainAccent1 = GlobalLight.Parse(reader),
                TerrainAccent2 = GlobalLight.Parse(reader),

                ObjectAccent1 = GlobalLight.Parse(reader),
                ObjectAccent2 = GlobalLight.Parse(reader),
            };
        }

        public void WriteTo(BinaryWriter writer)
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
