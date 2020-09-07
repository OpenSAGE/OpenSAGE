using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSage.FileFormats;

namespace OpenSage.Logic
{
    public sealed class Team
    {
        public string Name { get; }
        public Player Owner { get; }
        public bool IsSingleton { get; }

        public Team(string name, Player owner, bool isSingleton)
        {
            Name = name;
            Owner = owner;
            IsSingleton = isSingleton;
        }

        internal Team() { }

        public static Team FromMapData(Data.Map.Team mapTeam, IList<Player> players)
        {
            var name = mapTeam.Properties["teamName"].Value as string;

            var ownerName = mapTeam.Properties["teamOwner"].Value as string;
            var owner = players.FirstOrDefault(player => player.Name == ownerName);

            var isSingleton = (bool) mapTeam.Properties["teamIsSingleton"].Value;

            return new Team(name, owner, isSingleton);
        }

        internal void Load(BinaryReader reader)
        {
            var id = reader.ReadUInt32();

            var unknown1 = reader.ReadByte();
            if (unknown1 != 2)
            {
                throw new InvalidDataException();
            }

            var unknown1_2 = reader.ReadUInt32();

            var attackPriority = reader.ReadBytePrefixedAsciiString();

            var unknown2 = reader.ReadBooleanChecked();

            var unknown2_1 = reader.ReadByte();
            if (unknown2_1 != 1)
            {
                throw new InvalidDataException();
            }

            var unknown3 = reader.ReadUInt32();

            var unknown5 = reader.ReadUInt16();
            if (unknown5 != 0)
            {
                if (unknown5 != 1)
                {
                    throw new InvalidDataException();
                }

                var unknown5_1 = reader.ReadUInt32();

                var unknown6 = reader.ReadByte();

                var unknown6_1 = reader.ReadUInt32();

                var numObjects = reader.ReadUInt16();
                for (var i = 0; i < numObjects; i++)
                {
                    var objectId = reader.ReadUInt32();
                }

                var unknown7 = reader.ReadUInt16();

                var unknown8 = reader.ReadUInt32();
                if (unknown8 != 0 && unknown8 != 1)
                {
                    throw new InvalidDataException();
                }

                var unknown9 = reader.ReadUInt32();
                if (unknown9 != 0)
                {
                    throw new InvalidDataException();
                }

                var unknown10 = reader.ReadUInt32();
                if (unknown10 != 0)
                {
                    throw new InvalidDataException();
                }

                var unknown11 = reader.ReadUInt32();
                if (unknown11 != 0)
                {
                    throw new InvalidDataException();
                }

                var unknown12 = reader.ReadUInt16();
                if (unknown12 != 0)
                {
                    throw new InvalidDataException();
                }

                var unknown13 = reader.ReadUInt32();
                if (unknown13 != 16)
                {
                    throw new InvalidDataException();
                }

                for (var i = 0; i < 5; i++)
                {
                    var unknown14 = reader.ReadUInt32();
                    if (unknown14 != 0)
                    {
                        throw new InvalidDataException();
                    }
                }

                for (var i = 0; i < 2; i++)
                {
                    var unknown15 = reader.ReadUInt16();
                    if (unknown15 != 1)
                    {
                        throw new InvalidDataException();
                    }

                    var unknown16 = reader.ReadByte();
                    if (unknown16 != 0)
                    {
                        throw new InvalidDataException();
                    }
                }
            }
        }
    }
}
