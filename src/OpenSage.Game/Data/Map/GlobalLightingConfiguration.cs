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
            var result = new GlobalLightingConfiguration();

            result.TerrainSun = GlobalLight.Parse(reader);

            if (version < 11)
            {
                result.ObjectSun = GlobalLight.Parse(reader);

                if (version >= 7)
                {
                    result.InfantrySun = GlobalLight.Parse(reader);
                }
            }

            result.TerrainAccent1 = GlobalLight.Parse(reader);

            if (version < 11)
            {
                result.ObjectAccent1 = GlobalLight.Parse(reader);

                if (version >= 7)
                {
                    result.InfantryAccent1 = GlobalLight.Parse(reader);
                }
            }

            result.TerrainAccent2 = GlobalLight.Parse(reader);

            if (version < 11)
            {
                result.ObjectAccent2 = GlobalLight.Parse(reader);

                if (version >= 7)
                {
                    result.InfantryAccent2 = GlobalLight.Parse(reader);
                }
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer, uint version)
        {
            TerrainSun.WriteTo(writer);

            if (version < 11)
            {
                ObjectSun.WriteTo(writer);

                if (version >= 7)
                {
                    InfantrySun.WriteTo(writer);
                }
            }

            TerrainAccent1.WriteTo(writer);

            if (version < 11)
            {
                ObjectAccent1.WriteTo(writer);

                if (version >= 7)
                {
                    InfantryAccent1.WriteTo(writer);
                }
            }

            TerrainAccent2.WriteTo(writer);

            if (version < 11)
            {
                ObjectAccent2.WriteTo(writer);

                if (version >= 7)
                {
                    InfantryAccent2.WriteTo(writer);
                }
            }
        }
    }
}
