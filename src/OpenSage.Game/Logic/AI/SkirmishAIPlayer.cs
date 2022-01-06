namespace OpenSage.Logic.AI
{
    public sealed class SkirmishAIPlayer : AIPlayer
    {
        private int _unknownInt1;
        private int _unknownInt2;
        private float _unknownFloat1;
        private float _unknownFloat2;

        internal SkirmishAIPlayer(Player owner)
            : base(owner)
        {

        }

        public override void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Persist(reader);
            reader.EndObject();

            reader.PersistInt32("UnknownInt1", ref _unknownInt1);
            reader.PersistInt32("UnknownInt2", ref _unknownInt2);
            reader.PersistSingle("UnknownFloat1", ref _unknownFloat1);
            reader.PersistSingle("UnknownFloat2", ref _unknownFloat2);

            reader.SkipUnknownBytes(16);
        }
    }
}
