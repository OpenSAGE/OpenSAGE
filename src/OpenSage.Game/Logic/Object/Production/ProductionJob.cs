using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSage.Logic.Object.Production
{
    public class ProductionJob
    {
        double _cost;
        double _spent;

        public ProductionJobType Type { get; private set; }

        public double Progress { get { return Math.Max(0, Math.Min(1, _spent / _cost)); } }

        public ProductionJobResult Produce(GameObject source, double Spent)
        {
            _spent += Spent;
            if (_spent >= _cost)
            {
                Finish(source);
                return ProductionJobResult.Finished;
            }
            return ProductionJobResult.Producing;
        }

        private void Finish(GameObject source)
        {
            switch (Type)
            {
                case ProductionJobType.Unit:
                    source.Spawn(objectDefinition);
                    return;

            }
            throw new NotImplementedException();
        }

        public ObjectDefinition objectDefinition { get; private set; }

        public ProductionJob(ObjectDefinition definition)
        {
            objectDefinition = definition;
            Type = ProductionJobType.Unit;
            _cost = definition.BuildCost / 100.0f;
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
