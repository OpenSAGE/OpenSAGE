using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSage.Logic.Object.Production
{
    public class ProductionJob
    {
        int _cost;
        int _spent;

        public ProductionJobType Type { get; private set; }

        public float Progress { get { return Math.Max(0, Math.Min(1, (float)_spent / (float) _cost)); } }

        public ProductionJobResult Produce(int spent)
        {
            _spent += spent;
            if (_spent >= _cost)
            {
                return ProductionJobResult.Finished;
            }
            return ProductionJobResult.Producing;
        }

        public ObjectDefinition objectDefinition { get; private set; }

        public ProductionJob(ObjectDefinition definition)
        {
            objectDefinition = definition;
            Type = ProductionJobType.Unit;
            _cost = (int)definition.BuildCost;
            _spent = 0;
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
