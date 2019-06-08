using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSage.Logic.Object.Production
{
    public class ProductionJob
    {
        float _cost;
        float _spent;

        public ProductionJobType Type { get; private set; }

        public float Progress { get { return Math.Max(0, Math.Min(1, _spent / _cost)); } }

        public bool Produce(GameObject source, float Spent)
        {
            _spent += Spent;
            if (_spent > _cost)
            {
                Finish(source);
                return true;
            }
            return false;
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

}
