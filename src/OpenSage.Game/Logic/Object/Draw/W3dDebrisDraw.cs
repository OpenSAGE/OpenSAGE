using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Client;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class W3dDebrisDraw : DrawModule
    {
        private readonly W3dDebrisDrawModuleData _data;
        private readonly GameContext _gameContext;
        private ModelInstance _modelInstance;

        private string _modelName;
        private uint _unknownInt1;
        private uint _unknownInt2;
        private bool _unknownBool;

        internal W3dDebrisDraw(W3dDebrisDrawModuleData data, GameContext context)
        {
            _data = data;
            _gameContext = context;
        }

        public void SetModelName(string modelName)
        {
            var model = _gameContext.AssetLoadContext.AssetStore.Models.GetByName(modelName);
            _modelInstance = AddDisposable(model.CreateInstance(_gameContext.AssetLoadContext));
            _modelInstance.Update(TimeInterval.Zero);
        }

        public override IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; } = Array.Empty<BitArray<ModelConditionFlag>>();

        internal override void BuildRenderList(
                RenderList renderList,
                Camera camera,
                bool castsShadow,
                MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS,
                Dictionary<string, bool> shownSubObjects = null,
                Dictionary<string, bool> hiddenSubObjects = null)
        {
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

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            reader.ReadAsciiString(ref _modelName);

            reader.SkipUnknownBytes(7);

            _unknownInt1 = reader.ReadUInt32();
            _unknownInt2 = reader.ReadUInt32();
            reader.ReadBoolean(ref _unknownBool);
        }
    }

    /// <summary>
    /// Special-case draw module used by ObjectCreationList.INI when using the CreateDebris code 
    /// which defaults to calling the GenericDebris object definition as a template for each debris 
    /// object generated.
    /// </summary>
    public sealed class W3dDebrisDrawModuleData : DrawModuleData
    {
        internal static W3dDebrisDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<W3dDebrisDrawModuleData> FieldParseTable = new IniParseTable<W3dDebrisDrawModuleData>();

        internal override DrawModule CreateDrawModule(Drawable drawable, GameContext context)
        {
            return new W3dDebrisDraw(this, context);
        }
    }
}
