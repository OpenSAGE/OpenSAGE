using System;

namespace OpenSage.Logic.Object.Production
{
    public sealed class ProductionJob
    {
        //Duration in milliseconds
        private readonly int _duration;
        private float _passed;

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
    }

    public enum ProductionJobType
    {
        Unit,
        Upgrade,
        Science
    }

    public enum ProductionJobResult
    {
        Producing,
        Finished
    }
}
