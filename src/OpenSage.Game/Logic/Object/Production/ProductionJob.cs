using System;

namespace OpenSage.Logic.Object.Production
{
    public sealed class ProductionJob : IPersistableObject
    {
        //Duration in milliseconds
        private readonly int _duration;
        private float _passed;

        private uint _jobId;
        private float _progressPercentage;
        private int _progressInFrames;
        private int _unknownInt2;
        private int _unknownInt3;
        private int _unknownInt4;

        public ProductionJobType Type { get; }

        public float Progress => Math.Max(0, Math.Min(1, (float) (_passed / _duration)));

        public ProductionJobResult Produce(float passed)
        {
            _passed += passed;
            if (_passed >= _duration)
            {
                return ProductionJobResult.Finished;
            }
            return ProductionJobResult.Producing;
        }

        public ObjectDefinition ObjectDefinition { get; }
        public UpgradeTemplate UpgradeDefinition { get; }

        public ProductionJob(ObjectDefinition definition, float buildTime = 0.0f)
        {
            ObjectDefinition = definition;
            Type = ProductionJobType.Unit;
            _duration = (int) (buildTime * 1000.0f);
        }

        public ProductionJob(UpgradeTemplate definition)
        {
            UpgradeDefinition = definition;
            Type = ProductionJobType.Upgrade;
            _duration = (int) (definition.BuildTime * 1000.0f);
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistUInt32(ref _jobId);
            reader.PersistSingle(ref _progressPercentage);
            reader.PersistInt32(ref _progressInFrames); // Maybe progress
            reader.PersistInt32(ref _unknownInt2);
            reader.PersistInt32(ref _unknownInt3);
            reader.PersistInt32(ref _unknownInt4);
        }
    }

    public enum ProductionJobType
    {
        Unit = 1,
        Upgrade = 2,
    }

    public enum ProductionJobResult
    {
        Producing,
        Finished
    }
}
