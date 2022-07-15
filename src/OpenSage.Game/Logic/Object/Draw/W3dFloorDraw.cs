using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Client;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class W3dFloorDraw : DrawModule
    {
        private readonly Drawable _drawable;
        private readonly GameContext _gameContext;
        private readonly ModelInstance _modelInstance;
        private readonly W3dFloorDrawModuleData _moduleData;

        public override IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; } = Array.Empty<BitArray<ModelConditionFlag>>();

        internal W3dFloorDraw(W3dFloorDrawModuleData moduleData, GameContext context, Drawable drawable)
        {
            _drawable = drawable;
            _gameContext = context;
            _moduleData = moduleData;
            _modelInstance = AddDisposable(_moduleData.Model.Value.CreateInstance(_gameContext.AssetLoadContext.GraphicsDevice, _gameContext.AssetLoadContext.StandardGraphicsResources, _gameContext.AssetLoadContext.ShaderResources.Mesh));
        }

        internal override void BuildRenderList(
                RenderList renderList,
                Camera camera,
                bool castsShadow,
                MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS,
                Dictionary<string, bool> shownSubObjects = null,
                Dictionary<string, bool> hiddenSubObjects = null)
        {
            foreach (var hideFlag in _moduleData.HideIfModelConditions)
            {
                if (_drawable.ModelConditionFlags.Get(hideFlag))
                {
                    return;
                }
            }

            _modelInstance.BuildRenderList(
                renderList,
                camera,
                castsShadow,
                renderItemConstantsPS,
                shownSubObjects,
                hiddenSubObjects);
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
            { "ModelName", (parser, x) => x.Model = parser.ParseModelReference() },
            { "StaticModelLODMode", (parser, x) => x.StaticModelLODMode = parser.ParseBoolean() },
            { "StartHidden", (parser, x) => x.StartHidden = parser.ParseBoolean() },
            { "ForceToBack", (parser, x) => x.ForceToBack = parser.ParseBoolean() },
            { "FloorFadeRateOnObjectDeath", (parser, x) => x.FloorFadeRateOnObjectDeath = parser.ParseFloat() },
            { "HideIfModelConditions", (parser, x) => x.HideIfModelConditions.Add(parser.ParseEnum<ModelConditionFlag>())},
            { "WeatherTexture", (parser, x) => x.WeatherTexture = WeatherTexture.Parse(parser) }
        };

        public LazyAssetReference<Model> Model { get; private set; }
        public bool StaticModelLODMode { get; private set; }
        public bool StartHidden { get; private set; }
        public bool ForceToBack { get; private set; }
        public float FloorFadeRateOnObjectDeath { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<ModelConditionFlag> HideIfModelConditions { get; } = new List<ModelConditionFlag>();

        [AddedIn(SageGame.Bfme2)]
        public WeatherTexture WeatherTexture { get; private set; }

        internal override DrawModule CreateDrawModule(Drawable drawable, GameContext context)
        {
            return Model.Value == null ? null : (DrawModule) new W3dFloorDraw(this, context, drawable);
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
