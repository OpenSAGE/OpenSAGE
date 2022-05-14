using System;

namespace OpenSage.Logic.Object.Production
{
    public sealed class ProductionJob : IPersistableObject
    {
        private readonly uint _duration;

        private uint _jobId;
        private float _progressPercentage;
        private uint _progressInFrames;
        private int _unknownInt2;
        private int _unknownInt3;
        private int _unknownInt4;

        public ProductionJobType Type { get; }

        public float Progress => Math.Max(0, Math.Min(1, _progressPercentage));

        public ProductionJobResult Produce()
        {
            _progressInFrames++;
            _progressPercentage = _progressInFrames / (float)_duration;
            if (_progressInFrames >= _duration)
            {
                return ProductionJobResult.Finished;
            }
            return ProductionJobResult.Producing;
        }

        public ObjectDefinition ObjectDefinition { get; }
        public UpgradeTemplate UpgradeDefinition { get; }

        public ProductionJob(ObjectDefinition definition, LogicFrameSpan buildTime)
        {
            ObjectDefinition = definition;
            Type = ProductionJobType.Unit;
            _duration = buildTime.Value;
        }

        public ProductionJob(UpgradeTemplate definition)
        {
            UpgradeDefinition = definition;
            Type = ProductionJobType.Upgrade;
            _duration = definition.BuildTime.Value;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistUInt32(ref _jobId);
            reader.PersistSingle(ref _progressPercentage);
            reader.PersistUInt32(ref _progressInFrames);
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
