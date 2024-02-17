using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Terrain;

namespace OpenSage.Logic.Object
{
    public class OCLSpecialPowerModule : SpecialPowerModule, IUpgradableScienceModule
    {
        private readonly OCLSpecialPowerModuleData _moduleData;
        private bool _activated = false;
        private ObjectCreationList _activeOcl;
        private Vector3 _position;

        internal OCLSpecialPowerModule(GameObject gameObject, GameContext context, OCLSpecialPowerModuleData moduleData) : base(gameObject, context, moduleData)
        {
            _moduleData = moduleData;
            _activeOcl = _moduleData.OCL.Value;
        }

        // todo: unclear what this offset should be
        private const int CreateAboveLocationOffset = 250;

        internal override void Update(BehaviorUpdateContext context)
        {
            if (_activated)
            {
                var position = _moduleData.CreateLocation switch
                {
                    OCLCreateLocation.UseOwnerObject => GameObject.Transform.Translation,
                    OCLCreateLocation.CreateAtEdgeNearSource => throw new NotImplementedException(), // todo: calculate edge point and set target location
                    OCLCreateLocation.CreateAtEdgeFarthestFromTarget => throw new NotImplementedException(), // todo: calculate edge point and set target location
                    OCLCreateLocation.CreateAboveLocation => _position with { Z = _position.Z + CreateAboveLocationOffset }, // todo: locomotor needs to update z position to desired altitude agnostic of move order
                    OCLCreateLocation.CreateAtLocation => _position,
                    OCLCreateLocation.CreateAtEdgeNearTargetAndMoveToLocation => throw new NotImplementedException(),
                    OCLCreateLocation.UseSecondaryObjectLocation => throw new NotImplementedException(),
                    _ => throw new ArgumentOutOfRangeException(),
                };

                if (_moduleData.CreateLocation is OCLCreateLocation.UseOwnerObject)
                {
                    // todo: activate via DeliverPayloadAIUpdate
                    var payloads = _activeOcl.Nuggets.OfType<DeliverPayloadOCNugget>();
                    // most will use payload, delivery distance, and drop offset to spawn payload
                    // a10 and artillery barrage use a more complicated method

                    foreach (var deliverPayload in payloads)
                    {
                        if (deliverPayload.SelfDestructObject)
                        {
                            GameObject.Die(DeathType.Normal); // todo: what would this actually be?
                        }
                    }
                }
                else
                {
                    Context.ObjectCreationLists.CreateAtPosition(_activeOcl, context, position);
                }
                _activated = false;
            }
        }

        internal override void Activate(Vector3 position)
        {
            _position = position;
            _activated = true;

            base.Activate(position);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }

        public void TryUpgrade(Science purchasedScience)
        {
            foreach (var (science, ocl) in _moduleData.UpgradeOCLs)
            {
                if (science.Value == purchasedScience)
                {
                    _activeOcl = ocl.Value;
                    return;
                }
            }
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

        internal override OCLSpecialPowerModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new OCLSpecialPowerModule(gameObject, context, this);
        }
    }

    public readonly record struct OCLUpgradePair(LazyAssetReference<Science> Science, LazyAssetReference<ObjectCreationList> OCL)
    {
        internal static OCLUpgradePair Parse(IniParser parser)
        {
            return new OCLUpgradePair
            {
                Science = parser.ParseScienceReference(),
                OCL = parser.ParseObjectCreationListReference(),
            };
        }
    }

    public enum OCLCreateLocation
    {
        /// <summary>
        /// Applied to special power units, seems to work in conjunction with DeliverPayloadAIUpdate. This uses the Payload
        /// option in OCL -> DeliverPayload. Does not spawn transport.
        /// </summary>
        [IniEnum("USE_OWNER_OBJECT")]
        UseOwnerObject,

        /// <summary>
        /// These spawn at the edge of the map closest to the source command center. This is effectively a cardinal
        /// direction straight line from the source, with the direction depending on which quadrant of the map the
        /// source is located in (where the quadrants are defined by straight lines drawn from opposing map corners).
        /// </summary>
        [IniEnum("CREATE_AT_EDGE_NEAR_SOURCE")]
        CreateAtEdgeNearSource,

        /// <summary>
        /// These (artillery barrage, spectre gunship) spawn at the edge of the map the furthest away from their target
        /// physically possible. This is effectively always a corner. This can be any corner - unlike NEAR_SOURCE, the
        /// spawned unit does not need to overfly any additional point.
        /// </summary>
        [IniEnum("CREATE_AT_EDGE_FARTHEST_FROM_TARGET")]
        CreateAtEdgeFarthestFromTarget,

        /// <summary>
        /// USA spy drone - spawns in high and descends
        /// </summary>
        [IniEnum("CREATE_ABOVE_LOCATION")]
        CreateAboveLocation,

        /// <summary>
        /// Created on the ground where clicked (e.g. radar scan, rebel ambush, etc)
        /// </summary>
        [IniEnum("CREATE_AT_LOCATION")]
        CreateAtLocation,

        [IniEnum("CREATE_AT_EDGE_NEAR_TARGET_AND_MOVE_TO_LOCATION"), AddedIn(SageGame.Bfme)]
        CreateAtEdgeNearTargetAndMoveToLocation,

        [IniEnum("USE_SECONDARY_OBJECT_LOCATION"), AddedIn(SageGame.Bfme2)]
        UseSecondaryObjectLocation,
    }
}
