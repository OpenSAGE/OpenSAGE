using System;

namespace OpenSage.Logic.Object.Production
{
    public sealed class ProductionJob
    {
        private readonly int _cost;
        private int _spent;

        public ProductionJobType Type { get; }

        public float Progress => Math.Max(0, Math.Min(1, _spent / (float) _cost));

        public ProductionJobResult Produce(int spent)
        {
            _spent += spent;
            if (_spent >= _cost)
            {
                return ProductionJobResult.Finished;
            }
            return ProductionJobResult.Producing;
        }

        public ObjectDefinition ObjectDefinition { get; }

        public ProductionJob(ObjectDefinition definition)
        {
            ObjectDefinition = definition;
            Type = ProductionJobType.Unit;
            _cost = (int) definition.BuildCost;
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
