using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Terrain;

namespace OpenSage.Logic.Object
{
    public class OCLSpecialPowerModule : SpecialPowerModule
    {
        private readonly OCLSpecialPowerModuleData _moduleData;
        private readonly GameObject _gameObject;
        private readonly GameContext _context;
        private bool _activated = false;
        private Vector3 _position;

        internal OCLSpecialPowerModule(GameObject gameObject, GameContext context, OCLSpecialPowerModuleData moduleData)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;
            _context = context;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            if (_activated)
            {
                _context.ObjectCreationLists.CreateAtPosition(_moduleData.OCL.Value, context, _position);
                _activated = false;
            }
        }

        internal override void Activate(Vector3 position)
        {
            _position = position;
            _activated = true;
        }

        internal bool Matches(SpecialPower specialPower)
        {
            return _moduleData.SpecialPower.Value == specialPower;
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    public sealed class OCLSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new OCLSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<OCLSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<OCLSpecialPowerModuleData>
            {
                { "OCL", (parser, x) => x.OCL = parser.ParseObjectCreationListReference() },
                { "UpgradeOCL", (parser, x) => x.UpgradeOCLs.Add(OCLUpgradePair.Parse(parser)) },
                { "CreateLocation", (parser, x) => x.CreateLocation = parser.ParseEnum<OCLCreateLocation>() },
                { "ScriptedSpecialPowerOnly", (parser, x) => x.ScriptedSpecialPowerOnly = parser.ParseBoolean() },
                { "OCLAdjustPositionToPassable", (parser, x) => x.OCLAdjustPositionToPassable = parser.ParseBoolean() },
                { "ReferenceObject", (parser, x) => x.ReferenceObject = parser.ParseAssetReference() },
                { "UpgradeName", (parser, x) => x.UpgradeName = parser.ParseIdentifier() },
                { "NearestSecondaryObjectFilter", (parser, x) => x.NearestSecondaryObjectFilter = ObjectFilter.Parse(parser) },
                { "ReEnableAntiCategory", (parser, x) => x.ReEnableAntiCategory = parser.ParseBoolean() },
                { "WeatherDuration", (parser, x) => x.WeatherDuration = parser.ParseInteger() },
                { "ChangeWeather", (parser, x) => x.ChangeWeather = parser.ParseEnum<WeatherType>() }
            });

        public LazyAssetReference<ObjectCreationList> OCL { get; private set; }
        public List<OCLUpgradePair> UpgradeOCLs { get; } = new List<OCLUpgradePair>();
        public OCLCreateLocation CreateLocation { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool ScriptedSpecialPowerOnly { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool OCLAdjustPositionToPassable { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string ReferenceObject { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string UpgradeName { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectFilter NearestSecondaryObjectFilter { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ReEnableAntiCategory { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int WeatherDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public WeatherType ChangeWeather { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new OCLSpecialPowerModule(gameObject, context, this);
        }
    }

    public sealed class OCLUpgradePair
    {
        internal static OCLUpgradePair Parse(IniParser parser)
        {
            return new OCLUpgradePair
            {
                Science = parser.ParseAssetReference(),
                OCL = parser.ParseAssetReference()
            };
        }

        public string Science { get; private set; }
        public string OCL { get; private set; }
    }

    public enum OCLCreateLocation
    {
        [IniEnum("USE_OWNER_OBJECT")]
        UseOwnerObject,

        [IniEnum("CREATE_AT_EDGE_NEAR_SOURCE")]
        CreateAtEdgeNearSource,

        [IniEnum("CREATE_AT_EDGE_FARTHEST_FROM_TARGET")]
        CreateAtEdgeFarthestFromTarget,

        [IniEnum("CREATE_ABOVE_LOCATION")]
        CreateAboveLocation,

        [IniEnum("CREATE_AT_LOCATION")]
        CreateAtLocation,

        [IniEnum("CREATE_AT_EDGE_NEAR_TARGET_AND_MOVE_TO_LOCATION"), AddedIn(SageGame.Bfme)]
        CreateAtEdgeNearTargetAndMoveToLocation,

        [IniEnum("USE_SECONDARY_OBJECT_LOCATION"), AddedIn(SageGame.Bfme2)]
        UseSecondaryObjectLocation,
    }
}
