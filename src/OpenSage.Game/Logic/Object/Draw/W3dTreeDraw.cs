using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class W3dTreeDraw : DrawModule
    {
        private readonly GameContext _gameContext;
        private ModelInstance _modelInstance;
        private readonly W3dTreeDrawModuleData _moduleData;

        public override IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; } = Array.Empty<BitArray<ModelConditionFlag>>();

        internal W3dTreeDraw(W3dTreeDrawModuleData moduleData, GameContext context)
        {
            _gameContext = context;
            _moduleData = moduleData;
            _modelInstance = AddDisposable(_moduleData.Model.Value.CreateInstance(_gameContext.AssetLoadContext));
            //TODO: overwrite texture somehow & take care of other fields
        }

        internal override void BuildRenderList(RenderList renderList, Camera camera, bool castsShadow, MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS)
        {
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

    public sealed class W3dTreeDrawModuleData : DrawModuleData
    {
        internal static W3dTreeDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dTreeDrawModuleData> FieldParseTable = new IniParseTable<W3dTreeDrawModuleData>
        {
            { "ModelName", (parser, x) => x.Model = parser.ParseModelReference() },
            { "TextureName", (parser, x) => x.Texture = parser.ParseTextureReference() },

            { "DoTopple", (parser, x) => x.DoTopple = parser.ParseBoolean() },
            { "DoShadow", (parser, x) => x.DoShadow = parser.ParseBoolean() },
            { "ToppleFX", (parser, x) => x.ToppleFX = parser.ParseFXListReference() },
            { "BounceFX", (parser, x) => x.BounceFX = parser.ParseFXListReference() },
            { "KillWhenFinishedToppling", (parser, x) => x.KillWhenFinishedToppling = parser.ParseBoolean() },
            { "SinkDistance", (parser, x) => x.SinkDistance = parser.ParseInteger() },
            { "SinkTime", (parser, x) => x.SinkTime = parser.ParseInteger() },

            { "MoveOutwardTime", (parser, x) => x.MoveOutwardTime = parser.ParseInteger() },
            { "MoveInwardTime", (parser, x) => x.MoveInwardTime = parser.ParseInteger() },
            { "MoveOutwardDistanceFactor", (parser, x) => x.MoveOutwardDistanceFactor = parser.ParseFloat() },
            { "DarkeningFactor", (parser, x) => x.DarkeningFactor = parser.ParseFloat() },
            { "MorphTime", (parser, x) => x.MorphTime = parser.ParseInteger() },
            { "TaintedTree", (parser, x) => x.TaintedTree = parser.ParseBoolean() },
            { "FadeDistance", (parser, x) => x.FadeDistance = parser.ParseInteger() },
            { "FadeTarget", (parser, x) => x.FadeTarget = parser.ParseInteger() },
            { "MorphFX", (parser, x) => x.MorphFX = parser.ParseFXListReference() },
            { "MorphTree", (parser, x) => x.MorphTree = parser.ParseIdentifier() }
        };

        public LazyAssetReference<Model> Model { get; private set; }
        public LazyAssetReference<TextureAsset> Texture { get; private set; }

        public bool DoTopple { get; private set; }
        public bool DoShadow { get; private set; }
        public LazyAssetReference<FXList> ToppleFX { get; private set; }
        public LazyAssetReference<FXList> BounceFX { get; private set; }
        public bool KillWhenFinishedToppling { get; private set; }
        public int SinkDistance { get; private set; }
        public int SinkTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MoveOutwardTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MoveInwardTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float MoveOutwardDistanceFactor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float DarkeningFactor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int MorphTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool TaintedTree { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int FadeDistance { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int FadeTarget { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public LazyAssetReference<FXList> MorphFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string MorphTree { get; private set; }

        internal override DrawModule CreateDrawModule(GameObject gameObject, GameContext context)
        {
            return new W3dTreeDraw(this, context);
        }
    }
}
