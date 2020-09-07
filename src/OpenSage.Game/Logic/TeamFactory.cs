using System.IO;

namespace OpenSage.Logic
{
    internal sealed class TeamFactory
    {
        private Team[] _teams;

        internal void Load(BinaryReader reader)
        {
            var unknown1 = reader.ReadUInt32();

            var count = reader.ReadUInt16();
            _teams = new Team[count];

            for (var i = 0; i < count; i++)
            {
                _teams[i] = new Team();
                _teams[i].Load(reader);
            }
        }
    }
}
