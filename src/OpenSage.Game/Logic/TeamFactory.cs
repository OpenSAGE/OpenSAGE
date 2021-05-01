using OpenSage.Data.Sav;

namespace OpenSage.Logic
{
    internal sealed class TeamFactory
    {
        private TeamTemplate[] _teamTemplates;

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            var unknown1 = reader.ReadUInt32();

            var count = reader.ReadUInt16();
            _teamTemplates = new TeamTemplate[count];

            for (var i = 0; i < count; i++)
            {
                var id = reader.ReadUInt32();

                _teamTemplates[i] = new TeamTemplate { ID = id };
                _teamTemplates[i].Load(reader);
            }
        }
    }
}
