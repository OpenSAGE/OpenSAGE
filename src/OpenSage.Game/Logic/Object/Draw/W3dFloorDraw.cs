using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class W3dFloorDraw : DrawModule
    {
        private readonly GameObject _gameObject;
        private readonly GameContext _gameContext;
        private ModelInstance _modelInstance;
        private readonly W3dFloorDrawModuleData _moduleData;

        public override IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; } = Array.Empty<BitArray<ModelConditionFlag>>();

        internal W3dFloorDraw(W3dFloorDrawModuleData moduleData, GameContext context, GameObject gameObject)
        {
            _gameObject = gameObject;
            _gameContext = context;
            _moduleData = moduleData;
            SetModelName(_moduleData.ModelName);
        }

        public void SetModelName(string modelName)
        {
            var model = _gameContext.AssetLoadContext.AssetStore.Models.GetByName(modelName);
            _modelInstance = AddDisposable(model.CreateInstance(_gameContext.AssetLoadContext));
            _modelInstance.Update(TimeInterval.Zero);
        }

        internal override void BuildRenderList(RenderList renderList, Camera camera, bool castsShadow, MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS)
        {
            foreach (var hideFlag in _moduleData.HideIfModelConditions)
            {
                if (_gameObject.ModelConditionFlags.Get(hideFlag))
                {
                    return;
                }
            }

            _modelInstance.BuildRenderList(
                renderList,
                camera,
                castsShadow,
                renderItemConstantsPS);
        }

        internal override (ModelInstance, ModelBone) FindBone(string boneName)
        {
            throw new NotSupportedException();
        }

        internal override string GetWeaponFireFXBone(WeaponSlot slot)
        {
            throw new NotSupportedException();
        }

        internal override string GetWeaponLaunchBone(WeaponSlot slot)
        {
            throw new NotSupportedException();
        }

        internal override void SetWorldMatrix(in Matrix4x4 worldMatrix)
        {
            _modelInstance.SetWorldMatrix(worldMatrix);
        }

        internal override void Update(in TimeInterval time)
        {
            _modelInstance.Update(time);
        }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class W3dFloorDrawModuleData : DrawModuleData
    {
        internal static W3dFloorDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dFloorDrawModuleData> FieldParseTable = new IniParseTable<W3dFloorDrawModuleData>
        {
            { "ModelName", (parser, x) => x.ModelName = parser.ParseAssetReference() },
            { "StaticModelLODMode", (parser, x) => x.StaticModelLODMode = parser.ParseBoolean() },
            { "StartHidden", (parser, x) => x.StartHidden = parser.ParseBoolean() },
            { "ForceToBack", (parser, x) => x.ForceToBack = parser.ParseBoolean() },
            { "FloorFadeRateOnObjectDeath", (parser, x) => x.FloorFadeRateOnObjectDeath = parser.ParseFloat() },
            { "HideIfModelConditions", (parser, x) => x.HideIfModelConditions.Add(parser.ParseEnum<ModelConditionFlag>())},
            { "WeatherTexture", (parser, x) => x.WeatherTexture = WeatherTexture.Parse(parser) }
        };

        public string ModelName { get; private set; }
        public bool StaticModelLODMode { get; private set; }
        public bool StartHidden { get; private set; }
        public bool ForceToBack { get; private set; }
        public float FloorFadeRateOnObjectDeath { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<ModelConditionFlag> HideIfModelConditions { get; } = new List<ModelConditionFlag>();

        [AddedIn(SageGame.Bfme2)]
        public WeatherTexture WeatherTexture { get; private set; }

        internal override DrawModule CreateDrawModule(GameObject gameObject, GameContext context)
        {
            return new W3dFloorDraw(this, context, gameObject);
        }
    }

    public struct WeatherTexture
    {
        internal static WeatherTexture Parse(IniParser parser)
        {
            return new WeatherTexture()
            {
                WatherType = parser.ParseEnum<MapWeatherType>(),
                Texture = parser.ParseAssetReference()
            };
        }

        public string Texture { get; private set; }
        public MapWeatherType WatherType { get; private set; }
    }
}
