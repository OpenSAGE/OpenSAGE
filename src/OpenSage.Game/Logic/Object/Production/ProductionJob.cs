using System;

namespace OpenSage.Logic.Object.Production
{
    public sealed class ProductionJob : IPersistableObject
    {
        public ProductionJobType Type => _jobType;
        public uint UnitsProduced => _numberOfItemsProduced;

        // todo: this won't properly persist build time with modifiers - we should probably be fetching on-demand?
        // this will need to take into account things like MinLowEnergyProductionSpeed
        private LogicFrameSpan _buildTime;

        private ProductionJobType _jobType;
        private string _templateName;
        private uint _jobId;
        private float _progressPercentage; // can be over 100% when producing multiple items
        private LogicFrameSpan _progressInFrames;
        private uint _totalItemsToProduce; // in the case of china red guard, this is 2
        private uint _numberOfItemsProduced; // when building multiple items, if one of two has been produced this is 1
        private int _unknownInt4;

        public float Progress => Math.Max(0, Math.Min(1, _progressPercentage));

        public void Update()
        {
            _progressInFrames++;
            _progressPercentage = _progressInFrames / _buildTime;
        }

        public ProductionJobResult Produce()
        {
            if (_progressPercentage >= 1)
            {
                if (_jobType is ProductionJobType.Upgrade)
                {
                    return ProductionJobResult.Finished;
                }

                _numberOfItemsProduced++;

                if (_numberOfItemsProduced >= _totalItemsToProduce)
                {
                    return ProductionJobResult.Finished;
                }

                return ProductionJobResult.UnitReady;
            }



            return ProductionJobResult.Producing;
        }

        public ObjectDefinition ObjectDefinition { get; private set; }
        public UpgradeTemplate UpgradeDefinition { get; private set; }

        public ProductionJob(ObjectDefinition definition, LogicFrameSpan buildTime, uint jobId, uint quantity = 1) :
            this(definition.Name, ProductionJobType.Unit, buildTime, jobId, quantity)
        {
            ObjectDefinition = definition;
        }

        // quantity intentionally set to 0
        public ProductionJob(UpgradeTemplate definition, uint jobId) :
            this(definition.Name, ProductionJobType.Upgrade, definition.BuildTime, jobId, 0)
        {
            UpgradeDefinition = definition;
        }

        private ProductionJob(string templateName, ProductionJobType type, LogicFrameSpan buildTime, uint jobId, uint quantity)
        {
            _templateName = templateName;
            _jobType = type;
            _buildTime = buildTime;
            _jobId = jobId;
            _totalItemsToProduce = quantity;
        }

        /// <summary>
        /// Used for creating new copies for persistence.
        /// </summary>
        internal ProductionJob()
        {
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistEnum(ref _jobType);

            reader.PersistAsciiString(ref _templateName);

            if (reader.Mode is StatePersistMode.Read)
            {
                switch (_jobType)
                {
                    case ProductionJobType.Unit:
                        ObjectDefinition =
                            reader.AssetStore.ObjectDefinitions.GetByName(_templateName);
                        _buildTime = ObjectDefinition.BuildTime;
                        break;
                    case ProductionJobType.Upgrade:
                        UpgradeDefinition = reader.AssetStore.Upgrades.GetByName(_templateName);
                        _buildTime = UpgradeDefinition.BuildTime;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_jobType));
                }
            }

            reader.PersistUInt32(ref _jobId);
            reader.PersistSingle(ref _progressPercentage);
            reader.PersistLogicFrameSpan(ref _progressInFrames);
            reader.PersistUInt32(ref _totalItemsToProduce);
            reader.PersistUInt32(ref _numberOfItemsProduced);
            reader.PersistInt32(ref _unknownInt4);

            if (_unknownInt4 != 0 && _unknownInt4 != -1)
            {
                // 0 when producing upgrade, -1 when producing unit
                throw new InvalidStateException();
            }
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
        UnitReady,
        Finished,
    }
}
