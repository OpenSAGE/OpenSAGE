namespace OpenSage.Logic
{
    public sealed class CampaignManager
    {
        public string CampaignName;
        public string MissionName;

        private uint _unknown;
        private uint _difficultyMaybe;

        private bool _unknownBool1;
        private bool _unknownBool2;

        internal void Load(SaveFileReader reader)
        {
            var version = reader.ReadVersion(5);

            reader.ReadAsciiString(ref CampaignName);
            reader.ReadAsciiString(ref MissionName);
            _unknown = reader.ReadUInt32();
            _difficultyMaybe = reader.ReadUInt32();

            if (version >= 5)
            {
                _unknownBool1 = reader.ReadBoolean();

                reader.SkipUnknownBytes(4);
            }

            if (version == 1 && reader.SageGame >= SageGame.Bfme)
            {
                _unknownBool2 = reader.ReadBoolean();
            }
        }
    }
}
